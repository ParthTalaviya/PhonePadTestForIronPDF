        using Microsoft.Extensions.Logging;
using OldPhoneKeypad.Interface;
using System.Text;

namespace OldPhoneKeypad.Service;


/// <summary>
/// 
/// </summary>
/// <seealso cref="OldPhoneKeypad.Interface.IPhonePadService" />
public sealed class PhonePadService : IPhonePadService
    {
    // Mapping for keys '0'..'9' by index (index = key - 0).
    /// <summary>
    /// The digit map
    /// </summary>
    private static readonly string[] DigitMap = new[]
    {
        " ",    // '0'
        "&",    // '1'
        "ABC",  // '2'
        "DEF",  // '3'
        "GHI",  // '4'
        "JKL",  // '5'
        "MNO",  // '6'
        "PQRS", // '7'
        "TUV",  // '8'
        "WXYZ", // '9'
    };

    /// <summary>
    /// The logger
    /// </summary>
    private readonly ILogger<PhonePadService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PhonePadService" /> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <exception cref="System.ArgumentNullException">logger</exception>
    public PhonePadService(ILogger<PhonePadService> logger)
        {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


    /// <summary>
    /// Olds the phone pad builder.
    /// </summary>
    /// <param name="inputString">The input string.</param>
    /// <returns></returns>
    public string PhonePadBuilder(string inputString)
        {
        #region Validate Input
        if (string.IsNullOrEmpty(inputString))
            {
            _logger.LogWarning("PhonePad Builder Service Method called with null input.");
            return string.Empty;
            }
        #endregion

        #region Process the core logic
        try
            {
            var outputString = new StringBuilder(Math.Max(8, inputString.Length / 2));

            // Current buffered key and how many times it was pressed consecutively.
            char currentKey = '\0';
            int pressCount = 0;

            for (int i = 0; i < inputString.Length; i++)
                {
                char character = inputString[i];

                switch (character)
                    {
                    case ' ':
                        // Separator: Separate the word (if any)
                        CommitBufferedKey(outputString, ref currentKey, ref pressCount);
                        break;

                    case '#':
                        // End the process and exit
                        CommitBufferedKey(outputString, ref currentKey, ref pressCount);
                        i = inputString.Length; // break out of loop
                        break;

                    case '*':
                        // Backspace: remove last char from output if exists
                        CommitBufferedKey(outputString, ref currentKey, ref pressCount);
                        if (outputString.Length > 0)
                            {
                            outputString.Length--;
                            }
                        break;

                    default:
                        if (char.IsDigit(character))
                            {
                            if (currentKey == '\0')
                                {
                                // Start a new sequence
                                currentKey = character;
                                pressCount = 1;
                                }
                            else if (currentKey == character)
                                {
                                // Continue the same key sequence
                                pressCount++;
                                }
                            else
                                {
                                // Different digit: added previous and start new
                                CommitBufferedKey(outputString, ref currentKey, ref pressCount);
                                currentKey = character;
                                pressCount = 1;
                                }
                            }
                        // other characters are ignored
                        break;
                    }
                }
            
            return outputString.ToString();
            #endregion
            }
        catch (Exception ex)
            {
            _logger.LogError(ex, "Unhandled exception while processing PhonePad Builder Method input: {Input}", inputString);
            throw;
            }
        }


    /// <summary>
    /// Commits the buffered key.
    /// </summary>
    /// <param name="output">The output.</param>
    /// <param name="currentKey">The current key.</param>
    /// <param name="pressCount">The press count.</param>
    private void CommitBufferedKey(StringBuilder output, ref char currentKey, ref int pressCount)
        {
        if (pressCount == 0 || currentKey == '\0')
            {
            // Nothing to add
            currentKey = '\0';
            pressCount = 0;
            return;
            }

        // Only digits '0'..'9' map to letters/space in our mapping.
        if (currentKey >= '0' && currentKey <= '9')
            {
            string mappingKey = DigitMap[currentKey - '0'];

            if (!string.IsNullOrEmpty(mappingKey))
                {
                // Compute index using zero-based (pressCount - 1) and wrap it.
                int index = (pressCount - 1) % mappingKey.Length;
                output.Append(mappingKey[index]);
                }
            else
                {
                // Empty Mapping
                output.Append('?');
                _logger.LogWarning("Mapping for key '{Key}' is empty", currentKey);
                }
            }
        else
            {
            // Fallback for any unsupported key
            output.Append('!');
            _logger.LogWarning("Unsupported key encountered in buffer: {Key}", currentKey);
            }

        // Clear buffer
        currentKey = '\0';
        pressCount = 0;
        }
    }