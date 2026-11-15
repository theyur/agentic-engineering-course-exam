using System.Collections.Concurrent;
using System.Net;
using System.Text;

namespace HT.UiTests.SmokeChecks;

/// <summary>
/// A hand-written fake site for the smoke check (ADR 0006: no mocking library), with no network. Responses are
/// scripted per method and absolute URL without the query; a URL's last scripted response repeats, and anything
/// unscripted is 404. Every request is recorded.
/// </summary>
public sealed class ScriptedHandler : HttpMessageHandler
{
    private readonly ConcurrentDictionary<string, Queue<Func<HttpResponseMessage>>> _script = new();
    private readonly ConcurrentQueue<string> _requests = new();

    /// <summary>Every request so far, as <c>METHOD absolute-url</c>.</summary>
    public IReadOnlyList<string> Requests => [.. _requests];

    /// <summary>Scripts the responses of one method and URL, in order; the last one repeats.</summary>
    public ScriptedHandler On(HttpMethod method, string url, params Func<HttpResponseMessage>[] responses)
    {
        ArgumentNullException.ThrowIfNull(method);
        _script[Key(method, new Uri(url))] = new Queue<Func<HttpResponseMessage>>(responses);
        return this;
    }

    /// <summary>A 200 response with this HTML.</summary>
    public static Func<HttpResponseMessage> Html(string html) =>
        () => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(html, Encoding.UTF8, "text/html") };

    /// <summary>A response with this status and no content.</summary>
    public static Func<HttpResponseMessage> Status(HttpStatusCode status) => () => new HttpResponseMessage(status);

    /// <summary>A redirect with this status to this location.</summary>
    public static Func<HttpResponseMessage> Redirect(HttpStatusCode status, string location) =>
        () => new HttpResponseMessage(status) { Headers = { Location = new Uri(location, UriKind.RelativeOrAbsolute) } };

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var uri = request.RequestUri ?? throw new InvalidOperationException("A request without a URI.");
        _requests.Enqueue($"{request.Method} {uri.AbsoluteUri}");

        if (!_script.TryGetValue(Key(request.Method, uri), out var responses))
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound) { RequestMessage = request });
        }

        Func<HttpResponseMessage> next;
        lock (responses)
        {
            next = responses.Count > 1 ? responses.Dequeue() : responses.Peek();
        }

        var response = next();
        response.RequestMessage = request;
        return Task.FromResult(response);
    }

    private static string Key(HttpMethod method, Uri uri) => $"{method} {uri.GetLeftPart(UriPartial.Path)}";
}
