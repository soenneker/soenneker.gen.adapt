using Soenneker.Tests.Unit;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Soenneker.Gen.Adapt.Tests.Dtos;
using AwesomeAssertions;

namespace Soenneker.Gen.Adapt.Tests;

public sealed class EncodingTests : UnitTest
{
    public EncodingTests( output) : base(output)
    {
    }

    [Test]
    public void Adapt_Unicode_MultipleLanguages_ShouldMap()
    {
        // Arrange
        var source = new BasicSource 
        { 
            Id = "äöüß€£¥", 
            Name = "مرحبا שלום 你好 こんにちは", 
            Count = 42 
        };

        // Act
        var result = source.Adapt<BasicDest>();

        // Assert
        result.Id.Should().Be("äöüß€£¥");
        result.Name.Should().Be("مرحبا שלום 你好 こんにちは");
    }

    [Test]
    public void Adapt_UnicodeEmojis_ShouldMapCorrectly()
    {
        // Arrange
        var source = new BasicSource { Id = "🚀", Name = "🎉🎊", Count = 1 };

        // Act
        var result = source.Adapt<BasicDest>();

        // Assert
        result.Id.Should().Be("🚀");
        result.Name.Should().Be("🎉🎊");
    }

    [Test]
    public void Adapt_StringWithSurrogatesPairs_ShouldMap()
    {
        // Arrange
        var source = new BasicSource 
        { 
            Id = "🎉",
            Name = "𝐇𝐞𝐥𝐥𝐨",
            Count = 1 
        };

        // Act
        var result = source.Adapt<BasicDest>();

        // Assert
        result.Id.Should().Be("🎉");
        result.Name.Should().Be("𝐇𝐞𝐥𝐥𝐨");
    }

    [Test]
    public void Adapt_StringWithCombiningCharacters_ShouldMap()
    {
        // Arrange
        var source = new BasicSource 
        { 
            Id = "e\u0301",
            Name = "n\u0303",
            Count = 1 
        };

        // Act
        var result = source.Adapt<BasicDest>();

        // Assert
        result.Id.Should().Be("e\u0301");
        result.Name.Should().Be("n\u0303");
    }

    [Test]
    public void Adapt_StringWithBidiControlCharacters_ShouldMap()
    {
        // Arrange
        var source = new BasicSource 
        { 
            Id = "\u202Etest\u202C",
            Name = "\u202Ahello\u202C",
            Count = 1 
        };

        // Act
        var result = source.Adapt<BasicDest>();

        // Assert
        result.Id.Should().Be("\u202Etest\u202C");
        result.Name.Should().Be("\u202Ahello\u202C");
    }

    [Test]
    public void Adapt_ZeroWidthCharacters_ShouldMap()
    {
        // Arrange
        var source = new BasicSource 
        { 
            Id = "test\u200B\u200C\u200D",
            Name = "invisible\uFEFF",
            Count = 1 
        };

        // Act
        var result = source.Adapt<BasicDest>();

        // Assert
        result.Id.Should().Be("test\u200B\u200C\u200D");
        result.Name.Should().Be("invisible\uFEFF");
    }

    [Test]
    public void Adapt_StringWithNullCharacter_ShouldMap()
    {
        // Arrange
        var source = new BasicSource 
        { 
            Id = "before\0after", 
            Name = "\0\0\0", 
            Count = 0 
        };

        // Act
        var result = source.Adapt<BasicDest>();

        // Assert
        result.Id.Should().Be("before\0after");
        result.Name.Should().Be("\0\0\0");
    }

    [Test]
    public void Adapt_AllEscapeSequences_ShouldMap()
    {
        // Arrange
        var source = new BasicSource 
        { 
            Id = "\r\n\t\b\f\v\a", 
            Name = "\\\'\"\0", 
            Count = 1 
        };

        // Act
        var result = source.Adapt<BasicDest>();

        // Assert
        result.Id.Should().Be("\r\n\t\b\f\v\a");
        result.Name.Should().Be("\\\'\"\0");
    }

    [Test]
    public void Adapt_StringWithNonPrintableCharacters_ShouldMap()
    {
        // Arrange
        var source = new BasicSource 
        { 
            Id = "\u0001\u0002\u0003",
            Name = "\u001B[31mRed\u001B[0m",
            Count = 1 
        };

        // Act
        var result = source.Adapt<BasicDest>();

        // Assert
        result.Id.Should().Be("\u0001\u0002\u0003");
        result.Name.Should().Be("\u001B[31mRed\u001B[0m");
    }

    [Test]
    public void Adapt_StringLooksLikeEscapeSequence_ShouldMapLiterally()
    {
        // Arrange
        var source = new BasicSource 
        { 
            Id = "\\n\\t\\r",
            Name = "\\u0041",
            Count = 1 
        };

        // Act
        var result = source.Adapt<BasicDest>();

        // Assert
        result.Id.Should().Be("\\n\\t\\r");
        result.Name.Should().Be("\\u0041");
    }

    [Test]
    public void Adapt_StringWithXmlEscapeCharacters_ShouldMap()
    {
        // Arrange
        var source = new BasicSource 
        { 
            Id = "<tag>content</tag>",
            Name = "&amp;&lt;&gt;&quot;&#39;",
            Count = 1 
        };

        // Act
        var result = source.Adapt<BasicDest>();

        // Assert
        result.Id.Should().Be("<tag>content</tag>");
        result.Name.Should().Be("&amp;&lt;&gt;&quot;&#39;");
    }

