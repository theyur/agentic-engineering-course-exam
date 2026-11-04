<#
.SYNOPSIS
  Report every Copilot review thread's state after a re-review, and resolve those Copilot itself
  confirmed as fixed but left open. Dry run by default.

.DESCRIPTION
  Copilot resolves threads natively during a re-review when it judges them fixed. This script is
  the check and the fallback. For each unresolved Copilot thread it applies Get-ThreadResolution
  (PrGates.psm1). A thread resolves only when two things agree. First, its last comment is a
  repository writer's "Fixed in <sha>" reply, and <sha> is in the PR head after the commented
  commit. Second, Copilot's latest review is on the current head and raised no new thread on the
  same file. On top of that, the overview must list the finding under a "resolved" section (the
  default, strict mode). With -AcceptApproval, an "Approval recommended" verdict is enough instead,
  provided the fix touches the thread's file or the thread is outdated. That covers PR #13, where
  Copilot approved and still listed the fixed findings as open. Every other thread stays open with
  a reason.

  -Apply resolves the RESOLVE threads with resolveReviewThread (resolutionReason ADDRESSED),
  re-checking the head SHA right before. Already-resolved threads are never touched, so
  re-running is a no-op.
  Without -Apply nothing is written. -IncludeResolved (dry run only) also evaluates resolved
  threads, to replay the rules against a finished PR.

.EXAMPLE
  pwsh -NoProfile -File tools/pr-gates/pr-resolve.ps1 -Pr 13
  pwsh -NoProfile -File tools/pr-gates/pr-resolve.ps1 -Pr 14 -AcceptApproval -Apply

.OUTPUTS
  JSON report. Exit codes: 0 evaluated (and applied), 10 no usable re-review (none / not on head /
  unrecognized overview), 20 Copilot re-review pending, 30 STALE (head moved), 1 error or
  incomplete data.
#>
param(
    [int]$Pr,
    [string]$Repo,
    [switch]$Apply,
    # Also resolve when Copilot's verdict on the head is "Approval recommended" and it did not
    # re-raise the finding, even if its overview still lists the finding as open (PR #13).
    [switch]$AcceptApproval,
    [switch]$IncludeResolved
)
$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot 'PrGates.psm1') -Force

