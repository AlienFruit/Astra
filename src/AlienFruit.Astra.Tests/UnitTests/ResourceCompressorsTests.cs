using AlienFruit.Astra.Abstractions;
using AlienFruit.Astra.Core.ResourceCompressors;
using AlienFruit.Astra.Models;
using FluentAssertions;
using System.Text;

namespace AlienFruit.Astra.Tests.UnitTests;

public class ResourceCompressorsTests
{
    private class TestResource : Resource
    {
        private readonly string _content;

        public TestResource(string name, string content) : base(name)
        {
            _content = content;
        }

        public override Stream GetStream()
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(_content));
        }
    }

    [Fact]
    public void CompressToString_OnStubCompressor_ShouldReturnOriginalContent()
    {
        // Arrange
        var compressor = new StubCompressor();
        var resource = new TestResource("test.js", "console.log('test');");

        // Act
        var result = compressor.CompressToString(resource);

        // Assert
        result.Should().Be("console.log('test');");
    }

    [Fact]
    public void CompressToStream_OnStubCompressor_ShouldReturnOriginalStream()
    {
        // Arrange
        var compressor = new StubCompressor();
        var originalContent = "console.log('test');";
        var resource = new TestResource("test.js", originalContent);

        // Act
        var resultStream = compressor.CompressToStream(resource);
        using var reader = new StreamReader(resultStream);
        var result = reader.ReadToEnd();

        // Assert
        result.Should().Be(originalContent);
    }

    [Fact]
    public void CompressToString_OnJavaScriptResource_ShouldCompress()
    {
        // Arrange
        var compressor = new NuglifyResourceCompressor();
        var originalJs = "function test ( ) { console . log ( 'hello' ) ; }";
        var resource = new TestResource("test.js", originalJs);

        // Act
        var result = compressor.CompressToString(resource);

        // Assert
        result.Should().Be("function test(){console.log(\"hello\")}");
    }

    [Fact]
    public void CompressToString_OnCssResource_ShouldCompress()
    {
        // Arrange
        var compressor = new NuglifyResourceCompressor();
        var originalCss = ".class { color : red ; margin : 10px ; }";
        var resource = new TestResource("test.css", originalCss);

        // Act
        var result = compressor.CompressToString(resource);

        // Assert
        result.Should().Be(".class{color:#f00;margin:10px}");
    }

    [Fact]
    public void CompressToString_OnHtmlResource_ShouldCompress()
    {
        // Arrange
        var compressor = new NuglifyResourceCompressor();
        var originalHtml = "<div class=\"test\"> <p> Hello </p> </div>";
        var resource = new TestResource("test.html", originalHtml);

        // Act
        var result = compressor.CompressToString(resource);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        // HTML compression should remove extra whitespace
        result.Should().NotContain("  ");
    }

    [Fact]
    public void CompressToString_OnUnknownResourceType_ShouldReturnOriginal()
    {
        // Arrange
        var compressor = new NuglifyResourceCompressor();
        var originalContent = "some unknown content";
        var resource = new TestResource("test.unknown", originalContent);

        // Act
        var result = compressor.CompressToString(resource);

        // Assert
        result.Should().Be(originalContent);
    }
}
