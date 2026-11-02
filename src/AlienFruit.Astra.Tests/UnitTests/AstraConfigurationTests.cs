using AlienFruit.Astra.Core.ResourceCompressors;
using AlienFruit.Astra.DependencyInjection;
using AlienFruit.Astra.Tests.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text;

namespace AlienFruit.Astra.Tests.UnitTests;

public class AstraConfigurationTests : AstraTestBase
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
                ""ResourcesRoute"": ""custom"",
                ""EnableVersioning"": true,
                ""ResourceVersion"": ""1.0.0"",
                ""CacheMaxAge"": 86400
            }
        }";

        var configuration = new ConfigurationBuilder()
            .AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(json)))
            .Build();

        // Act
        var section = configuration.GetSection(AstraConfiguration.Name);
        var astraConfig = section.Get<AstraConfiguration>();

        // Assert
        astraConfig.Should().NotBeNull();
        astraConfig!.UseCompression.Should().BeFalse();
        astraConfig.ResourcesRoute.Should().Be("custom");
        astraConfig.EnableVersioning.Should().BeTrue();
        astraConfig.ResourceVersion.Should().Be("1.0.0");
        astraConfig.CacheMaxAge.Should().Be(86400);
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
            .AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(json)))
            .Build();

        // Act
        var section = configuration.GetSection(AstraConfiguration.Name);
        var astraConfig = section.Get<AstraConfiguration>();

        // Assert
        astraConfig.Should().NotBeNull();
        astraConfig.UseCompression.Should().BeFalse();
        astraConfig.ResourcesRoute.Should().Be("custom");
        astraConfig.EnableVersioning.Should().BeFalse(); // default
        astraConfig.ResourceVersion.Should().BeNull(); // default
        astraConfig.CacheMaxAge.Should().Be(31536000); // default
    }

    [Fact]
    public void AddAstra_UseCompressionTrue_RegistersNuglifyCompressor()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();

        // Act
        builder.AddAstra(options => options.UseCompression = true);
        var serviceProvider = builder.Services.BuildServiceProvider();

        // Assert
        serviceProvider.GetRequiredService<IResourceCompressor>().Should().BeOfType<NuglifyResourceCompressor>();
    }

    [Fact]
    public void AddAstra_UseCompressionFalse_RegistersStubCompressor()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();

        // Act
        builder.AddAstra(options => options.UseCompression = false);
        var serviceProvider = builder.Services.BuildServiceProvider();

        // Assert
        serviceProvider.GetRequiredService<IResourceCompressor>().Should().BeOfType<StubCompressor>();
    }

    [Fact]
    public void AddAstra_RegistersCoreAstraServices()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();

        // Act
        builder.AddAstra();
        var serviceProvider = builder.Services.BuildServiceProvider();

        // Assert
        serviceProvider.GetRequiredService<IResourceStorage>().Should().BeOfType<ResourceStorage>();
        serviceProvider.GetRequiredService<IInitScriptRenderer>().Should().BeOfType<InitScriptRenderer>();
        serviceProvider.GetRequiredService<AstraEngine>().Should().NotBeNull();
        serviceProvider.GetRequiredService<AstraConfiguration>().ResourcesRoute.Should().Be("astra");
    }

    [Fact]
    public void ResourcesRoute_CustomValue_ShouldBeUsedInResourceUrls()
    {
        // Arrange
        var config = new AstraConfiguration { ResourcesRoute = "my-assets" };
        var resourceStorage = new Mock<IResourceStorage>();
        var renderer = new HtmlResourceRenderer(resourceStorage.Object, config);
        renderer.AddResource("test.js", "test.js");

        // Act
        var html = GetContent(renderer.RenderHeaders());

        // Assert
        html.Should().Contain("/my-assets/test.js");
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
        resourceStorage.Setup(x => x.OpenRead("test.js")).Returns(new MemoryStream(Encoding.UTF8.GetBytes(testJsCode)));
        renderer.AddResource("test.js", "test.js");

        // Act
        var html = GetContent(renderer.RenderHeaders());

        // Assert
        html.Should().Contain("?v=");
        html.Should().MatchRegex(@"\?v=[a-f0-9]{64}");
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
        renderer.AddResource("test.js", "test.js");

        // Act
        var html = GetContent(renderer.RenderHeaders());

        // Assert
        html.Should().Contain("/astra/test.js?v=2.1.0");
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
        renderer.AddResource("test.js", "test.js");

        // Act
        var html = GetContent(renderer.RenderHeaders());

        // Assert
        html.Should().Contain("/astra/test.js");
        html.Should().NotContain("?v=");
    }
}
