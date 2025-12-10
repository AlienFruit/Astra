using AlienFruit.Astra.Abstractions;
using AlienFruit.Astra.Core;
using AlienFruit.Astra.Tests.Infrastructure;
using AlienFruit.Astra.ViewBoxLink;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Moq;

namespace AlienFruit.Astra.Tests.UnitTests;

public class ViewBoxLinkTests : AstraTestBase
{
    private AlienFruit.Astra.ViewBoxLink.ViewBoxLink CreateViewBoxLink(string id = "test-link", string uri = "/test", string viewBoxId = "test-viewbox")
    {
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/current";
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Use Activator to create ViewBoxLink without required property validation
        var viewBoxLink = (AlienFruit.Astra.ViewBoxLink.ViewBoxLink)Activator.CreateInstance(
            typeof(AlienFruit.Astra.ViewBoxLink.ViewBoxLink),
            HtmlResourceRendererMock.Object,
            httpContextAccessorMock.Object)!;

        typeof(AlienFruit.Astra.ViewBoxLink.ViewBoxLink).GetProperty("Id")!.SetValue(viewBoxLink, id);
        typeof(AlienFruit.Astra.ViewBoxLink.ViewBoxLink).GetProperty("Uri")!.SetValue(viewBoxLink, uri);
        typeof(AlienFruit.Astra.ViewBoxLink.ViewBoxLink).GetProperty("ViewBoxId")!.SetValue(viewBoxLink, viewBoxId);

        return viewBoxLink;
    }

    [Fact]
    public void Constructor_ShouldInitializeWithDependencies()
    {
        // Arrange & Act
        var viewBoxLink = CreateViewBoxLink();

        // Assert
        viewBoxLink.Should().NotBeNull();
    }

    [Fact]
    public void Process_WhenIdIsNull_ShouldThrowArgumentException()
    {
        // Arrange
        var viewBoxLink = CreateViewBoxLink();
        // Id is required, so we need to use reflection to set it to null for testing
        typeof(AlienFruit.Astra.ViewBoxLink.ViewBoxLink).GetProperty("Id")!.SetValue(viewBoxLink, null);
        var context = CreateTagHelperContext("view-box-link");
        var output = CreateTagHelperOutput("view-box-link");

        // Act
        var act = () => viewBoxLink.Process(context, output);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Id attribute is required");
    }

    [Fact]
    public void Process_WhenIdIsEmpty_ShouldThrowArgumentException()
    {
        // Arrange
        var viewBoxLink = CreateViewBoxLink();
        viewBoxLink.Id = "";
        var context = CreateTagHelperContext("view-box-link");
        var output = CreateTagHelperOutput("view-box-link");

        // Act
        var act = () => viewBoxLink.Process(context, output);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Id attribute is required");
    }

    [Fact]
    public void Process_WhenIdIsWhitespace_ShouldThrowArgumentException()
    {
        // Arrange
        var viewBoxLink = CreateViewBoxLink();
        viewBoxLink.Id = "   ";
        var context = CreateTagHelperContext("view-box-link");
        var output = CreateTagHelperOutput("view-box-link");

        // Act
        var act = () => viewBoxLink.Process(context, output);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Id attribute is required");
    }

    [Fact]
    public void Process_WhenUriIsNull_ShouldThrowArgumentException()
    {
        // Arrange
        var viewBoxLink = CreateViewBoxLink();
        typeof(AlienFruit.Astra.ViewBoxLink.ViewBoxLink).GetProperty("Uri")!.SetValue(viewBoxLink, null);
        var context = CreateTagHelperContext("view-box-link");
        var output = CreateTagHelperOutput("view-box-link");

        // Act
        var act = () => viewBoxLink.Process(context, output);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Uri attribute is required");
    }

    [Fact]
    public void Process_WhenUriIsEmpty_ShouldThrowArgumentException()
    {
        // Arrange
        var viewBoxLink = CreateViewBoxLink();
        viewBoxLink.Uri = "";
        var context = CreateTagHelperContext("view-box-link");
        var output = CreateTagHelperOutput("view-box-link");

        // Act
        var act = () => viewBoxLink.Process(context, output);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Uri attribute is required");
    }

    [Fact]
    public void Process_WhenUriIsWhitespace_ShouldThrowArgumentException()
    {
        // Arrange
        var viewBoxLink = CreateViewBoxLink();
        viewBoxLink.Uri = "   ";
        var context = CreateTagHelperContext("view-box-link");
        var output = CreateTagHelperOutput("view-box-link");

        // Act
        var act = () => viewBoxLink.Process(context, output);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Uri attribute is required");
    }

