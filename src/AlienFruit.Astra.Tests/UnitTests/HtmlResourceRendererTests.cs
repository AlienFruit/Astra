using AlienFruit.Astra.Tests.Infrastructure;

namespace AlienFruit.Astra.Tests.UnitTests;

public class HtmlResourceRendererTests : AstraTestBase
{
    private HtmlResourceRenderer CreateHtmlResourceRenderer() =>
        new(ResourceStorageMock.Object, CreateAstraConfiguration());

    [Fact]
    public void AddResource_NewResource_RegistersCorrectly()
    {
        // Arrange
        var renderer = CreateHtmlResourceRenderer();
        var name = "test-resource.css";
        var path = "Test.Path.Css";
        var assembly = typeof(HtmlResourceRendererTests).Assembly;

        // Act
        renderer.AddResource(name, path, assembly);

        // Assert
        ResourceStorageMock.Verify(x => x.RegisterResource(name, path, assembly), Times.Once);
    }
}
