using AlienFruit.Astra.Tests.Infrastructure;
using AlienFruit.Astra.ViewBoxLinkGroup;

namespace AlienFruit.Astra.Tests.UnitTests;

/// <summary>
/// Tests for the view-box-link-group tag helper. Created at stage 3 together with the
/// stateless init-script path — the helper previously had no dedicated coverage.
/// </summary>
public class ViewBoxLinkGroupTests : AstraTestBase
{
    private AlienFruit.Astra.ViewBoxLinkGroup.ViewBoxLinkGroup CreateGroup(
        IInitScriptRenderer? initScriptRenderer = null,
        string path = "/current")
    {
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = path;
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        return new AlienFruit.Astra.ViewBoxLinkGroup.ViewBoxLinkGroup(
            initScriptRenderer ?? InitScriptRendererMock.Object,
            httpContextAccessorMock.Object)
        {
            Id = "test-group",
            ViewBoxId = "test-viewbox",
            UriToActivete = ["/one", "/two"]
        };
    }

    [Fact]
    public async Task Process_WhenIdIsValid_ShouldSetAttributes()
    {
        // Arrange
        var group = CreateGroup();
        group.DefaultClassName = "group-link";
        var context = CreateTagHelperContext("view-box-link-group");
        var output = CreateTagHelperOutput("view-box-link-group");

        InitScriptRendererMock.Setup(x => x.Render(It.IsAny<ViewBoxLinkGroupSpecification>()))
            .Returns("group init");

        // Act
        await group.ProcessAsync(context, output);

        // Assert
        output.TagName.Should().Be("div");
        output.Attributes["id"].Value.Should().Be("test-group");
        output.Attributes["class"].Value.Should().Be("group-link");
        output.PostContent.GetContent().Should().Be("<script>group init</script>");
    }

    [Fact]
    public async Task Process_ShouldRenderInlineInitScriptWithSpecification()
    {
        // Arrange
        var group = CreateGroup();
        group.SelectedClassName = "active";
        group.DefaultClassName = "link";
        var context = CreateTagHelperContext("view-box-link-group");
        var output = CreateTagHelperOutput("view-box-link-group");

        InitScriptRendererMock.Setup(x => x.Render(It.IsAny<ViewBoxLinkGroupSpecification>()))
            .Returns("group init");

        // Act
        await group.ProcessAsync(context, output);

        // Assert
        InitScriptRendererMock.Verify(x => x.Render(
            It.Is<ViewBoxLinkGroupSpecification>(spec =>
                spec.Id == "test-group"
                && spec.ViewBoxId == "test-viewbox"
                && spec.SelectedClassName == "active"
                && spec.DefaultClassName == "link"
                && spec.UriToActivete.SequenceEqual(new[] { "/one", "/two" }))), Times.Once);
    }

    [Fact]
    public async Task InitScript_GoldenOutput_RendersTemplateWithUriList()
    {
        // Arrange: real stateless renderer, no compressor — raw template output.
        var group = CreateGroup(new InitScriptRenderer());
        group.SelectedClassName = "active";
        group.DefaultClassName = "link";
        var context = CreateTagHelperContext("view-box-link-group");
        var output = CreateTagHelperOutput("view-box-link-group");

        // Act
        await group.ProcessAsync(context, output);

        // Assert
        var html = output.PostContent.GetContent();
        html.Should().StartWith("<script>");
        html.TrimEnd().Should().EndWith("</script>");
        html.Should().Contain("ViewBoxRegistry.getOrOnReady('test-viewbox'");
        html.Should().Contain("var uriListToActivate = ['/one', '/two'];");
        html.Should().Contain("viewBox.registerUriGroup('test-group', 'active', 'link', uriListToActivate)");
    }

    [Fact]
    public async Task Process_WhenIdIsMissing_ShouldThrowArgumentException()
    {
        // Arrange
        var group = CreateGroup();
        group.Id = "";
        var context = CreateTagHelperContext("view-box-link-group");
        var output = CreateTagHelperOutput("view-box-link-group");

        // Act
        var act = async () => await group.ProcessAsync(context, output);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("Id attribute is required");
    }

    [Fact]
    public async Task Process_WhenUriToActivateIsEmpty_ShouldThrowArgumentException()
    {
        // Arrange
        var group = CreateGroup();
        group.UriToActivete = [];
        var context = CreateTagHelperContext("view-box-link-group");
        var output = CreateTagHelperOutput("view-box-link-group");

        // Act
        var act = async () => await group.ProcessAsync(context, output);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("UriToActivete attribute must contain at least one URI");
    }
}
