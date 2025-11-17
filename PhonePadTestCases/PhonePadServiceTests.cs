using Microsoft.Extensions.Logging.Abstractions;
using OldPhoneKeypad.Service;

namespace ConsoleApp1.Tests;

/// <summary>
/// 
/// </summary>
public class PhonePadServiceTests
    {
    /// <summary>
    /// Creates the service.
    /// </summary>
    /// <returns></returns>
    private static PhonePadService CreateService() =>
        new PhonePadService(NullLogger<PhonePadService>.Instance);

    /// <summary>
    /// Translates the valid inputs returns expected.
    /// </summary>
    /// <param name="input">The input.</param>
    /// <param name="expected">The expected.</param>
    [Theory]
    [InlineData("33#", "E")]
    [InlineData("227*#", "B")]
    [InlineData("4433555 555666#", "HELLO")]
    [InlineData("8 88777444666*664#", "TURING")]
    [InlineData("222 2 22#", "CAB")]
    public void Translate_ValidInputs_ReturnsExpected(string input, string expected)
        {
        var svc = CreateService();
        var actual = svc.PhonePadBuilder(input);
        Assert.Equal(expected, actual);
        }

    /// <summary>
    /// Nulls the input returns empty string.
    /// </summary>
    [Fact]
    public void NullInput_ReturnsEmptyString()
        {
        var svc = CreateService();
        string result = svc.PhonePadBuilder(null!);
        Assert.Equal(string.Empty, result);
        }

    /// <summary>
    /// Empties the input returns empty string.
    /// </summary>
    [Fact]
    public void EmptyInput_ReturnsEmptyString()
        {
        var svc = CreateService();
        string result = svc.PhonePadBuilder(string.Empty);
        Assert.Equal(string.Empty, result);
        }

    /// <summary>
    /// Missings the terminator does not commit buffered key.
    /// </summary>
    [Fact]
    public void MissingTerminator_DoesNotCommitBufferedKey()
        {
        // Implementation only commits on change / special chars / '#'.
        // A raw trailing buffer without '#' will not be committed.
        var svc = CreateService();
        string result = svc.PhonePadBuilder("2"); // no '#'
        Assert.Equal(string.Empty, result);
        }

    /// <summary>
    /// Wraps the around presses wraps correctly.
    /// </summary>
    [Fact]
    public void WrapAroundPresses_WrapsCorrectly()
        {
        var svc = CreateService();
        // '2' => "ABC", pressing 4 times should wrap to 'A'
        string result = svc.PhonePadBuilder("2222#");
        Assert.Equal("A", result);
        }

    /// <summary>
    /// Invalids the characters are ignored but digits commit.
    /// </summary>
    [Fact]
    public void InvalidCharacters_AreIgnoredButDigitsCommit()
        {
        var svc = CreateService();
        // '2' => 'A', 'a' ignored, '3' => 'D'
        string result = svc.PhonePadBuilder("2a3#");
        Assert.Equal("AD", result);
        }

    /// <summary>
    /// Backspaces the when output empty no exception and empty result.
    /// </summary>
    [Fact]
    public void BackspaceWhenOutputEmpty_NoExceptionAndEmptyResult()
        {
        var svc = CreateService();
        string result = svc.PhonePadBuilder("*#");
        Assert.Equal(string.Empty, result);
        }

    /// <summary>
    /// Multiples the backspaces remove committed characters.
    /// </summary>
    [Fact]
    public void MultipleBackspaces_RemoveCommittedCharacters()
        {
        var svc = CreateService();
        // 44 -> 'I', 33 -> 'F' => "IF", then '*' removes 'F', another '*' removes 'I'
        string result = svc.PhonePadBuilder("4433**#");
        Assert.Equal(string.Empty, result);
        }
    }