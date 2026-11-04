// THROWAWAY - DO NOT MERGE. Deliberately vulnerable code to check that the CodeQL results check
// on a PR reflects C# alerts (tools/pr-gates). Controllers are not registered in Program.cs, so
// this is unreachable at runtime; CodeQL still treats MVC action parameters as remote input.
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace HT.Ui;

[ApiController]
public sealed class CodeQlDemoController : ControllerBase
{
    // cs/command-line-injection: request input becomes process arguments.
    [HttpGet("/codeql-demo/run")]
    public IActionResult Run(string args)
    {
        using var process = Process.Start("sh", "-c " + args);
        return Ok(process?.Id);
    }

    // cs/path-injection: request input becomes a file path.
    [HttpGet("/codeql-demo/read")]
    public IActionResult Read(string path) => Content(System.IO.File.ReadAllText(path));
}
