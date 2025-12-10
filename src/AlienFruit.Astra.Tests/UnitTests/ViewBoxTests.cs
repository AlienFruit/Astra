using AlienFruit.Astra.Abstractions;
using AlienFruit.Astra.Core;
using AlienFruit.Astra.Models;
using AlienFruit.Astra.Tests.Infrastructure;
using AlienFruit.Astra.ViewBox;
using FluentAssertions;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Moq;

namespace AlienFruit.Astra.Tests.UnitTests;

public class ViewBoxTests : AstraTestBase
{
    private AlienFruit.Astra.ViewBox.ViewBox CreateViewBox(string id = "test-viewbox")
    {
        // Use Activator to create ViewBox without required property validation
        var viewBox = (AlienFruit.Astra.ViewBox.ViewBox)Activator.CreateInstance(
            typeof(AlienFruit.Astra.ViewBox.ViewBox),
            HtmlResourceRendererMock.Object,
            ResourceCompressorMock.Object)!;

        typeof(AlienFruit.Astra.ViewBox.ViewBox).GetProperty("Id")!.SetValue(viewBox, id);
        return viewBox;
    }

    [Fact]
    public void Constructor_ShouldInitializeWithDependencies()
    {
        // Arrange & Act
        var viewBox = CreateViewBox();

        // Assert
        viewBox.Should().NotBeNull();
    }

    [Fact]
    public void Process_WhenIdIsNull_ShouldThrowArgumentException()
    {
        // Arrange
        var viewBox = CreateViewBox();
        // Id is required, so we need to use reflection to set it to null for testing
        typeof(AlienFruit.Astra.ViewBox.ViewBox).GetProperty("Id")!.SetValue(viewBox, null);
        var context = CreateTagHelperContext("view-box");
        var output = CreateTagHelperOutput("view-box");

        // Act
        var act = () => viewBox.Process(context, output);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Attribute Id is required");
    }

    [Fact]
    public void Process_WhenIdIsEmpty_ShouldThrowArgumentException()
    {
        // Arrange
        var viewBox = CreateViewBox();
        viewBox.Id = "";
        var context = CreateTagHelperContext("view-box");
        var output = CreateTagHelperOutput("view-box");

        // Act
        var act = () => viewBox.Process(context, output);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Attribute Id is required");
    }

    [Fact]
    public void Process_WhenIdIsWhitespace_ShouldThrowArgumentException()
    {
        // Arrange
        var viewBox = CreateViewBox();
        viewBox.Id = "   ";
        var context = CreateTagHelperContext("view-box");
        var output = CreateTagHelperOutput("view-box");

        // Act
        var act = () => viewBox.Process(context, output);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Attribute Id is required");
    }

    [Fact]
    public void Process_WhenIdIsValid_ShouldSetIdAttribute()
    {
        // Arrange
        var viewBox = CreateViewBox();
        viewBox.Id = "test-viewbox";
        var context = CreateTagHelperContext("view-box");
        var output = CreateTagHelperOutput("view-box");

        SetupResourceCompressorCompressToString("Connection error message");

        // Act
        viewBox.Process(context, output);

        // Assert
        output.Attributes["id"].Value.Should().Be("test-viewbox");
    }

    [Fact]
    public void Process_WhenStyleIsSet_ShouldSetStyleAttribute()
    {
        // Arrange
        var viewBox = CreateViewBox();
        viewBox.Id = "test-viewbox";
        viewBox.Style = "color: red; font-size: 14px";
        var context = CreateTagHelperContext("view-box");
        var output = CreateTagHelperOutput("view-box");

        SetupResourceCompressorCompressToString("Connection error message");

        // Act
        viewBox.Process(context, output);

        // Assert
        output.Attributes["style"].Value.Should().Be("color: red; font-size: 14px");
    }