    [Test]
    public void Adapt_StringWithJsonEscapeCharacters_ShouldMap()
    {
        // Arrange
        var source = new BasicSource 
        { 
            Id = "{\"key\":\"value\"}",
            Name = "Line1\nLine2\tTabbed",
            Count = 1 
        };

        // Act
        var result = source.Adapt<BasicDest>();

        // Assert
        result.Id.Should().Be("{\"key\":\"value\"}");
        result.Name.Should().Be("Line1\nLine2\tTabbed");
    }

    [Test]
    public void Adapt_StringWithSqlInjectionAttempt_ShouldMapSafely()
    {
        // Arrange
        var source = new BasicSource 
        { 
            Id = "'; DROP TABLE Users;--",
            Name = "1' OR '1'='1",
            Count = 1 
        };

        // Act
        var result = source.Adapt<BasicDest>();

        // Assert
        result.Id.Should().Be("'; DROP TABLE Users;--");
        result.Name.Should().Be("1' OR '1'='1");
    }

    [Test]
    public void Adapt_StringWithScriptTags_ShouldMap()
    {
        // Arrange
        var source = new BasicSource 
        { 
            Id = "<script>alert('XSS')</script>",
            Name = "<img src=x onerror=alert(1)>",
            Count = 1 
        };

        // Act
        var result = source.Adapt<BasicDest>();

        // Assert
        result.Id.Should().Be("<script>alert('XSS')</script>");
        result.Name.Should().Be("<img src=x onerror=alert(1)>");
    }

    [Test]
    public void Adapt_StringWithPathTraversal_ShouldMap()
    {
        // Arrange
        var source = new BasicSource 
        { 
            Id = "../../etc/passwd",
            Name = "..\\..\\windows\\system32",
            Count = 1 
        };

        // Act
        var result = source.Adapt<BasicDest>();

        // Assert
        result.Id.Should().Be("../../etc/passwd");
        result.Name.Should().Be("..\\..\\windows\\system32");
    }

    [Test]
    public void Adapt_StringWithFormatSpecifiers_ShouldMapLiterally()
    {
        // Arrange
        var source = new BasicSource 
        { 
            Id = "{0} {1} {2}",
            Name = "%s %d %f",
            Count = 1 
        };

        // Act
        var result = source.Adapt<BasicDest>();

        // Assert
        result.Id.Should().Be("{0} {1} {2}");
        result.Name.Should().Be("%s %d %f");
    }

    [Test]
    public void Adapt_StringWithRegexMetacharacters_ShouldMap()
    {
        // Arrange
        var source = new BasicSource 
        { 
            Id = ".*+?[]{}()^$|\\",
            Name = "\\d+\\.\\w*",
            Count = 1 
        };

        // Act
        var result = source.Adapt<BasicDest>();

        // Assert
        result.Id.Should().Be(".*+?[]{}()^$|\\");
        result.Name.Should().Be("\\d+\\.\\w*");
    }

    [Test]
    public void Adapt_StringWithUrlEncodedCharacters_ShouldMap()
    {
        // Arrange
        var source = new BasicSource 
        { 
            Id = "%20%21%22%23",
            Name = "hello%20world",
            Count = 1 
        };

        // Act
        var result = source.Adapt<BasicDest>();

        // Assert
        result.Id.Should().Be("%20%21%22%23");
        result.Name.Should().Be("hello%20world");
    }

    [Test]
    public void Adapt_StringWithBase64_ShouldMap()
    {
        // Arrange
        var source = new BasicSource 
        { 
            Id = "SGVsbG8gV29ybGQh",
            Name = "QmFzZTY0IGVuY29kZWQgc3RyaW5n",
            Count = 1 
        };

        // Act
        var result = source.Adapt<BasicDest>();

        // Assert
        result.Id.Should().Be("SGVsbG8gV29ybGQh");
        result.Name.Should().Be("QmFzZTY0IGVuY29kZWQgc3RyaW5n");
    }

    [Test]
    public void Adapt_StringWithOnlyWhitespace_VariousTypes_ShouldMap()
    {
        // Arrange
        var sources = new List<BasicSource>
        {
            new() { Id = "   ", Name = "\t\t\t", Count = 1 },
            new() { Id = "\n\n\n", Name = "\r\r\r", Count = 2 },
            new() { Id = " \t\n\r ", Name = "\v\f", Count = 3 }
        };

        // Act
        List<BasicDest> results = sources.Select(s => s.Adapt<BasicDest>()).ToList();

        // Assert
        results[0].Id.Should().Be("   ");
        results[0].Name.Should().Be("\t\t\t");
        results[1].Id.Should().Be("\n\n\n");
        results[1].Name.Should().Be("\r\r\r");
        results[2].Id.Should().Be(" \t\n\r ");
    }

    [Test]
    public void Adapt_StringWithRepeatingPattern_ShouldMap()
    {
        // Arrange
        var pattern = "abc123";
        string repeated = string.Concat(Enumerable.Repeat(pattern, 1000));
        var source = new BasicSource 
        { 
            Id = repeated,
            Name = repeated,
            Count = 1 
        };

        // Act
        var result = source.Adapt<BasicDest>();

        // Assert
        result.Id.Length.Should().Be(6000);
        result.Name.Length.Should().Be(6000);
        result.Id.Should().Be(repeated);
    }

    [Test]
    public void Adapt_UTF8_AllValidCodePoints_ShouldMap()
    {
        // Arrange
        var builder = new StringBuilder();
        builder.Append("Hello");
        builder.Append("Café");
        builder.Append("Привет");
        builder.Append("Γειά");
        builder.Append("你好世界");
        builder.Append("😀🎉🚀");

        var text = builder.ToString();
        var source = new BasicSource { Id = text, Name = text, Count = 1 };

        // Act
        var result = source.Adapt<BasicDest>();

        // Assert
        result.Id.Should().Be(text);
        result.Name.Should().Be(text);
    }
}

