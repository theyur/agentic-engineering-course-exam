using Microsoft.AspNetCore.Antiforgery;

namespace HT.Ui.Account;

/// <summary><c>POST /account/sign-out</c>: ends the cookie in this browser and redirects to Home (HP-006).</summary>
internal static class SignOutEndpoint
{
    public const string Path = "/account/sign-out";

    public static IEndpointConventionBuilder MapSignOut(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPost(Path, SignOutAsync);

    // The endpoint reads no form fields, so the framework would not validate the token by itself: check it here.
    private static async Task<IResult> SignOutAsync(HttpContext context, IAntiforgery antiforgery, AccountSession session)
    {
        if (!await antiforgery.IsRequestValidAsync(context))
        {
            return TypedResults.BadRequest();
        }

        await session.SignOutAsync();
        return TypedResults.LocalRedirect("/");
    }
}