    [Fact]
    public void Process_WhenStyleIsEmpty_ShouldNotSetStyleAttribute()
    {
        // Arrange
        var viewBox = CreateViewBox();
        viewBox.Id = "test-viewbox";
        viewBox.Style = "";
        var context = CreateTagHelperContext("view-box");
        var output = CreateTagHelperOutput("view-box");

        SetupResourceCompressorCompressToString("Connection error message");

        // Act
        viewBox.Process(context, output);

        // Assert
        output.Attributes.ContainsName("style").Should().BeFalse();
    }

    [Fact]
    public void Process_WhenClassIsSet_ShouldSetClassAttribute()
    {
        // Arrange
        var viewBox = CreateViewBox();
        viewBox.Id = "test-viewbox";
        viewBox.Class = "container active";
        var context = CreateTagHelperContext("view-box");
        var output = CreateTagHelperOutput("view-box");

        SetupResourceCompressorCompressToString("Connection error message");

        // Act
        viewBox.Process(context, output);

        // Assert
        output.Attributes["class"].Value.Should().Be("container active");
    }

    [Fact]
    public void Process_WhenClassIsEmpty_ShouldNotSetClassAttribute()
    {
        // Arrange
        var viewBox = CreateViewBox();
        viewBox.Id = "test-viewbox";
        viewBox.Class = "";
        var context = CreateTagHelperContext("view-box");
        var output = CreateTagHelperOutput("view-box");

        SetupResourceCompressorCompressToString("Connection error message");

        // Act
        viewBox.Process(context, output);

        // Assert
        output.Attributes.ContainsName("class").Should().BeFalse();
    }

    [Fact]
    public void Process_WhenRoleIsSet_ShouldSetRoleAttribute()
    {
        // Arrange
        var viewBox = CreateViewBox();
        viewBox.Id = "test-viewbox";
        viewBox.Role = "main";
        var context = CreateTagHelperContext("view-box");
        var output = CreateTagHelperOutput("view-box");

        SetupResourceCompressorCompressToString("Connection error message");

        // Act
        viewBox.Process(context, output);

        // Assert
        output.Attributes["role"].Value.Should().Be("main");
    }

    [Fact]
    public void Process_WhenRoleIsEmpty_ShouldNotSetRoleAttribute()
    {
        // Arrange
        var viewBox = CreateViewBox();
        viewBox.Id = "test-viewbox";
        viewBox.Role = "";
        var context = CreateTagHelperContext("view-box");
        var output = CreateTagHelperOutput("view-box");

        SetupResourceCompressorCompressToString("Connection error message");

        // Act
        viewBox.Process(context, output);

        // Assert
        output.Attributes.ContainsName("role").Should().BeFalse();
    }

    [Fact]
    public void Process_WhenTagNameIsSet_ShouldUseCustomTagName()
    {
        // Arrange
        var viewBox = CreateViewBox();
        viewBox.Id = "test-viewbox";
        viewBox.TagName = "section";
        var context = CreateTagHelperContext("view-box");
        var output = CreateTagHelperOutput("view-box");

        SetupResourceCompressorCompressToString("Connection error message");

        // Act
        viewBox.Process(context, output);

        // Assert
        output.TagName.Should().Be("section");
    }

    [Fact]
    public void Process_WhenTagNameIsEmpty_ShouldUseDefaultMainTag()
    {
        // Arrange
        var viewBox = CreateViewBox();
        viewBox.Id = "test-viewbox";
        viewBox.TagName = "";
        var context = CreateTagHelperContext("view-box");
        var output = CreateTagHelperOutput("view-box");

        SetupResourceCompressorCompressToString("Connection error message");

        // Act
        viewBox.Process(context, output);

        // Assert
        output.TagName.Should().Be("main");
    }

