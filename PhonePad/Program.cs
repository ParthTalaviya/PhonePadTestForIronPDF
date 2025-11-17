using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OldPhoneKeypad.Interface;
using OldPhoneKeypad.Service;

namespace PhonePadApp;

/// <summary>
/// 
/// </summary>
public class Program
    {
    /// <summary>
    /// The phone pad service
    /// </summary>
    private readonly IPhonePadService _phonePadService;
    /// <summary>
    /// The logger
    /// </summary>
    private readonly ILogger<Program> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="Program"/> class.
    /// </summary>
    /// <param name="phonePadService">The phone pad service.</param>
    /// <param name="logger">The logger.</param>
    /// <exception cref="System.ArgumentNullException">
    /// phonePadService
    /// or
    /// logger
    /// </exception>
    public Program(IPhonePadService phonePadService, ILogger<Program> logger)
        {
        _phonePadService = phonePadService ?? throw new ArgumentNullException(nameof(phonePadService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

    /// <summary>
    /// Mains the specified arguments.
    /// </summary>
    /// <param name="args">The arguments.</param>
    /// <returns></returns>
    public static int Main(string[] args)
        {
        #region Setup Host and DI
        using IHost host = Host.CreateDefaultBuilder(args)
            .ConfigureLogging((context, logging) =>
            {
                logging.ClearProviders();
                logging.AddConfiguration(context.Configuration.GetSection("Logging"));
                logging.AddConsole();
            })
            .ConfigureServices((_, services) =>
            {
                // PhonePadService; register as singleton to avoid any transient state.
                services.AddSingleton<IPhonePadService, PhonePadService>();

                // Register Program so it can be constructed with DI
                services.AddTransient<Program>();
            })
            .Build();

        var program = host.Services.GetRequiredService<Program>();
        #endregion

        try
            {
            program.PhonePad();
            }
        catch (Exception ex)
            {
            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            logger.LogCritical(ex, "Unhandled exception in application");
            }

        return 0;
        }

    /// <summary>
    /// Olds the phone pad.
    /// </summary>
    public void PhonePad()
        {
        _logger.LogInformation("Starting Phone Pad Translator");
        Console.WriteLine("Phone Pad Translator");
        Console.WriteLine("Allowed characters: digits 0-9, space (for pause), '*' (for backspace), '#' (for end).");
        Console.WriteLine("Type 'Q' to quit.\n");

        while (true)
            {
            Console.Write("Enter phone-pad input: ");
            string? inputRaw = Console.ReadLine();

            #region Validate Input
            if (inputRaw is null)
                {
                _logger.LogInformation("Input is blank. Please insert some input");
                break;
                }

            string inputData = inputRaw.Trim();

            if (char.Equals(inputData, "Q"))
                {
                _logger.LogInformation("Quit command received.");
                break;
                }

            if (inputData.Length == 0)
                {
                Console.WriteLine("Empty input. Please enter a sequence (or 'Q').");
                continue;
                }

            // Validate allowed characters
            var invalidChars = inputData
                .Where(ch => !(char.IsDigit(ch) || ch == ' ' || ch == '*' || ch == '#'))
                .Distinct()
                .ToArray();

            if (invalidChars.Length > 0)
                {
                _logger.LogWarning("User input have invalid characters: {InvalidChars}", string.Join(", ", invalidChars));
                Console.WriteLine($"Invalid character(s) in input: {string.Join(", ", invalidChars.Select(chars => $"'{chars}'"))}");
                continue;
                }
            #endregion

            #region Append # as end marker
            // Ensure there's an end marker; offer to append automatically if missing
            if (!inputData.Contains('#'))
                {
                Console.Write("Input does not contain end marker '#'. Append '#' automatically? (Y/N): ");
                var key = Console.ReadKey(intercept: true);
                Console.WriteLine();
                if (key.Key == ConsoleKey.N)
                    {
                    _logger.LogInformation("User opted not to auto-append # end marker.");
                    Console.WriteLine("Please include a terminating '#' and try again.");
                    continue;
                    }

                inputData += "#";
                _logger.LogInformation("Appended # end marker automatically. New input: {Input}", inputData);
                Console.WriteLine($"Updated input: \"{inputData}\"");
                }
            #endregion

            #region Process the input summary
            // Simple classification summary (helps verify what user provided)
            int digits = inputData.Count(char.IsDigit);
            int spaces = inputData.Count(c => c == ' ');
            int backspaces = inputData.Count(c => c == '*');
            int ends = inputData.Count(c => c == '#');

            // Process and print result
            try
                {
                string output = _phonePadService.PhonePadBuilder(inputData);
                _logger.LogInformation("Processing completed successfully. Output = {Output}", output);
                Console.WriteLine($"Output: \"{output}\"");
                }
            catch (ArgumentException aex)
                {
                _logger.LogWarning(aex, "Validation error when processing input: {Input}", inputData);
                Console.WriteLine($"Invalid input: {aex.Message}");
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error processing input: {Input}", inputData);
                Console.WriteLine($"Processing failed due to: {ex.Message}");
                }

            Console.WriteLine(new string('-', 40));
            #endregion
            }
        _logger.LogInformation("Exit application.");
        Console.WriteLine("Exit application.");
        }
    }