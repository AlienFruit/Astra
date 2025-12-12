using AlienFruit.Astra.Configuration;
using AlienFruit.Astra.Core;
using AlienFruit.Astra.Core.ResourceCompressors;
using AlienFruit.Astra.Abstractions;
using AlienFruit.Astra.Models;
using AlienFruit.Astra.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.IO;
using System.Text;

namespace AlienFruit.Astra.Tests.UnitTests;

public class AstraConfigurationTests
{
    [Fact]
    public void Constructor_DefaultValues_ShouldSetCorrectDefaults()
    {
        // Act
        var config = new AstraConfiguration();

        // Assert
        config.UseCompression.Should().BeTrue();
        config.ResourcesRoute.Should().Be("astra");
        config.EnableVersioning.Should().BeFalse();
        config.ResourceVersion.Should().BeNull();
        config.CacheMaxAge.Should().Be(31536000); // 1 year in seconds
    }

    [Fact]
    public void Name_Property_ShouldReturnClassName()
    {
        // Act & Assert
        AstraConfiguration.Name.Should().Be("AstraConfiguration");
    }

    [Fact]
    public void Configuration_BindFromJson_ShouldLoadAllProperties()
    {
        // Arrange
        var json = @"
        {
            ""AstraConfiguration"": {
                ""UseCompression"": false,
                ""ResourcesRoute"": ""custom-resources"",
                ""EnableVersioning"": true,
                ""ResourceVersion"": ""2.1.0"",
                ""CacheMaxAge"": 86400
            }
        }";

        var configuration = new ConfigurationBuilder()
            .AddJsonStream(new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(json)))
            .Build();

        // Act
        var section = configuration.GetSection(AstraConfiguration.Name);
        var astraConfig = section.Get<AstraConfiguration>();

        // Assert
        astraConfig.Should().NotBeNull();
        astraConfig!.UseCompression.Should().BeFalse();
        astraConfig.ResourcesRoute.Should().Be("custom-resources");
        astraConfig.EnableVersioning.Should().BeTrue();
        astraConfig.ResourceVersion.Should().Be("2.1.0");
        astraConfig.CacheMaxAge.Should().Be(86400);
    }

    [Fact]
    public void Configuration_BindFromEmptySection_ShouldReturnNull()
    {
        // Arrange
        var json = @"{}";

        var configuration = new ConfigurationBuilder()
            .AddJsonStream(new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(json)))
            .Build();

        // Act
        var section = configuration.GetSection(AstraConfiguration.Name);
        var astraConfig = section.Get<AstraConfiguration>();

        // Assert
        astraConfig.Should().BeNull();
    }

    [Fact]
    public void Configuration_BindPartialConfig_ShouldUseDefaultsForMissingProperties()
    {
        // Arrange
        var json = @"
        {
            ""AstraConfiguration"": {
                ""UseCompression"": false,
                ""ResourcesRoute"": ""custom""
            }
        }";

        var configuration = new ConfigurationBuilder()
            .AddJsonStream(new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(json)))
            .Build();

        // Act
        var section = configuration.GetSection(AstraConfiguration.Name);
        var astraConfig = section.Get<AstraConfiguration>();

        // Assert
        astraConfig.Should().NotBeNull();
        astraConfig!.UseCompression.Should().BeFalse();
        astraConfig.ResourcesRoute.Should().Be("custom");
        astraConfig.EnableVersioning.Should().BeFalse(); // default
        astraConfig.ResourceVersion.Should().BeNull(); // default
        astraConfig.CacheMaxAge.Should().Be(31536000); // default
    }

    [Fact]
    public void UseCompression_True_ShouldRegisterNuglifyCompressor()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new AstraConfiguration { UseCompression = true };

        // Act
        services.AddSingleton(configuration);
        services.AddSingleton<IResourceCompressor, NuglifyResourceCompressor>();

        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var compressor = serviceProvider.GetService<IResourceCompressor>();
        compressor.Should().NotBeNull();
        compressor.Should().BeOfType<NuglifyResourceCompressor>();
    }

    [Fact]
    public void UseCompression_False_ShouldRegisterStubCompressor()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new AstraConfiguration { UseCompression = false };

        // Act
        services.AddSingleton(configuration);
        services.AddSingleton<IResourceCompressor, StubCompressor>();

        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var compressor = serviceProvider.GetService<IResourceCompressor>();
        compressor.Should().NotBeNull();
        compressor.Should().BeOfType<StubCompressor>();
    }

    [Fact]
    public void ResourcesRoute_CustomValue_ShouldBeUsedInResourceUrls()
    {
        // Arrange
        var config = new AstraConfiguration { ResourcesRoute = "my-assets" };
        var resourceStorage = new Mock<IResourceStorage>();
        var renderer = new HtmlResourceRenderer(resourceStorage.Object, config);

        // Act
        var url = renderer.GetResourceUrl("test.js");

        // Assert
        url.Should().StartWith("/my-assets/test.js");
    }

    [Fact]
    public void EnableVersioning_TrueWithoutResourceVersion_ShouldUseContentHash()
    {
        // Arrange
        var config = new AstraConfiguration
        {
            EnableVersioning = true,
            ResourceVersion = null
        };
        var resourceStorage = new Mock<IResourceStorage>();
        var renderer = new HtmlResourceRenderer(resourceStorage.Object, config);

        // First register a resource to trigger hash calculation
        var testJsCode = "console.log('test');";
        resourceStorage.Setup(x => x.Contains("test.js")).Returns(true);
        resourceStorage.Setup(x => x.OpenRead("test.js")).Returns(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(testJsCode)));
        renderer.AddJsCode("test.js", testJsCode);

        // Act
        var url = renderer.GetResourceUrl("test.js");

        // Assert
        url.Should().Contain("?v=");
        url.Should().MatchRegex(@"\?v=[a-zA-Z0-9_-]{32}$"); // Base64 URL-safe hash truncated to 32 chars
    }

    [Fact]
    public void EnableVersioning_TrueWithResourceVersion_ShouldUseSpecifiedVersion()
    {
        // Arrange
        var config = new AstraConfiguration
        {
            EnableVersioning = true,
            ResourceVersion = "2.1.0"
        };
        var resourceStorage = new Mock<IResourceStorage>();
        var renderer = new HtmlResourceRenderer(resourceStorage.Object, config);

        // Act
        var url = renderer.GetResourceUrl("test.js");

        // Assert
        url.Should().Be("/astra/test.js?v=2.1.0");
    }

    [Fact]
    public void EnableVersioning_False_ShouldNotAddVersionToUrl()
    {
        // Arrange
        var config = new AstraConfiguration
        {
            EnableVersioning = false,
            ResourceVersion = "1.0.0"
        };
        var resourceStorage = new Mock<IResourceStorage>();
        var renderer = new HtmlResourceRenderer(resourceStorage.Object, config);

        // Act
        var url = renderer.GetResourceUrl("test.js");

        // Assert
        url.Should().Be("/astra/test.js");
        url.Should().NotContain("?v=");
    }
}