    [Fact]
    public void Process_WhenIdIsValid_ShouldSetIdAttribute()
    {
        // Arrange
        var viewBoxLink = CreateViewBoxLink();
        viewBoxLink.Id = "test-link";
        var context = CreateTagHelperContext("view-box-link");
        var output = CreateTagHelperOutput("view-box-link");

        SetupHtmlResourceRendererRenderBodyResource("viewbox-link-init-test-link", "<script>init</script>");

        // Act
        viewBoxLink.Process(context, output);

        // Assert
        output.Attributes["id"].Value.Should().Be("test-link");
    }

    [Fact]
    public void Process_WhenUriIsValid_ShouldSetHrefAttribute()
    {
        // Arrange
        var viewBoxLink = CreateViewBoxLink();
        viewBoxLink.Uri = "/test/path";
        var context = CreateTagHelperContext("view-box-link");
        var output = CreateTagHelperOutput("view-box-link");

        SetupHtmlResourceRendererRenderBodyResource("viewbox-link-init-test-link", "<script>init</script>");

        // Act
        viewBoxLink.Process(context, output);

        // Assert
        output.Attributes["href"].Value.Should().Be("/test/path");
    }

    [Fact]
    public void Process_WhenStyleIsSet_ShouldSetStyleAttribute()
    {
        // Arrange
        var viewBoxLink = CreateViewBoxLink();
        viewBoxLink.Style = "color: red; font-size: 14px;";
        var context = CreateTagHelperContext("view-box-link");
        var output = CreateTagHelperOutput("view-box-link");

        SetupHtmlResourceRendererRenderBodyResource("viewbox-link-init-test-link", "<script>init</script>");

        // Act
        viewBoxLink.Process(context, output);

        // Assert
        output.Attributes["style"].Value.Should().Be("color: red; font-size: 14px;");
    }

    [Fact]
    public void Process_WhenStyleIsNullOrEmpty_ShouldNotSetStyleAttribute()
    {
        // Arrange
        var viewBoxLink = CreateViewBoxLink();
        viewBoxLink.Style = null;
        var context = CreateTagHelperContext("view-box-link");
        var output = CreateTagHelperOutput("view-box-link");

        SetupHtmlResourceRendererRenderBodyResource("viewbox-link-init-test-link", "<script>init</script>");

        // Act
        viewBoxLink.Process(context, output);

        // Assert
        output.Attributes.ContainsName("style").Should().BeFalse();
    }

    [Fact]
    public void Process_WhenTagNameIsSet_ShouldUseSpecifiedTagName()
    {
        // Arrange
        var viewBoxLink = CreateViewBoxLink();
        viewBoxLink.TagName = "div";
        var context = CreateTagHelperContext("view-box-link");
        var output = CreateTagHelperOutput("view-box-link");

        SetupHtmlResourceRendererRenderBodyResource("viewbox-link-init-test-link", "<script>init</script>");

        // Act
        viewBoxLink.Process(context, output);

        // Assert
        output.TagName.Should().Be("div");
    }

    [Fact]
    public void Process_WhenTagNameIsNull_ShouldUseDefaultAnchorTag()
    {
        // Arrange
        var viewBoxLink = CreateViewBoxLink();
        viewBoxLink.TagName = null;
        var context = CreateTagHelperContext("view-box-link");
        var output = CreateTagHelperOutput("view-box-link");

        SetupHtmlResourceRendererRenderBodyResource("viewbox-link-init-test-link", "<script>init</script>");

        // Act
        viewBoxLink.Process(context, output);

        // Assert
        output.TagName.Should().Be("a");
    }

    [Fact]
    public void Process_ShouldCreateViewBoxLinkSpecification()
    {
        // Arrange
        var viewBoxLink = CreateViewBoxLink();
        viewBoxLink.Id = "test-link";
        viewBoxLink.Uri = "/test/path";
        viewBoxLink.ViewBoxId = "test-viewbox";
        viewBoxLink.SelectedClassName = "active";
        viewBoxLink.DefaultClassName = "link";
        viewBoxLink.OnClickJsFunction = "handleClick()";
        viewBoxLink.ScrollUp = false;

        var context = CreateTagHelperContext("view-box-link");
        var output = CreateTagHelperOutput("view-box-link");

        SetupHtmlResourceRendererRenderBodyResource("viewbox-link-init-test-link", "<script>init</script>");

        // Act
        viewBoxLink.Process(context, output);

        // Assert
        HtmlResourceRendererMock.Verify(x => x.AddJsCodeFromTemplate(
            "viewbox-link-init-test-link",
            ViewBoxLinkSpecification.InitScriptTemplatePath,
            It.Is<ViewBoxLinkSpecification>(spec =>
                spec.Id == "test-link" &&
                spec.Uri.ToString() == "/test/path" &&
                spec.ViewBoxId == "test-viewbox" &&
                spec.SelectedClassName == "active" &&
                spec.DefaultClassName == "link" &&
                spec.OnClick == "handleClick()" &&
                spec.ScrollUp == false),
            AstraResourceLocation.Body), Times.Once);
    }

