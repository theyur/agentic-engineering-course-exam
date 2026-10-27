namespace HT.Persistence;

// Deliberately referenced by a Blazor page to demonstrate a forbidden dependency.
public sealed class DependencyViolationDemo
{
    public string GetGreeting() => "Hello from Persistence (intentional architecture violation).";
}