    [Fact]
    public void Process_ShouldCreateViewBoxSpecificationWithAllProperties()
    {
        // Arrange
        var viewBox = CreateViewBox();
        viewBox.Id = "test-viewbox";
        viewBox.OnStartLoadingJsFunction = "onStart";
        viewBox.OnTimeoutAfterStartLoadingJsFunction = "onTimeout";
        viewBox.OnFinishLoadingJsFunction = "onFinish";
        viewBox.OnScriptsExecutedJsFunction = "onScripts";
        viewBox.StartLoadingEventDelay = 200;
        viewBox.ChangingBrowserAddressEnable = false;
        viewBox.ParentViewBoxId = "parent-viewbox";

        var context = CreateTagHelperContext("view-box");
        var output = CreateTagHelperOutput("view-box");

        SetupResourceCompressorCompressToString("Custom error message");

        // Act
        viewBox.Process(context, output);

        // Assert
        HtmlResourceRendererMock.Verify(x => x.AddJsCodeFromTemplate(
            "viewbox-init-test-viewbox.js",
            ViewBoxSpecification.InitScriptTemplatePath,
            It.Is<ViewBoxSpecification>(spec =>
                spec.Id == "test-viewbox" &&
                spec.OnStartLoadingJsFunction == "onStart" &&
                spec.OnTimeoutAfterStartLoadingJsFunction == "onTimeout" &&
                spec.OnFinishLoadingJsFunction == "onFinish" &&
                spec.OnScriptsExecutedJsFunction == "onScripts" &&
                spec.StartLoadingEventDelay == 200 &&
                spec.ChangingBrowserAddressEnable == false &&
                spec.ParrentViewBoxId == "parent-viewbox" &&
                spec.ConnectionErrorMessage == "Custom error message"),
            AstraResourceLocation.Body), Times.Once);
    }

    [Fact]
    public void Process_ShouldCreateViewBoxSpecificationWithDefaultValues()
    {
        // Arrange
        var viewBox = CreateViewBox();
        viewBox.Id = "test-viewbox";

        var context = CreateTagHelperContext("view-box");
        var output = CreateTagHelperOutput("view-box");

        SetupResourceCompressorCompressToString("Default error message");
        SetupHtmlResourceRendererRenderBodyResource("viewbox-init-test-viewbox.js", "<script>init code</script>");

        // Act
        viewBox.Process(context, output);

        // Assert
        HtmlResourceRendererMock.Verify(x => x.AddJsCodeFromTemplate(
            "viewbox-init-test-viewbox.js",
            ViewBoxSpecification.InitScriptTemplatePath,
            It.Is<ViewBoxSpecification>(spec =>
                spec.Id == "test-viewbox" &&
                spec.OnStartLoadingJsFunction == null &&
                spec.OnTimeoutAfterStartLoadingJsFunction == null &&
                spec.OnFinishLoadingJsFunction == null &&
                spec.OnScriptsExecutedJsFunction == null &&
                spec.StartLoadingEventDelay == 100 &&
                spec.ChangingBrowserAddressEnable == true &&
                spec.ParrentViewBoxId == null &&
                spec.ConnectionErrorMessage == "Default error message"),
            AstraResourceLocation.Body), Times.Once);

        HtmlResourceRendererMock.Verify(x => x.RenderBodyResource("viewbox-init-test-viewbox.js"), Times.Once);
    }

    [Fact]
    public void Process_ShouldGenerateJavaScriptWithCorrectResourceName()
    {
        // Arrange
        var viewBox = CreateViewBox();
        viewBox.Id = "my-custom-viewbox";

        var context = CreateTagHelperContext("view-box");
        var output = CreateTagHelperOutput("view-box");

        SetupResourceCompressorCompressToString("Error message");
        SetupHtmlResourceRendererRenderBodyResource("viewbox-init-my-custom-viewbox.js", "<script>custom init</script>");

        // Act
        viewBox.Process(context, output);

        // Assert
        HtmlResourceRendererMock.Verify(x => x.AddJsCodeFromTemplate(
            "viewbox-init-my-custom-viewbox.js",
            ViewBoxSpecification.InitScriptTemplatePath,
            It.IsAny<ViewBoxSpecification>(),
            AstraResourceLocation.Body), Times.Once);

        HtmlResourceRendererMock.Verify(x => x.RenderBodyResource("viewbox-init-my-custom-viewbox.js"), Times.Once);
    }