    [Fact]
    public void Process_WhenDefaultClassNameIsSet_ShouldSetClassAttribute()
    {
        // Arrange
        var viewBoxLink = CreateViewBoxLink();
        viewBoxLink.DefaultClassName = "link-class";
        var context = CreateTagHelperContext("view-box-link");
        var output = CreateTagHelperOutput("view-box-link");

        SetupHtmlResourceRendererRenderBodyResource("viewbox-link-init-test-link", "<script>init</script>");

        // Act
        viewBoxLink.Process(context, output);

        // Assert
        output.Attributes["class"].Value.Should().Be("link-class");
    }

    [Fact]
    public void Process_WhenSelectedClassNameIsSetAndUriMatchesCurrentPath_ShouldSetSelectedClass()
    {
        // Arrange
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/current/path";
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var viewBoxLink = (AlienFruit.Astra.ViewBoxLink.ViewBoxLink)Activator.CreateInstance(
            typeof(AlienFruit.Astra.ViewBoxLink.ViewBoxLink),
            HtmlResourceRendererMock.Object,
            httpContextAccessorMock.Object)!;

        typeof(AlienFruit.Astra.ViewBoxLink.ViewBoxLink).GetProperty("Id")!.SetValue(viewBoxLink, "test-link");
        typeof(AlienFruit.Astra.ViewBoxLink.ViewBoxLink).GetProperty("Uri")!.SetValue(viewBoxLink, "/current/path");
        typeof(AlienFruit.Astra.ViewBoxLink.ViewBoxLink).GetProperty("ViewBoxId")!.SetValue(viewBoxLink, "test-viewbox");
        viewBoxLink.SelectedClassName = "active";
        viewBoxLink.DefaultClassName = "link";

        var context = CreateTagHelperContext("view-box-link");
        var output = CreateTagHelperOutput("view-box-link");

        SetupHtmlResourceRendererRenderBodyResource("viewbox-link-init-test-link", "<script>init</script>");

        // Act
        viewBoxLink.Process(context, output);

        // Assert
        output.Attributes["class"].Value.Should().Be("active");
    }

    [Fact]
    public void Process_WhenSelectedClassNameIsSetAndUriDoesNotMatchCurrentPath_ShouldSetDefaultClass()
    {
        // Arrange
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/current/path";
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var viewBoxLink = (AlienFruit.Astra.ViewBoxLink.ViewBoxLink)Activator.CreateInstance(
            typeof(AlienFruit.Astra.ViewBoxLink.ViewBoxLink),
            HtmlResourceRendererMock.Object,
            httpContextAccessorMock.Object)!;

        typeof(AlienFruit.Astra.ViewBoxLink.ViewBoxLink).GetProperty("Id")!.SetValue(viewBoxLink, "test-link");
        typeof(AlienFruit.Astra.ViewBoxLink.ViewBoxLink).GetProperty("Uri")!.SetValue(viewBoxLink, "/different/path");
        typeof(AlienFruit.Astra.ViewBoxLink.ViewBoxLink).GetProperty("ViewBoxId")!.SetValue(viewBoxLink, "test-viewbox");
        viewBoxLink.SelectedClassName = "active";
        viewBoxLink.DefaultClassName = "link";

        var context = CreateTagHelperContext("view-box-link");
        var output = CreateTagHelperOutput("view-box-link");

        SetupHtmlResourceRendererRenderBodyResource("viewbox-link-init-test-link", "<script>init</script>");

        // Act
        viewBoxLink.Process(context, output);

        // Assert
        output.Attributes["class"].Value.Should().Be("link");
    }

