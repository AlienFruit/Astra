using AlienFruit.Astra.Tests.Infrastructure;
using FluentAssertions;

namespace AlienFruit.Astra.Tests.UnitTests;

public class InfrastructureTests : AstraTestBase
{
    [Fact]
    public void AstraTestBase_ShouldInitializeMocks()
    {
        // Assert
        HtmlResourceRendererMock.Should().NotBeNull();
        ResourceCompressorMock.Should().NotBeNull();
    }

    [Fact]
    public void CreateTagHelperContext_ShouldCreateValidContext()
    {
        // Act
        var context = CreateTagHelperContext("test-tag");

        // Assert
        context.TagName.Should().Be("test-tag");
        context.UniqueId.Should().NotBeNullOrEmpty();
        context.Items.Should().NotBeNull();
    }

    [Fact]
    public void CreateTagHelperOutput_ShouldCreateValidOutput()
    {
        // Act
        var output = CreateTagHelperOutput("test-tag");

        // Assert
        output.TagName.Should().Be("test-tag");
        output.Attributes.Should().NotBeNull();
    }

    [Fact]
    public void SetupHtmlResourceRendererRenderHeaders_ShouldConfigureMock()
    {
        // Arrange
        var expectedHtml = "<script src='test.js'></script>";

        // Act
        SetupHtmlResourceRendererRenderHeaders(expectedHtml);

        // Assert
        var result = HtmlResourceRendererMock.Object.RenderHeaders();
        result.ToString().Should().Be(expectedHtml);
    }

    [Fact]
    public void SetupHtmlResourceRendererRenderBodyResource_ShouldConfigureMock()
    {
        // Arrange
        var resourceName = "test.js";
        var expectedHtml = "<script>console.log('test');</script>";

        // Act
        SetupHtmlResourceRendererRenderBodyResource(resourceName, expectedHtml);

        // Assert
        var result = HtmlResourceRendererMock.Object.RenderBodyResource(resourceName);
        result.ToString().Should().Be(expectedHtml);
    }

    [Fact]
    public void SetupResourceCompressorCompressToString_ShouldConfigureMock()
    {
        // Arrange
        var expectedResult = "compressed content";

        // Act
        SetupResourceCompressorCompressToString(expectedResult);

        // Assert
        var result = ResourceCompressorMock.Object.CompressToString(null!);
        result.Should().Be(expectedResult);
    }

    [Fact]
    public void CreateEmbeddedResource_ShouldCreateValidResource()
    {
        // Arrange
        var name = "test-resource";
        var path = "Test.Path.Resource.js";
        var assembly = typeof(InfrastructureTests).Assembly;

        // Act
        var resource = CreateEmbeddedResource(name, path, assembly);

        // Assert
        resource.Name.Should().Be(name);
        resource.Should().BeOfType<EmbeddedResource>();
    }

    [Fact]
    public void CreateInMemoryResource_ShouldCreateValidResource()
    {
        // Arrange
        var name = "test-resource";
        var content = "console.log('test');";

        // Act
        var resource = CreateInMemoryResource(name, content);

        // Assert
        resource.Name.Should().Be(name);
        resource.Should().BeOfType<InMemoryResource>();
    }

    [Fact]
    public void CreateMemoryStream_ShouldCreateStreamWithContent()
    {
        // Arrange
        var content = "test content";

        // Act
        var stream = CreateMemoryStream(content);

        // Assert
        stream.Should().NotBeNull();
        stream.Length.Should().BeGreaterThan(0);

        // Verify content
        stream.Position = 0;
        using var reader = new StreamReader(stream);
        var readContent = reader.ReadToEnd();
        readContent.Should().Be(content);
    }
}
