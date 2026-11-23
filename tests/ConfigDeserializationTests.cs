using FluentAssertions;
using Xunit;

namespace PDT.NecDisplay.EPI.Tests;

public class ConfigDeserializationTests
{
    private static Type GetConfigType(string name) =>
        AssemblyFixture.PluginAssembly.GetTypes().Single(t => t.Name == name);

    [Theory]
    [InlineData("NecDisplayConfigObject")]
    [InlineData("FriendlyName")]
    public void Config_Class_Exists(string className)
    {
        AssemblyFixture.PluginAssembly.GetTypes()
            .Should().Contain(t => t.Name == className);
    }

    [Theory]
    [InlineData("NecDisplayConfigObject")]
    [InlineData("FriendlyName")]
    public void Config_Has_Parameterless_Constructor(string className)
    {
        var type = GetConfigType(className);
        type.GetConstructor(Type.EmptyTypes).Should().NotBeNull();
    }

    [Theory]
    [InlineData("NecDisplayConfigObject", "Id", "id")]
    [InlineData("NecDisplayConfigObject", "WarmupTime", "warmupTime")]
    [InlineData("NecDisplayConfigObject", "CooldownTime", "cooldownTime")]
    [InlineData("NecDisplayConfigObject", "FriendlyNames", "friendlyNames")]
    [InlineData("FriendlyName", "InputKey", "inputKey")]
    [InlineData("FriendlyName", "Name", "name")]
    [InlineData("FriendlyName", "HideInput", "hideInput")]
    public void Config_Property_Has_JsonPropertyAttribute(string className, string propertyName, string jsonName)
    {
        var type = GetConfigType(className);
        var property = type.GetProperty(propertyName);
        property.Should().NotBeNull($"{className} should declare property {propertyName}");

        var hasAttribute = property!.CustomAttributes.Any(a =>
            a.AttributeType.Name == "JsonPropertyAttribute"
            && a.ConstructorArguments.Any(arg =>
                string.Equals(arg.Value?.ToString(), jsonName, StringComparison.Ordinal)));

        hasAttribute.Should().BeTrue(
            $"{className}.{propertyName} should have [JsonProperty(\"{jsonName}\")]");
    }
}
