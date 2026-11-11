using System.Net;
using System.Security.Claims;
using System.Text.RegularExpressions;
using HT.Application.Accounts;
using HT.Domain.Accounts;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HT.UiTests.Host;

/// <summary>A form read from a page: its antiforgery token and its handler name.</summary>
public sealed record FormPage(HttpStatusCode Status, string Html, string Token, string? Handler);

/// <summary>An antiforgery cookie (name and value) and the request token that goes with it.</summary>
public sealed record AntiforgeryTokens(string CookieName, string CookieToken, string RequestToken);

/// <summary>Helpers for host tests (T049).</summary>
public static partial class HostClient
{
    /// <summary>GETs a page and reads its form's antiforgery token and <c>_handler</c> name.</summary>
    public static async Task<FormPage> GetFormAsync(this HttpClient client, string path, string? handler = null)
    {
        using var response = await client.GetAsync(new Uri(path, UriKind.Relative));
        var html = await response.Content.ReadAsStringAsync();

        var token = TokenPattern().Match(html);
        if (!token.Success)
        {
            throw new InvalidOperationException($"No antiforgery token on {path}.");
        }

        var handlers = HandlerPattern().Matches(html).Select(m => WebUtility.HtmlDecode(m.Groups["value"].Value)).ToList();
        var chosen = handler ?? handlers.FirstOrDefault();
        if (handler is not null && !handlers.Contains(handler))
        {
            throw new InvalidOperationException($"No form '{handler}' on {path}.");
        }

        return new FormPage(response.StatusCode, html, WebUtility.HtmlDecode(token.Groups["value"].Value), chosen);
    }

    /// <summary>POSTs the form's fields with its token and handler name.</summary>
    public static async Task<HttpResponseMessage> PostFormAsync(
        this HttpClient client, string path, FormPage form, IEnumerable<KeyValuePair<string, string>> fields)
    {
        ArgumentNullException.ThrowIfNull(form);

        var values = new List<KeyValuePair<string, string>> { new("__RequestVerificationToken", form.Token) };
        if (form.Handler is not null)
        {
            values.Add(new("_handler", form.Handler));
        }

        values.AddRange(fields);
        using var content = new FormUrlEncodedContent(values);
        return await client.PostAsync(new Uri(path, UriKind.Relative), content);
    }

    /// <summary>The attributes of the <c>Set-Cookie</c> header for this cookie, or null when it is not set.</summary>
    public static IReadOnlyList<string>? SetCookieAttributes(this HttpResponseMessage response, string cookieName)
    {
        ArgumentNullException.ThrowIfNull(response);
        if (!response.Headers.TryGetValues("Set-Cookie", out var headers))
        {
            return null;
        }

        var header = headers.FirstOrDefault(h => h.StartsWith(cookieName + "=", StringComparison.Ordinal));
        return header?.Split(';').Select(part => part.Trim()).ToList();
    }

    /// <summary>
    /// Creates an account directly through the host's account store, so a story's host tests do not depend on
    /// another story's page.
    /// </summary>
    public static async Task<HT.Domain.Accounts.Account> CreateAccountAsync(
        this HostFixture host, string email, string password, Role role = Role.RegisteredUser)
    {
        ArgumentNullException.ThrowIfNull(host);
        if (!Email.TryParse(email, out var parsed))
        {
            throw new ArgumentException("Not a valid email.", nameof(email));
        }

        await using var scope = host.Services.CreateAsyncScope();
        var store = scope.ServiceProvider.GetRequiredService<IAccountStore>();
        var outcome = await store.CreateAsync(parsed, role, password, CancellationToken.None);
        return outcome.Account ?? throw new InvalidOperationException($"The email {email} already has an account.");
    }

    /// <summary>
    /// Antiforgery tokens for a signed-in account. A request token is bound to its user, and until SiteNav (T092)
    /// no page renders a form for a signed-in user, so the test asks the host's antiforgery service for one, as
    /// the account's own cookie would identify it (its user id claim).
    /// </summary>
    public static AntiforgeryTokens SignedInAntiforgery(this HostFixture host, AccountId accountId)
    {
        ArgumentNullException.ThrowIfNull(host);
        using var scope = host.Services.CreateScope();
        var context = new DefaultHttpContext
        {
            RequestServices = scope.ServiceProvider,
            User = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, accountId.Value.ToString())],
                IdentityConstants.ApplicationScheme)),
        };
        var tokens = scope.ServiceProvider.GetRequiredService<IAntiforgery>().GetTokens(context);
        var cookieName = scope.ServiceProvider.GetRequiredService<IOptions<AntiforgeryOptions>>().Value.Cookie.Name
            ?? throw new InvalidOperationException("The antiforgery cookie has no name.");
        return new AntiforgeryTokens(
            cookieName,
            tokens.CookieToken ?? throw new InvalidOperationException("No antiforgery cookie token."),
            tokens.RequestToken ?? throw new InvalidOperationException("No antiforgery request token."));
    }

    /// <summary>Deletes the <c>AspNetUsers</c> rows with this user name.</summary>
    public static async Task DeleteUserAsync(string connectionString, string userName)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand("DELETE FROM [AspNetUsers] WHERE [UserName] = @userName", connection);
        command.Parameters.AddWithValue("@userName", userName);
        await command.ExecuteNonQueryAsync();
    }

    /// <summary>How many <c>AspNetUsers</c> rows have this user name.</summary>
    public static async Task<int> CountUsersAsync(string connectionString, string userName)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand("SELECT COUNT(*) FROM [AspNetUsers] WHERE [UserName] = @userName", connection);
        command.Parameters.AddWithValue("@userName", userName);
        return (int)(await command.ExecuteScalarAsync())!;
    }

    /// <summary>True when a table with this name exists in the database.</summary>
    public static async Task<bool> TableExistsAsync(string connectionString, string table)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand("SELECT OBJECT_ID(@table, N'U')", connection);
        command.Parameters.AddWithValue("@table", table);
        return await command.ExecuteScalarAsync() is not (null or DBNull);
    }

    [GeneratedRegex("""name="__RequestVerificationToken"[^>]*value="(?<value>[^"]*)"|value="(?<value>[^"]*)"[^>]*name="__RequestVerificationToken"\b""")]
    private static partial Regex TokenPattern();

    [GeneratedRegex("""name="_handler"[^>]*value="(?<value>[^"]*)"|value="(?<value>[^"]*)"[^>]*name="_handler"\b""")]
    private static partial Regex HandlerPattern();
}