    [Fact]
    public void Process_WhenHttpContextIsNull_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext)null);

        var viewBoxLink = (AlienFruit.Astra.ViewBoxLink.ViewBoxLink)Activator.CreateInstance(
            typeof(AlienFruit.Astra.ViewBoxLink.ViewBoxLink),
            HtmlResourceRendererMock.Object,
            httpContextAccessorMock.Object)!;

        typeof(AlienFruit.Astra.ViewBoxLink.ViewBoxLink).GetProperty("Id")!.SetValue(viewBoxLink, "test-link");
        typeof(AlienFruit.Astra.ViewBoxLink.ViewBoxLink).GetProperty("Uri")!.SetValue(viewBoxLink, "/test");
        typeof(AlienFruit.Astra.ViewBoxLink.ViewBoxLink).GetProperty("ViewBoxId")!.SetValue(viewBoxLink, "test-viewbox");
        viewBoxLink.SelectedClassName = "active";
        viewBoxLink.DefaultClassName = "link";

        var context = CreateTagHelperContext("view-box-link");
        var output = CreateTagHelperOutput("view-box-link");

        // Act
        var act = () => viewBoxLink.Process(context, output);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("HttpContext is null");
    }

    [Fact]
    public void Process_ShouldAddJsCodeFromTemplate()
    {
        // Arrange
        var viewBoxLink = CreateViewBoxLink();
        viewBoxLink.Id = "test-link";
        var context = CreateTagHelperContext("view-box-link");
        var output = CreateTagHelperOutput("view-box-link");

        SetupHtmlResourceRendererRenderBodyResource("viewbox-link-init-test-link", "<script>init code</script>");

        // Act
        viewBoxLink.Process(context, output);

        // Assert
        HtmlResourceRendererMock.Verify(x => x.AddJsCodeFromTemplate(
            "viewbox-link-init-test-link",
            ViewBoxLinkSpecification.InitScriptTemplatePath,
            It.IsAny<ViewBoxLinkSpecification>(),
            AstraResourceLocation.Body), Times.Once);
    }

    [Fact]
    public void Process_ShouldAppendRenderedBodyResource()
    {
        // Arrange
        var viewBoxLink = CreateViewBoxLink();
        viewBoxLink.Id = "test-link";
        var context = CreateTagHelperContext("view-box-link");
        var output = CreateTagHelperOutput("view-box-link");

        SetupHtmlResourceRendererRenderBodyResource("viewbox-link-init-test-link", "<script>init code</script>");

        // Act
        viewBoxLink.Process(context, output);

        // Assert
        output.PostContent.GetContent().Should().Contain("<script>init code</script>");
    }

    [Fact]
    public void Process_WithAbsoluteUri_ShouldHandleCorrectly()
    {
        // Arrange
        var viewBoxLink = CreateViewBoxLink();
        viewBoxLink.Uri = "https://example.com/test";
        var context = CreateTagHelperContext("view-box-link");
        var output = CreateTagHelperOutput("view-box-link");

        SetupHtmlResourceRendererRenderBodyResource("viewbox-link-init-test-link", "<script>init</script>");

        // Act
        viewBoxLink.Process(context, output);

        // Assert
        output.Attributes["href"].Value.Should().Be("https://example.com/test");
    }

    [Fact]
    public void Process_WithRelativeUri_ShouldHandleCorrectly()
    {
        // Arrange
        var viewBoxLink = CreateViewBoxLink();
        viewBoxLink.Uri = "../relative/path";
        var context = CreateTagHelperContext("view-box-link");
        var output = CreateTagHelperOutput("view-box-link");

        SetupHtmlResourceRendererRenderBodyResource("viewbox-link-init-test-link", "<script>init</script>");

        // Act
        viewBoxLink.Process(context, output);

        // Assert
        output.Attributes["href"].Value.Should().Be("../relative/path");
    }

    [Fact]
    public void Process_WithOnClickJsFunction_ShouldIncludeInSpecification()
    {
        // Arrange
        var viewBoxLink = CreateViewBoxLink();
        viewBoxLink.OnClickJsFunction = "customClickHandler(event)";
        var context = CreateTagHelperContext("view-box-link");
        var output = CreateTagHelperOutput("view-box-link");

        SetupHtmlResourceRendererRenderBodyResource("viewbox-link-init-test-link", "<script>init</script>");

        // Act
        viewBoxLink.Process(context, output);

        // Assert
        HtmlResourceRendererMock.Verify(x => x.AddJsCodeFromTemplate(
            "viewbox-link-init-test-link",
            ViewBoxLinkSpecification.InitScriptTemplatePath,
            It.Is<ViewBoxLinkSpecification>(spec => spec.OnClick == "customClickHandler(event)"),
            AstraResourceLocation.Body), Times.Once);
    }

    [Fact]
    public void Process_WithScrollUpFalse_ShouldIncludeInSpecification()
    {
        // Arrange
        var viewBoxLink = CreateViewBoxLink();
        viewBoxLink.ScrollUp = false;
        var context = CreateTagHelperContext("view-box-link");
        var output = CreateTagHelperOutput("view-box-link");

        SetupHtmlResourceRendererRenderBodyResource("viewbox-link-init-test-link", "<script>init</script>");

        // Act
        viewBoxLink.Process(context, output);

        // Assert
        HtmlResourceRendererMock.Verify(x => x.AddJsCodeFromTemplate(
            "viewbox-link-init-test-link",
            ViewBoxLinkSpecification.InitScriptTemplatePath,
            It.Is<ViewBoxLinkSpecification>(spec => spec.ScrollUp == false),
            AstraResourceLocation.Body), Times.Once);
    }
}