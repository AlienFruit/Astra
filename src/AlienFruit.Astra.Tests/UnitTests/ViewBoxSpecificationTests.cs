using AlienFruit.Astra.ViewBox;
using FluentAssertions;

namespace AlienFruit.Astra.Tests.UnitTests;

public class ViewBoxSpecificationTests
{

    [Fact]
    public void Create_ObjectInitializer_DefaultValuesApplied()
    {
        // Act
        var specification = new ViewBoxSpecification
        {
            Id = "test-viewbox",
            ConnectionErrorMessage = "Connection error"
        };

        // Assert
        specification.OnStartLoadingJsFunction.Should().BeNull();
        specification.OnTimeoutAfterStartLoadingJsFunction.Should().BeNull();
        specification.OnFinishLoadingJsFunction.Should().BeNull();
        specification.OnScriptsExecutedJsFunction.Should().BeNull();
        specification.StartLoadingEventDelay.Should().Be(100);
        specification.ChangingBrowserAddressEnable.Should().BeTrue();
        specification.ParrentViewBoxId.Should().BeNull();
    }

    [Fact]
    public void TemplateResources_Get_ResourcesExistInAssembly()
    {
        // Arrange
        var assembly = typeof(ViewBoxSpecification).Assembly;

        // Act & Assert
        assembly.GetManifestResourceNames().Should().Contain(ViewBoxSpecification.JsResourcePath);
        assembly.GetManifestResourceNames().Should().Contain(ViewBoxSpecification.InitScriptTemplatePath);
    }

    [Fact]
    public void InitScriptTemplate_Read_ContainsRequiredPlaceholders()
    {
        // Arrange
        var assembly = typeof(ViewBoxSpecification).Assembly;
        using var stream = assembly.GetManifestResourceStream(ViewBoxSpecification.InitScriptTemplatePath);
        using var reader = new StreamReader(stream!);
        var templateContent = reader.ReadToEnd();

        // Act & Assert
        templateContent.Should().Contain("{{ Id }}");
        templateContent.Should().Contain("{{ ChangingBrowserAddressEnable }}");
        templateContent.Should().Contain("{{ StartLoadingEventDelay }}");
        templateContent.Should().Contain("{{ ConnectionErrorMessage }}");
        templateContent.Should().Contain("{{ ParrentViewBoxId }}");
        templateContent.Should().Contain("{{ OnStartLoadingJsFunction }}");
        templateContent.Should().Contain("{{ OnTimeoutAfterStartLoadingJsFunction }}");
        templateContent.Should().Contain("{{ OnFinishLoadingJsFunction }}");
        templateContent.Should().Contain("{{ OnScriptsExecutedJsFunction }}");
    }
}
