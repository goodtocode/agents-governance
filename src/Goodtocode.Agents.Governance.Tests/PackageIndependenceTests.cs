using System.Reflection;

namespace Goodtocode.Agents.Governance.Tests;

[TestClass]
public sealed class PackageIndependenceTests
{
    [TestMethod]
    public void GovernanceDoesNotReferencePlaybook()
    {
        var references = typeof(Governance.Application.IRepeatPolicy).Assembly.GetReferencedAssemblies();

        Assert.IsFalse(references.Any(r => r.Name!.Contains("Playbook", StringComparison.OrdinalIgnoreCase)));
    }
}
