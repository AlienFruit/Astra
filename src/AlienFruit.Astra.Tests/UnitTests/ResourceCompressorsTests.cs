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
    public void StubCompressor_CompressToString_ShouldReturnOriginalContent()
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
    public void StubCompressor_CompressToStream_ShouldReturnOriginalStream()
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
    public void NuglifyResourceCompressor_CompressToString_JavaScript_ShouldCompress()
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
    public void NuglifyResourceCompressor_CompressToString_Css_ShouldCompress()
    {
        // Arrange
        var compressor = new NuglifyResourceCompressor();
        var originalCss = ".class { color : red ; margin : 10px ; }";
        var resource = new TestResource("test.css", originalCss);

        // Act
        var result = compressor.CompressToString(resource);

        // Assert
        result.Should().Be(".class{color:red;margin:10px}");
    }

    [Fact]
    public void NuglifyResourceCompressor_CompressToString_Html_ShouldCompress()
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
    public void NuglifyResourceCompressor_CompressToString_UnknownType_ShouldReturnOriginal()
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
