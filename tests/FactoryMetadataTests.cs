using System.Text.RegularExpressions;
using FluentAssertions;
using Xunit;

namespace PDT.NecDisplay.EPI.Tests;

public class FactoryMetadataTests
{
    // Matches the pinned prerelease this plugin actually targets - NOT a bare "3.0.0", which
    // doesn't exist as a shipped Essentials version.
    private const string ExpectedMinimumEssentialsFrameworkVersion = "3.0.0-rc.1";

    [Theory]
    [InlineData("NecDisplayFactory")]
    public void Factory_Source_Sets_MinimumEssentialsFrameworkVersion(string factoryName)
    {
        var source = AssemblyFixture.FindSourceForClass(factoryName);
        source.Should().NotBeNull($"source for {factoryName} should be found under src/");

        source!.Should().Contain(
            $"MinimumEssentialsFrameworkVersion = \"{ExpectedMinimumEssentialsFrameworkVersion}\"");
    }

    [Theory]
    [InlineData("NecDisplayFactory")]
    public void Factory_Source_Sets_TypeNames(string factoryName)
    {
        var source = AssemblyFixture.FindSourceForClass(factoryName);
        source.Should().NotBeNull();
        source!.Should().Contain("TypeNames = new List<string>");
    }

    [Theory]
    [InlineData("NecDisplayFactory", "necDisplay")]
    [InlineData("NecDisplayFactory", "necmpsx")]
    public void Factory_Source_Contains_TypeName(string factoryName, string typeName)
    {
        var source = AssemblyFixture.FindSourceForClass(factoryName);
        source.Should().NotBeNull();
        source!.Should().Contain($"\"{typeName}\"");
    }

    [Fact]
    public void No_Duplicate_TypeNames_Across_Factory_Sources()
    {
        var factoryNames = new[] { "NecDisplayFactory" };
        var allTypeNames = new List<string>();

        foreach (var factoryName in factoryNames)
        {
            var source = AssemblyFixture.FindSourceForClass(factoryName);
            source.Should().NotBeNull();

            var match = Regex.Match(source!, @"TypeNames\s*=\s*new List<string>\s*\{([^}]*)\}");
            match.Success.Should().BeTrue($"{factoryName} should assign TypeNames in its constructor");

            var names = Regex.Matches(match.Groups[1].Value, "\"([^\"]+)\"")
                .Select(m => m.Groups[1].Value);
            allTypeNames.AddRange(names);
        }

        allTypeNames.Should().OnlyHaveUniqueItems();
    }
}