    [Fact]
    public void Process_WhenConnectionErrorMessageResourceIsSet_ShouldUseCustomResource()
    {
        // Arrange
        var viewBox = CreateViewBox();
        viewBox.Id = "test-viewbox";
        var customResource = CreateInMemoryResource("custom-error", "Custom connection error message");
        viewBox.ConnectionErrorMessageResource = customResource;

        var context = CreateTagHelperContext("view-box");
        var output = CreateTagHelperOutput("view-box");

        SetupResourceCompressorCompressToString("Compressed custom error");

        // Act
        viewBox.Process(context, output);

        // Assert
        ResourceCompressorMock.Verify(x => x.CompressToString(customResource), Times.Once);
        HtmlResourceRendererMock.Verify(x => x.AddJsCodeFromTemplate(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.Is<ViewBoxSpecification>(spec => spec.ConnectionErrorMessage == "Compressed custom error"),
            It.IsAny<AstraResourceLocation>()), Times.Once);
    }

    [Fact]
    public void Process_WhenConnectionErrorMessageResourceIsNull_ShouldUseDefaultResource()
    {
        // Arrange
        var viewBox = CreateViewBox();
        viewBox.Id = "test-viewbox";
        viewBox.ConnectionErrorMessageResource = null;

        var context = CreateTagHelperContext("view-box");
        var output = CreateTagHelperOutput("view-box");

        SetupResourceCompressorCompressToString("Default compressed error");

        // Act
        viewBox.Process(context, output);

        // Assert
        ResourceCompressorMock.Verify(x => x.CompressToString(It.IsAny<Resource>()), Times.Once);
        HtmlResourceRendererMock.Verify(x => x.AddJsCodeFromTemplate(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.Is<ViewBoxSpecification>(spec => spec.ConnectionErrorMessage == "Default compressed error"),
            It.IsAny<AstraResourceLocation>()), Times.Once);
    }