try {
    if ($Apply -and $IncludeResolved) { throw '-IncludeResolved is for dry runs only.' }
    if (-not $Repo) { $Repo = Get-RepoSlug }
    $info = Get-PrInfo $Repo $Pr
    $Pr = $info.number
    $head = $info.headRefOid
    if ($Apply -and $info.state -ne 'OPEN') { throw "PR #$Pr is $($info.state); nothing to resolve." }
    $owner, $name = $Repo -split '/'

    # Copilot reviews with their state: Get-CopilotReReviewState ignores DISMISSED / PENDING ones.
    $readReviews = {
        @(Invoke-GhList "repos/$Repo/pulls/$Pr/reviews?per_page=100" | Where-Object { Test-CopilotLogin $_.user.login } |
            ForEach-Object { [pscustomobject]@{ id = $_.id; state = $_.state; commit = $_.commit_id; submittedAt = $_.submitted_at; body = $_.body } })
    }
    # Timeline events that mean a Copilot re-review is on its way.
    $readEvents = {
        @(Invoke-GhList "repos/$Repo/issues/$Pr/events?per_page=100" |
            Where-Object { $_.event -in 'review_requested', 'copilot_work_started' } |
            ForEach-Object { [pscustomobject]@{ event = $_.event; createdAt = $_.created_at
                    reviewer = $(if ($_.PSObject.Properties['requested_reviewer'] -and $_.requested_reviewer) { $_.requested_reviewer.login } else { '' }) } })
    }
    $reviews = & $readReviews
    $reReview = Get-CopilotReReviewState -Reviews $reviews -Events (& $readEvents) -HeadSha $head

    $query = @'
query($owner: String!, $name: String!, $number: Int!) {
  repository(owner: $owner, name: $name) {
    pullRequest(number: $number) {
      reviewThreads(first: 100) {
        pageInfo { hasNextPage }
        nodes {
          id isResolved isOutdated viewerCanResolve path
          comments(first: 50) {
            pageInfo { hasNextPage }
            nodes {
              databaseId author { __typename login } body createdAt updatedAt
              pullRequestReview { databaseId } originalCommit { oid }
            }
          }
        }
      }
    }
  }
}
'@
    $data = Invoke-Gh @('api', 'graphql', '-f', "query=$query", '-F', "owner=$owner", '-F', "name=$name", '-F', "number=$Pr") | ConvertFrom-Json
    $rt = $data.data.repository.pullRequest.reviewThreads
    # Partial data could hide a later Copilot reply or a re-raised thread: refuse rather than guess.
    if ($rt.pageInfo.hasNextPage) { throw 'INCOMPLETE: more than 100 review threads; not evaluating.' }
    $threads = @($rt.nodes | ForEach-Object {
            if ($_.comments.pageInfo.hasNextPage) { throw "INCOMPLETE: thread $($_.id) has more than 50 comments; not evaluating." }
            [pscustomobject]@{
                id = $_.id; isResolved = $_.isResolved; isOutdated = $_.isOutdated; viewerCanResolve = $_.viewerCanResolve; path = $_.path
                originalCommit = $(($_.comments.nodes | Select-Object -First 1).originalCommit.oid)
                commentCount = @($_.comments.nodes).Count
                lastCommentId = $(($_.comments.nodes | Select-Object -Last 1).databaseId)
                lastCommentUpdatedAt = $("$(($_.comments.nodes | Select-Object -Last 1).updatedAt)")
                lastCommentBody = $(($_.comments.nodes | Select-Object -Last 1).body)
                comments =@($_.comments.nodes | ForEach-Object {
                        [pscustomobject]@{ databaseId = $_.databaseId; author = $(if ($_.author) { $_.author.login } else { '' })
                            authorType = $(if ($_.author) { $_.author.__typename } else { '' }); canPush = $false
                            body = $_.body; createdAt = $_.createdAt
                            reviewId = $(if ($_.pullRequestReview) { $_.pullRequestReview.databaseId } else { $null }) }
                    })
            }
        })

    # Actual repository permission of whoever wrote each thread's last comment (the only comment
    # whose author matters). authorAssociation is not a permission: MEMBER / COLLABORATOR can be
    # read-only. A failed lookup counts as no permission.
    $pushCache = @{}
    foreach ($t in $threads) {
        $last = @($t.comments) | Select-Object -Last 1
        if (-not $last -or $last.authorType -ne 'User' -or -not $last.author) { continue }
        if (-not $pushCache.ContainsKey($last.author)) {
            $pushCache[$last.author] = try {
                (Invoke-Gh @('api', "repos/$Repo/collaborators/$($last.author)/permission", '--jq', '.user.permissions.push')).Trim() -eq 'true'
            } catch { $false }
        }
        $last.canPush = $pushCache[$last.author]
    }

    # Commit facts via the compare API, cached per (sha, commented commit).
    $cache = @{}
    $commitInfo = {
        param([string]$Sha, $Thread)
        $key = "$Sha|$($Thread.originalCommit)"
        if ($cache.ContainsKey($key)) { return $cache[$key] }
        $r = [pscustomobject]@{ found = $false; sha = $Sha; inHead = $false; afterFinding = $false; files = @() }
        try {
            $c = Invoke-Gh @('api', "repos/$Repo/commits/$Sha") | ConvertFrom-Json
            $r.found = $true; $r.sha = $c.sha; $r.files = @($c.files | ForEach-Object filename)
            # compare A...B status: 'identical' or 'ahead' means A is an ancestor of (or equal to) B.
            $toHead = (Invoke-Gh @('api', "repos/$Repo/compare/$($c.sha)...$head", '--jq', '.status')).Trim()
            $r.inHead = $toHead -in 'ahead', 'identical'
            if ($Thread.originalCommit) {
                $fromFinding = (Invoke-Gh @('api', "repos/$Repo/compare/$($Thread.originalCommit)...$($c.sha)", '--jq', '.status')).Trim()
                $r.afterFinding = $fromFinding -eq 'ahead'
            }
        } catch { $r | Add-Member error $_.Exception.Message }   # not found / unreachable: stays not-in-head -> KEEP
        $cache[$key] = $r
        $r
    }

    $overview = if ($reReview.review) { Get-CopilotOverviewStatus $reReview.review.body } else { @{} }
    $copilotThreads =@($threads | Where-Object { Test-CopilotLogin (@($_.comments) | Select-Object -First 1).author })
    $decisions = @($copilotThreads | ForEach-Object {
            $t = $_
            if ($IncludeResolved -and $t.isResolved) { $t = $t.PSObject.Copy(); $t.isResolved = $false }
            $d = Get-ThreadResolution -Thread $t -ReReview $reReview -Threads $threads -CommitInfo $commitInfo -AcceptApproval:$AcceptApproval
            if ($d.decision -eq 'RESOLVE' -and -not $_.viewerCanResolve -and -not $IncludeResolved) {
                $d.decision = 'KEEP'; $d.reason = "Eligible, but this token cannot resolve the thread. $($d.reason)"
            }
            $firstId = "$((@($_.comments) | Select-Object -First 1).databaseId)"
            $d | Add-Member copilotStatus $(if ($overview.ContainsKey($firstId)) { $overview[$firstId] } else { 'not listed' })
            $d | Add-Member wasResolved $_.isResolved
            $d
        })

    # The head can move while we read: decisions for an old head must not be applied.
    $headNow = (Get-PrInfo $Repo $Pr).headRefOid
    if ($headNow -ne $head) {
        [Console]::Error.WriteLine("STALE: head moved from $head to $headNow while evaluating; nothing resolved. Re-run.")
        exit 30
    }

    $resolved = @()
    $skipped = @()
    if ($Apply) {
        $mutation = 'mutation($id: ID!) { resolveReviewThread(input: { threadId: $id, resolutionReason: ADDRESSED }) { thread { id isResolved } } }'
        $reread = 'query($id: ID!) { node(id: $id) { ... on PullRequestReviewThread { isResolved comments(last: 1) { totalCount nodes { databaseId updatedAt body } } } } }'
        $authorizing = if ($reReview.review) { "$($reReview.review.id)" } else { '' }
        foreach ($d in @($decisions | Where-Object decision -eq 'RESOLVE')) {
            # Everything the decision rested on, read again right before the mutation.
            if ((Get-PrInfo $Repo $Pr).headRefOid -ne $head) {
                [Console]::Error.WriteLine("STALE: head moved during apply; stopped after $($resolved.Count) thread(s).")
                exit 30
            }
            # Reviews AND timeline events: a re-review requested or started since the snapshot means
            # the authorizing review is about to be superseded.
            $stateNow = Get-CopilotReReviewState -Reviews (& $readReviews) -Events (& $readEvents) -HeadSha $head
            if (-not (Test-ReReviewStillValid $authorizing $stateNow)) {
                [Console]::Error.WriteLine("STALE: Copilot re-review state changed during apply ($($stateNow.state)); stopped after $($resolved.Count) thread(s).")
                exit 30
            }
            $before = $threads | Where-Object id -eq $d.threadId | Select-Object -First 1
            $node = (Invoke-Gh @('api', 'graphql', '-f', "query=$reread", '-F', "id=$($d.threadId)") | ConvertFrom-Json).data.node
            $now = [pscustomobject]@{ isResolved = $node.isResolved; commentCount = $node.comments.totalCount
                lastCommentId = $(@($node.comments.nodes) | Select-Object -Last 1 | ForEach-Object databaseId)
                lastCommentUpdatedAt = $("$(@($node.comments.nodes) | Select-Object -Last 1 | ForEach-Object updatedAt)")
                lastCommentBody = $(@($node.comments.nodes) | Select-Object -Last 1 | ForEach-Object body) }
            if (-not (Test-ThreadUnchanged $before $now)) {
                $d.decision = 'KEEP'; $d.reason = 'Thread changed after evaluation (new comment or resolved meanwhile); left alone - re-run.'
                $skipped += $d.threadId
                continue
            }
            $null = Invoke-Gh @('api', 'graphql', '-f', "query=$mutation", '-F', "id=$($d.threadId)")
            $resolved += $d.threadId
        }
    }

    [pscustomobject]@{
        repo = $Repo; pr = $Pr; headSha = $head; applied = [bool]$Apply
        mode = $(if ($AcceptApproval) { 'approval' } else { 'strict' })
        reReview = [pscustomobject]@{ state = $reReview.state; reason = $reReview.reason
            review = $(if ($reReview.review) { [pscustomobject]@{ id = $reReview.review.id; commit = $reReview.review.commit; submittedAt = $reReview.review.submittedAt; verdict = (Get-CopilotVerdict $reReview.review.body) } }) }
        resolved = $resolved
        changedDuringApply = $skipped
        threads = $decisions
    } | ConvertTo-Json -Depth 6

    switch ($reReview.state) {
        'READY' { exit 0 }
        'PENDING' { exit 20 }
        default { exit 10 }
    }
} catch {
    [Console]::Error.WriteLine("pr-resolve: $($_.Exception.Message)")
    exit 1
}
