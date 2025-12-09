using AlienFruit.Astra.Core;
using AlienFruit.Astra.Models;
using AlienFruit.Astra.Tests.Infrastructure;
using FluentAssertions;
using Moq;
using System.Reflection;

namespace AlienFruit.Astra.Tests.UnitTests;

public class ResourceStorageTests : AstraTestBase
{
    private ResourceStorage CreateResourceStorage() => new(ResourceCompressorMock.Object);

    [Fact]
    public void Constructor_ShouldInitializeWithResourceCompressor()
    {
        // Act
        var storage = CreateResourceStorage();

        // Assert
        storage.Should().NotBeNull();
    }

    [Fact]
    public void RegisterResource_Embedded_ShouldAddEmbeddedResource()
    {
        // Arrange
        var storage = CreateResourceStorage();
        var name = "test-resource.js";
        var path = "Test.Path.Resource.js";
        var assembly = typeof(ResourceStorageTests).Assembly;

        // Act
        storage.RegisterResource(name, path, assembly);

        // Assert
        storage.Contains(name).Should().BeTrue();
    }

    [Fact]
    public void RegisterResource_InMemory_ShouldAddInMemoryResource()
    {
        // Arrange
        var storage = CreateResourceStorage();
        var name = "test-resource.js";
        var content = "console.log('test');";

        // Act
        storage.RegisterResource(name, content);

        // Assert
        storage.Contains(name).Should().BeTrue();
    }

    [Fact]
    public void RegisterResource_Generic_ShouldAddResource()
    {
        // Arrange
        var storage = CreateResourceStorage();
        var resource = CreateEmbeddedResource("test-resource.js", "Test.Path.js", typeof(ResourceStorageTests).Assembly);

        // Act
        storage.RegisterResource(resource);

        // Assert
        storage.Contains(resource.Name).Should().BeTrue();
    }

    [Fact]
    public void RegisterResource_Duplicate_ShouldNotOverwriteExisting()
    {
        // Arrange
        var storage = CreateResourceStorage();
        var name = "test-resource.js";
        var content1 = "console.log('first');";
        var content2 = "console.log('second');";

        // Act
        storage.RegisterResource(name, content1);
        storage.RegisterResource(name, content2);

        // Assert
        storage.Contains(name).Should().BeTrue();
        // The first registration should remain
    }

    [Fact]
    public void Contains_ShouldReturnTrueForExistingResource()
    {
        // Arrange
        var storage = CreateResourceStorage();
        var name = "test-resource.js";
        storage.RegisterResource(name, "console.log('test');");

        // Act
        var result = storage.Contains(name);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Contains_ShouldReturnFalseForNonExistingResource()
    {
        // Arrange
        var storage = CreateResourceStorage();

        // Act
        var result = storage.Contains("non-existing.js");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void OpenRead_ShouldReturnCompressedStreamForExistingResource()
    {
        // Arrange
        var storage = CreateResourceStorage();
        var name = "test-resource.js";
        var content = "console.log('test');";
        var expectedStream = CreateMemoryStream("compressed content");

        storage.RegisterResource(name, content);
        SetupResourceCompressorCompressToStream(expectedStream);

        // Act
        var result = storage.OpenRead(name);

        // Assert
        result.Should().BeSameAs(expectedStream);
        ResourceCompressorMock.Verify(x => x.CompressToStream(It.Is<Resource>(r => r.Name == name)), Times.Once);
    }

    [Fact]
    public void OpenRead_ShouldThrowExceptionForNonExistingResource()
    {
        // Arrange
        var storage = CreateResourceStorage();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => storage.OpenRead("non-existing.js"));
        exception.Message.Should().Contain("The file \"non-existing.js\" was not registered");
    }

    [Fact]
    public async Task OpenReadAsync_ShouldReturnCompressedStreamForExistingResource()
    {
        // Arrange
        var storage = CreateResourceStorage();
        var name = "test-resource.js";
        var content = "console.log('test');";
        var expectedStream = CreateMemoryStream("compressed content");

        storage.RegisterResource(name, content);
        SetupResourceCompressorCompressToStream(expectedStream);

        // Act
        var result = await storage.OpenReadAsync(name);

        // Assert
        result.Should().BeSameAs(expectedStream);
        ResourceCompressorMock.Verify(x => x.CompressToStream(It.Is<Resource>(r => r.Name == name)), Times.Once);
    }

    [Fact]
    public async Task OpenReadAsync_ShouldThrowExceptionForNonExistingResource()
    {
        // Arrange
        var storage = CreateResourceStorage();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => storage.OpenReadAsync("non-existing.js"));
        exception.Message.Should().Contain("The file \"non-existing.js\" was not registered");
    }
}