    [Fact]
    public void GetConnectionErrorMessage_ShouldReplaceNewlinesWithSpaces()
    {
        // Arrange
        var viewBox = CreateViewBox();
        viewBox.Id = "test-viewbox";
        var resourceWithNewlines = CreateInMemoryResource("error", "Error:\r\nConnection failed\nPlease retry\rLater");
        viewBox.ConnectionErrorMessageResource = resourceWithNewlines;

        ResourceCompressorMock.Setup(x => x.CompressToString(resourceWithNewlines))
            .Returns("Error:\r\nConnection failed\nPlease retry\rLater");

        // Act
        var result = typeof(AlienFruit.Astra.ViewBox.ViewBox)
            .GetMethod("GetConnectionErrorMessage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(viewBox, null) as string;

        // Assert
        result.Should().Be("Error:  Connection failed Please retry Later");
    }

    [Fact]
    public void GetConnectionErrorMessage_ShouldCompressResourceContent()
    {
        // Arrange
        var viewBox = CreateViewBox();
        viewBox.Id = "test-viewbox";
        var uncompressedResource = CreateInMemoryResource("error", "<div class=\"error\">Connection failed</div>");
        viewBox.ConnectionErrorMessageResource = uncompressedResource;

        ResourceCompressorMock.Setup(x => x.CompressToString(uncompressedResource))
            .Returns("<div class=\"error\">Connection failed</div>");

        // Act
        var result = typeof(AlienFruit.Astra.ViewBox.ViewBox)
            .GetMethod("GetConnectionErrorMessage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(viewBox, null) as string;

        // Assert
        ResourceCompressorMock.Verify(x => x.CompressToString(uncompressedResource), Times.Once);
        result.Should().Be("<div class=\"error\">Connection failed</div>");
    }

    [Fact]
    public void GetConnectionErrorMessage_ShouldUseDefaultResourceWhenCustomIsNull()
    {
        // Arrange
        var viewBox = CreateViewBox();
        viewBox.Id = "test-viewbox";
        viewBox.ConnectionErrorMessageResource = null;

        // Mock the default embedded resource compression
        ResourceCompressorMock.Setup(x => x.CompressToString(It.IsAny<Resource>()))
            .Returns("Default connection error message");

        // Act
        var result = typeof(AlienFruit.Astra.ViewBox.ViewBox)
            .GetMethod("GetConnectionErrorMessage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(viewBox, null) as string;

        // Assert
        result.Should().Be("Default connection error message");
    }

    [Fact]
    public void Process_ShouldHandleAllConfigurationParameters()
    {
        // Arrange
        var viewBox = CreateViewBox();
        viewBox.Id = "config-test";
        viewBox.OnStartLoadingJsFunction = "startLoading";
        viewBox.OnTimeoutAfterStartLoadingJsFunction = "timeoutHandler";
        viewBox.OnFinishLoadingJsFunction = "finishLoading";
        viewBox.OnScriptsExecutedJsFunction = "scriptsExecuted";
        viewBox.StartLoadingEventDelay = 300;
        viewBox.ChangingBrowserAddressEnable = false;
        viewBox.ParentViewBoxId = "parent-viewbox";
        viewBox.Class = "custom-class";
        viewBox.Style = "background: blue;";
        viewBox.Role = "navigation";
        viewBox.TagName = "nav";

        var context = CreateTagHelperContext("view-box");
        var output = CreateTagHelperOutput("view-box");

        SetupResourceCompressorCompressToString("Error");

        // Act
        viewBox.Process(context, output);

        // Assert
        output.Attributes["id"].Value.Should().Be("config-test");
        output.Attributes["class"].Value.Should().Be("custom-class");
        output.Attributes["style"].Value.Should().Be("background: blue;");
        output.Attributes["role"].Value.Should().Be("navigation");
        output.TagName.Should().Be("nav");

        HtmlResourceRendererMock.Verify(x => x.AddJsCodeFromTemplate(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.Is<ViewBoxSpecification>(spec =>
                spec.Id == "config-test" &&
                spec.OnStartLoadingJsFunction == "startLoading" &&
                spec.OnTimeoutAfterStartLoadingJsFunction == "timeoutHandler" &&
                spec.OnFinishLoadingJsFunction == "finishLoading" &&
                spec.OnScriptsExecutedJsFunction == "scriptsExecuted" &&
                spec.StartLoadingEventDelay == 300 &&
                spec.ChangingBrowserAddressEnable == false &&
                spec.ParrentViewBoxId == "parent-viewbox"),
            AstraResourceLocation.Body), Times.Once);
    }

    [Fact]
    public void Process_ShouldUseDefaultValuesForUnspecifiedParameters()
    {
        // Arrange
        var viewBox = CreateViewBox();
        viewBox.Id = "defaults-test";
        // Don't set any other properties - they should use defaults

        var context = CreateTagHelperContext("view-box");
        var output = CreateTagHelperOutput("view-box");

        SetupResourceCompressorCompressToString("Error");

        // Act
        viewBox.Process(context, output);

        // Assert
        HtmlResourceRendererMock.Verify(x => x.AddJsCodeFromTemplate(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.Is<ViewBoxSpecification>(spec =>
                spec.Id == "defaults-test" &&
                spec.OnStartLoadingJsFunction == null &&
                spec.OnTimeoutAfterStartLoadingJsFunction == null &&
                spec.OnFinishLoadingJsFunction == null &&
                spec.OnScriptsExecutedJsFunction == null &&
                spec.StartLoadingEventDelay == 100 && // default value
                spec.ChangingBrowserAddressEnable == true && // default value
                spec.ParrentViewBoxId == null),
            AstraResourceLocation.Body), Times.Once);
    }
}
