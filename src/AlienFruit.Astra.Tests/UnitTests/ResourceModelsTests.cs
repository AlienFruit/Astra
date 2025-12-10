using AlienFruit.Astra.Abstractions;
using AlienFruit.Astra.Models;
using FluentAssertions;
using System.Reflection;
using System.Text;

namespace AlienFruit.Astra.Tests.UnitTests;

public class ResourceModelsTests
{
    private class CustomTestResource : Resource
    {
        private readonly string _content;

        public CustomTestResource(string name, string content) : base(name)
        {
            _content = content;
        }

        public override Stream GetStream()
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(_content));
        }
    }

    [Fact]
    public void EmbeddedResource_Constructor_ShouldInitializeProperties()
    {
        // Arrange
        var name = "test-resource.js";
        var path = "AlienFruit.Astra.Core.Resources.load-check.js";
        var assembly = typeof(Resource).Assembly;

        // Act
        var resource = new EmbeddedResource(name, path, assembly);

        // Assert
        resource.Name.Should().Be(name);
        resource.Should().NotBeNull();
    }

    [Fact]
    public void EmbeddedResource_GetStream_ShouldReturnManifestResourceStream()
    {
        // Arrange
        var name = "test-resource.js";
        var path = "AlienFruit.Astra.Core.Resources.load-check.js";
        var assembly = typeof(Resource).Assembly;
        var resource = new EmbeddedResource(name, path, assembly);

        // Act
        var stream = resource.GetStream();

        // Assert
        stream.Should().NotBeNull();
        stream.CanRead.Should().BeTrue();
    }

    [Fact]
    public void EmbeddedResource_GetStream_WithInvalidPath_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var name = "test-resource.js";
        var invalidPath = "Invalid.Path.Resource.js";
        var assembly = typeof(Resource).Assembly;
        var resource = new EmbeddedResource(name, invalidPath, assembly);

        // Act
        var act = () => resource.GetStream();

        // Assert
        act.Should().Throw<KeyNotFoundException>()
            .WithMessage($"There is no resource with path: {invalidPath}");
    }

    [Fact]
    public void EmbeddedResource_GetString_ShouldReturnResourceContentAsString()
    {
        // Arrange
        var name = "test-resource.js";
        var path = "AlienFruit.Astra.Core.Resources.load-check.js";
        var assembly = typeof(Resource).Assembly;
        var resource = new EmbeddedResource(name, path, assembly);

        // Act
        var content = resource.GetString();

        // Assert
        content.Should().NotBeNull();
        content.Should().NotBeEmpty();
        content.Should().Contain("loadCheck"); // Known content from load-check.js
    }

    [Fact]
    public void InMemoryResource_Constructor_ShouldInitializeProperties()
    {
        // Arrange
        var name = "test-resource.html";
        var content = "<div>Hello World</div>";

        // Act
        var resource = new InMemoryResource(name, content);

        // Assert
        resource.Name.Should().Be(name);
        resource.Should().NotBeNull();
    }

    [Fact]
    public void InMemoryResource_GetStream_ShouldReturnMemoryStreamWithContent()
    {
        // Arrange
        var name = "test-resource.html";
        var content = "<div>Hello World</div>";
        var resource = new InMemoryResource(name, content);

        // Act
        var stream = resource.GetStream();

        // Assert
        stream.Should().NotBeNull();
        stream.Should().BeOfType<MemoryStream>();
        stream.CanRead.Should().BeTrue();
        stream.CanSeek.Should().BeTrue();
    }

    [Fact]
    public void InMemoryResource_GetString_ShouldReturnOriginalContent()
    {
        // Arrange
        var name = "test-resource.html";
        var content = "<div>Hello World</div>";
        var resource = new InMemoryResource(name, content);

        // Act
        var result = resource.GetString();

        // Assert
        result.Should().Be(content);
    }

    [Fact]
    public void Resource_Name_ShouldBeSetCorrectly()
    {
        // Arrange
        var name = "custom-resource.js";

        // Act
        var resource = new CustomTestResource(name, "content");

        // Assert
        resource.Name.Should().Be(name);
    }

    [Fact]
    public void Resource_GetString_CustomImplementation_ShouldWork()
    {
        // Arrange
        var name = "custom-resource.txt";
        var content = "Custom resource content";
        var resource = new CustomTestResource(name, content);

        // Act
        var result = resource.GetString();

        // Assert
        result.Should().Be(content);
    }
}
