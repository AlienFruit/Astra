using AlienFruit.Astra.Tests.Infrastructure;

namespace AlienFruit.Astra.Tests.UnitTests;

public class ViewBoxLinkTests : AstraTestBase
{
    private const string currentPath = "/current";

    private void SetupViewBoxLinkScript(string scriptContent)
    {
        InitScriptRendererMock.Setup(x => x.Render(It.IsAny<ViewBoxLinkSpecification>()))
            .Returns(scriptContent);
    }

    private static IHttpContextAccessor CreateHttpContextAccessor(string? requestPath)
    {
        var mock = new Mock<IHttpContextAccessor>();
        if (requestPath is null)
        {
            mock.Setup(x => x.HttpContext).Returns((HttpContext)null!);
        }
        else
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Path = requestPath;
            mock.Setup(x => x.HttpContext).Returns(httpContext);
        }

        return mock.Object;
    }

    private AlienFruit.Astra.ViewBoxLink.ViewBoxLink CreateViewBoxLink(
        string id = "test-link",
        string uri = "/test",
        string viewBoxId = "test-viewbox",
        IHttpContextAccessor? httpContextAccessor = null,
        IInitScriptRenderer? initScriptRenderer = null)
    {
        return new AlienFruit.Astra.ViewBoxLink.ViewBoxLink(
            initScriptRenderer ?? InitScriptRendererMock.Object,
            httpContextAccessor ?? CreateHttpContextAccessor(currentPath))
        {
            Id = id,
            Uri = uri,
            ViewBoxId = viewBoxId
        };
    }

    private TagHelperContext Context() => CreateTagHelperContext("view-box-link");

    private TagHelperOutput Output() => CreateTagHelperOutput("view-box-link");

    [Fact]
    public void Process_WhenIdIsNull_ShouldThrowArgumentException()
    {
        var viewBoxLink = CreateViewBoxLink();
        viewBoxLink.Id = null!;

        var context = Context();
        var output = Output();

        var act = () => viewBoxLink.Process(context, output);

        act.Should().Throw<ArgumentException>()
            .WithMessage("Id attribute is required");
    }

    [Fact]
    public void Process_WhenIdIsEmpty_ShouldThrowArgumentException()
    {
        var viewBoxLink = CreateViewBoxLink();
        viewBoxLink.Id = "";

        var context = Context();
        var output = Output();

        var act = () => viewBoxLink.Process(context, output);

        act.Should().Throw<ArgumentException>()
            .WithMessage("Id attribute is required");
    }

    [Fact]
    public void Process_WhenIdIsWhitespace_ShouldThrowArgumentException()
    {
        var viewBoxLink = CreateViewBoxLink();
        viewBoxLink.Id = "   ";

        var context = Context();
        var output = Output();

        var act = () => viewBoxLink.Process(context, output);

        act.Should().Throw<ArgumentException>()
            .WithMessage("Id attribute is required");
    }

    [Fact]
    public void Process_WhenUriIsNull_ShouldThrowArgumentException()
    {
        var viewBoxLink = CreateViewBoxLink();
        viewBoxLink.Uri = null!;

        var context = Context();
        var output = Output();

        var act = () => viewBoxLink.Process(context, output);

        act.Should().Throw<ArgumentException>()
            .WithMessage("Uri attribute is required");
    }

    [Fact]
    public void Process_WhenUriIsEmpty_ShouldThrowArgumentException()
    {
        var viewBoxLink = CreateViewBoxLink();
        viewBoxLink.Uri = "";

        var context = Context();
        var output = Output();

        var act = () => viewBoxLink.Process(context, output);

        act.Should().Throw<ArgumentException>()
            .WithMessage("Uri attribute is required");
    }

    [Fact]
    public void Process_WhenUriIsWhitespace_ShouldThrowArgumentException()
    {
        var viewBoxLink = CreateViewBoxLink();
        viewBoxLink.Uri = "   ";

        var context = Context();
        var output = Output();

        var act = () => viewBoxLink.Process(context, output);

        act.Should().Throw<ArgumentException>()
            .WithMessage("Uri attribute is required");
    }

    [Fact]
    public void Process_WhenViewBoxIdIsWhitespace_ShouldThrowArgumentException()
    {
        var viewBoxLink = CreateViewBoxLink(viewBoxId: " ");

        var context = Context();
        var output = Output();

        var act = () => viewBoxLink.Process(context, output);

        act.Should().Throw<ArgumentException>()
            .WithMessage("ViewBoxId attribute is required");
    }

    [Fact]
    public void Process_WhenIdIsValid_ShouldSetIdAttribute()
    {
        var viewBoxLink = CreateViewBoxLink(id: "my-link");
        var context = Context();
        var output = Output();

        SetupViewBoxLinkScript("init");

        viewBoxLink.Process(context, output);

        output.Attributes["id"].Value.Should().Be("my-link");
    }

    [Fact]
    public void Process_WhenUriIsValid_ShouldSetHrefAttribute()
    {
        var viewBoxLink = CreateViewBoxLink(uri: "/test/path");
        var context = Context();
        var output = Output();

        SetupViewBoxLinkScript("init");

        viewBoxLink.Process(context, output);

        output.Attributes["href"].Value.Should().Be("/test/path");
    }

    [Fact]
    public void Process_WhenStyleIsSet_ShouldSetStyleAttribute()
    {
        var viewBoxLink = CreateViewBoxLink();
        viewBoxLink.Style = "color: red; font-size: 14px;";
        var context = Context();
        var output = Output();

        SetupViewBoxLinkScript("init");

        viewBoxLink.Process(context, output);

        output.Attributes["style"].Value.Should().Be("color: red; font-size: 14px;");
    }

    [Fact]
    public void Process_WhenStyleIsNullOrEmpty_ShouldNotSetStyleAttribute()
    {
        var viewBoxLink = CreateViewBoxLink();
        viewBoxLink.Style = null;
        var context = Context();
        var output = Output();

        SetupViewBoxLinkScript("init");

        viewBoxLink.Process(context, output);

        output.Attributes.ContainsName("style").Should().BeFalse();
    }

    [Fact]
    public void Process_WhenTagNameIsSet_ShouldUseSpecifiedTagName()
    {
        var viewBoxLink = CreateViewBoxLink();
        viewBoxLink.TagName = "div";
        var context = Context();
        var output = Output();

        SetupViewBoxLinkScript("init");

        viewBoxLink.Process(context, output);

        output.TagName.Should().Be("div");
    }

    [Fact]
    public void Process_WhenTagNameIsNull_ShouldUseDefaultAnchorTag()
    {
        var viewBoxLink = CreateViewBoxLink();
        viewBoxLink.TagName = null;
        var context = Context();
        var output = Output();

        SetupViewBoxLinkScript("init");

        viewBoxLink.Process(context, output);

        output.TagName.Should().Be("a");
    }

    [Fact]
    public void Process_ShouldRenderInitScriptWrappedInScriptTag()
    {
        var viewBoxLink = CreateViewBoxLink();
        var context = Context();
        var output = Output();

        SetupViewBoxLinkScript("group init();");

        viewBoxLink.Process(context, output);

        output.PostContent.GetContent().Should().Be("<script>group init();</script>");
    }

    [Fact]
    public void InitScript_GoldenOutput_RendersTemplateFromSpecification()
    {
        // Arrange: real stateless renderer — raw template output, no mocks involved.
        var viewBoxLink = CreateViewBoxLink(
            id: "test-link",
            uri: "/some/path",
            viewBoxId: "test-viewbox",
            initScriptRenderer: new InitScriptRenderer());
        viewBoxLink.SelectedClassName = "active";
        viewBoxLink.DefaultClassName = "link";

        var context = Context();
        var output = Output();

        // Act
        viewBoxLink.Process(context, output);

        // Assert
        var html = output.PostContent.GetContent();
        html.Should().StartWith("<script>");
        html.TrimEnd().Should().EndWith("</script>");
        html.Should().Contain("ViewBoxRegistry.getOrOnReady('test-viewbox'");
        html.Should().Contain("viewBox.registerUri('/some/path', 'test-link', 'active', 'link', true)");
    }

    [Fact]
    public void InitScript_GoldenOutput_OmitsClickHandlerWhenOnClickIsNotSet()
    {
        var viewBoxLink = CreateViewBoxLink(initScriptRenderer: new InitScriptRenderer());

        var context = Context();
        var output = Output();

        // Act
        viewBoxLink.Process(context, output);

        // Assert
        output.PostContent.GetContent().Should().NotContain("addEventListener");
    }

    [Fact]
    public void InitScript_GoldenOutput_RendersClickHandlerWhenOnClickIsSet()
    {
        var viewBoxLink = CreateViewBoxLink(id: "click-link", initScriptRenderer: new InitScriptRenderer());
        viewBoxLink.OnClickJsFunction = "console.log('clicked')";

        var context = Context();
        var output = Output();

        // Act
        viewBoxLink.Process(context, output);

        // Assert
        var html = output.PostContent.GetContent();
        html.Should().Contain("document.getElementById('click-link')");
        html.Should().Contain("addEventListener('click'");
        html.Should().Contain("console.log('clicked')");
    }

    [Fact]
    public void Process_ShouldCreateViewBoxLinkSpecification()
    {
        var viewBoxLink = CreateViewBoxLink(id: "test-link", uri: "/test/path", viewBoxId: "test-viewbox");
        viewBoxLink.SelectedClassName = "active";
        viewBoxLink.DefaultClassName = "link";
        viewBoxLink.OnClickJsFunction = "handleClick()";
        viewBoxLink.ScrollUp = false;

        var context = Context();
        var output = Output();

        SetupViewBoxLinkScript("init");

        viewBoxLink.Process(context, output);

        InitScriptRendererMock.Verify(x => x.Render(It.Is<ViewBoxLinkSpecification>(spec =>
            spec.Id == "test-link"
            && spec.ViewBoxId == "test-viewbox"
            && spec.Uri.OriginalString == "/test/path"
            && spec.SelectedClassName == "active"
            && spec.DefaultClassName == "link"
            && spec.OnClick == "handleClick()"
            && spec.ScrollUp == false)), Times.Once);
    }

    [Fact]
    public void Process_WhenDefaultClassNameIsSet_ShouldSetClassAttribute()
    {
        var viewBoxLink = CreateViewBoxLink(uri: currentPath);
        viewBoxLink.DefaultClassName = "link-class";
        var context = Context();
        var output = Output();

        SetupViewBoxLinkScript("init");

        viewBoxLink.Process(context, output);

        output.Attributes["class"].Value.Should().Be("link-class");
    }

    [Fact]
    public void Process_WhenNoClassNameIsSet_ShouldNotSetClassAttribute()
    {
        var viewBoxLink = CreateViewBoxLink(uri: currentPath);
        var context = Context();
        var output = Output();

        SetupViewBoxLinkScript("init");

        viewBoxLink.Process(context, output);

        output.Attributes.ContainsName("class").Should().BeFalse();
    }

    [Fact]
    public void Process_WhenSelectedClassNameIsSetAndUriMatchesCurrentPath_ShouldSetSelectedClass()
    {
        var viewBoxLink = CreateViewBoxLink(uri: currentPath);
        viewBoxLink.SelectedClassName = "active";
        viewBoxLink.DefaultClassName = "link";

        var context = Context();
        var output = Output();

        SetupViewBoxLinkScript("init");

        viewBoxLink.Process(context, output);

        output.Attributes["class"].Value.Should().Be("active");
    }

    [Fact]
    public void Process_WhenSelectedClassNameIsSetAndUriDoesNotMatchCurrentPath_ShouldSetDefaultClass()
    {
        var viewBoxLink = CreateViewBoxLink(uri: "/different/path");
        viewBoxLink.SelectedClassName = "active";
        viewBoxLink.DefaultClassName = "link";

        var context = Context();
        var output = Output();

        SetupViewBoxLinkScript("init");

        viewBoxLink.Process(context, output);

        output.Attributes["class"].Value.Should().Be("link");
    }

    [Fact]
    public void Process_WhenSelectedClassNameIsSetAndUriMatchesCurrentPathIgnoringCase_ShouldSetSelectedClass()
    {
        var viewBoxLink = CreateViewBoxLink(uri: "/Current");
        viewBoxLink.SelectedClassName = "active";
        viewBoxLink.DefaultClassName = "link";

        var context = Context();
        var output = Output();

        SetupViewBoxLinkScript("init");

        viewBoxLink.Process(context, output);

        output.Attributes["class"].Value.Should().Be("active");
    }

    [Fact]
    public void Process_WhenSelectedClassNameIsSetAndHttpContextIsNull_ShouldThrowInvalidOperationException()
    {
        var viewBoxLink = CreateViewBoxLink(
            uri: currentPath,
            httpContextAccessor: CreateHttpContextAccessor(null));
        viewBoxLink.SelectedClassName = "active";
        viewBoxLink.DefaultClassName = "link";

        var context = Context();
        var output = Output();

        SetupViewBoxLinkScript("init");

        var act = () => viewBoxLink.Process(context, output);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("HttpContext is null");
    }

    [Fact]
    public void Process_WithAbsoluteUri_ShouldSetHrefAndKeepDefaultClass()
    {
        var viewBoxLink = CreateViewBoxLink(uri: "https://example.com/test");
        viewBoxLink.SelectedClassName = "active";
        viewBoxLink.DefaultClassName = "link";

        var context = Context();
        var output = Output();

        SetupViewBoxLinkScript("init");

        viewBoxLink.Process(context, output);

        output.Attributes["href"].Value.Should().Be("https://example.com/test");
        output.Attributes["class"].Value.Should().Be("link");
    }

    [Fact]
    public void Process_WithRelativeUri_ShouldSetHref()
    {
        var viewBoxLink = CreateViewBoxLink(uri: "../relative/path");
        var context = Context();
        var output = Output();

        SetupViewBoxLinkScript("init");

        viewBoxLink.Process(context, output);

        output.Attributes["href"].Value.Should().Be("../relative/path");
    }

    [Fact]
    public void Process_WithOnClickJsFunction_ShouldIncludeInSpecification()
    {
        var viewBoxLink = CreateViewBoxLink();
        viewBoxLink.OnClickJsFunction = "customClickHandler(event)";

        var context = Context();
        var output = Output();

        SetupViewBoxLinkScript("init");

        viewBoxLink.Process(context, output);

        InitScriptRendererMock.Verify(x => x.Render(
            It.Is<ViewBoxLinkSpecification>(spec => spec.OnClick == "customClickHandler(event)")), Times.Once);
    }

    [Fact]
    public void Process_WithScrollUpFalse_ShouldIncludeInSpecification()
    {
        var viewBoxLink = CreateViewBoxLink();
        viewBoxLink.ScrollUp = false;

        var context = Context();
        var output = Output();

        SetupViewBoxLinkScript("init");

        viewBoxLink.Process(context, output);

        InitScriptRendererMock.Verify(x => x.Render(
            It.Is<ViewBoxLinkSpecification>(spec => spec.ScrollUp == false)), Times.Once);
    }
}
