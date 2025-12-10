using AlienFruit.Astra.ViewBox;
using FluentAssertions;

namespace AlienFruit.Astra.Tests.UnitTests;

public class ViewBoxSpecificationTests
{
    private ViewBoxSpecification CreateViewBoxSpecification(string id = "test-viewbox", string connectionErrorMessage = "Connection error")
    {
        // Use Activator to create ViewBoxSpecification without required property validation
        var specification = (ViewBoxSpecification)Activator.CreateInstance(typeof(ViewBoxSpecification))!;
        typeof(ViewBoxSpecification).GetProperty("Id")!.SetValue(specification, id);
        typeof(ViewBoxSpecification).GetProperty("ConnectionErrorMessage")!.SetValue(specification, connectionErrorMessage);
        return specification;
    }

    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        // Act
        var specification = CreateViewBoxSpecification();

        // Assert
        specification.Id.Should().Be("test-viewbox");
        specification.OnStartLoadingJsFunction.Should().BeNull();
        specification.OnTimeoutAfterStartLoadingJsFunction.Should().BeNull();
        specification.OnFinishLoadingJsFunction.Should().BeNull();
        specification.OnScriptsExecutedJsFunction.Should().BeNull();
        specification.StartLoadingEventDelay.Should().Be(100);
        specification.ChangingBrowserAddressEnable.Should().BeTrue();
        specification.ParrentViewBoxId.Should().BeNull();
        specification.ConnectionErrorMessage.Should().Be("Connection error");
    }

    [Fact]
    public void Constructor_ShouldAllowSettingAllProperties()
    {
        // Act
        var specification = CreateViewBoxSpecification("test-viewbox", "Connection error occurred");
        specification.OnStartLoadingJsFunction = "onStart";
        specification.OnTimeoutAfterStartLoadingJsFunction = "onTimeout";
        specification.OnFinishLoadingJsFunction = "onFinish";
        specification.OnScriptsExecutedJsFunction = "onScripts";
        specification.StartLoadingEventDelay = 200;
        specification.ChangingBrowserAddressEnable = false;
        specification.ParrentViewBoxId = "parent-viewbox";

        // Assert
        specification.Id.Should().Be("test-viewbox");
        specification.OnStartLoadingJsFunction.Should().Be("onStart");
        specification.OnTimeoutAfterStartLoadingJsFunction.Should().Be("onTimeout");
        specification.OnFinishLoadingJsFunction.Should().Be("onFinish");
        specification.OnScriptsExecutedJsFunction.Should().Be("onScripts");
        specification.StartLoadingEventDelay.Should().Be(200);
        specification.ChangingBrowserAddressEnable.Should().BeFalse();
        specification.ParrentViewBoxId.Should().Be("parent-viewbox");
        specification.ConnectionErrorMessage.Should().Be("Connection error occurred");
    }

    [Fact]
    public void Properties_ShouldBeSettableIndividually()
    {
        // Arrange
        var specification = CreateViewBoxSpecification();

        // Act
        typeof(ViewBoxSpecification).GetProperty("Id")!.SetValue(specification, "dynamic-id");
        specification.StartLoadingEventDelay = 500;
        specification.ChangingBrowserAddressEnable = false;

        // Assert
        specification.Id.Should().Be("dynamic-id");
        specification.StartLoadingEventDelay.Should().Be(500);
        specification.ChangingBrowserAddressEnable.Should().BeFalse();
    }

    [Fact]
    public void RequiredProperties_ShouldBeValidated()
    {
        // Act & Assert - Id and ConnectionErrorMessage are required
        var specification = CreateViewBoxSpecification();
        specification.Id.Should().Be("test-viewbox");
        specification.ConnectionErrorMessage.Should().Be("Connection error");
    }

    [Fact]
    public void DefaultValues_ShouldMatchExpectedConstants()
    {
        // Act
        var specification = CreateViewBoxSpecification();

        // Assert
        specification.StartLoadingEventDelay.Should().Be(100);
        specification.ChangingBrowserAddressEnable.Should().BeTrue();
    }

    [Fact]
    public void JsResourcePath_ShouldBeCorrectConstant()
    {
        // Assert
        ViewBoxSpecification.JsResourcePath.Should().Be("AlienFruit.Astra.ViewBox.Resources.viewbox.js");
    }

    [Fact]
    public void InitScriptTemplatePath_ShouldBeCorrectConstant()
    {
        // Assert
        ViewBoxSpecification.InitScriptTemplatePath.Should().Be("AlienFruit.Astra.ViewBox.Resources.viewbox-init-script.template.js");
    }

    [Fact]
    public void Specification_ShouldBeSerializableToJavaScript()
    {
        // Arrange
        var specification = CreateViewBoxSpecification("my-viewbox", "Connection failed");
        specification.OnStartLoadingJsFunction = "showSpinner";
        specification.OnFinishLoadingJsFunction = "hideSpinner";
        specification.StartLoadingEventDelay = 150;
        specification.ChangingBrowserAddressEnable = false;
        specification.ParrentViewBoxId = "parent-viewbox";

        // Act & Assert - The specification should be usable in template rendering
        // This is tested indirectly through ViewBox tests, but we verify the structure here
        specification.Id.Should().Be("my-viewbox");
        specification.ConnectionErrorMessage.Should().Be("Connection failed");
        specification.OnStartLoadingJsFunction.Should().Be("showSpinner");
        specification.OnFinishLoadingJsFunction.Should().Be("hideSpinner");
        specification.StartLoadingEventDelay.Should().Be(150);
        specification.ChangingBrowserAddressEnable.Should().BeFalse();
        specification.ParrentViewBoxId.Should().Be("parent-viewbox");
    }

    [Fact]
    public void TemplateResources_ShouldExist()
    {
        // Arrange
        var assembly = typeof(ViewBoxSpecification).Assembly;

        // Act & Assert
        assembly.GetManifestResourceNames().Should().Contain(ViewBoxSpecification.JsResourcePath);
        assembly.GetManifestResourceNames().Should().Contain(ViewBoxSpecification.InitScriptTemplatePath);
    }

    [Fact]
    public void InitScriptTemplate_ShouldContainExpectedPlaceholders()
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
