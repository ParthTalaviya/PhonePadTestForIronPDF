# PhonePad Translator (Old Phone Keypad)
Concise, production-ready console app and test-suite that translates legacy phone-keypad sequences (digits, space, `*`, `#`) into text. Implemented with modern .NET patterns (Host, DI, Logging) and designed for clarity, testability and interview discussion.

## Quick facts
- Language: C# 12
- Target framework: .NET 8
- Projects:
  - `PhonePad` — console app and host wiring (`Program.cs`)
  - `PhonePad.Service` — core translator (`PhonePad\Service\PhonePadService.cs`)
  - `PhonePad.Interface` — `IPhonePadService` contract (`PhonePad\Interface\IPhonePadService.cs`)
  - `PhonePadTestCases` — unit tests (`PhonePadTestCases\PhonePadServiceTests.cs`)
- Build tools: Visual Studio 2022 or `dotnet` CLI

## What it does
Converts a compact legacy keypad input into text:
- Digits `0`..`9` map to characters (`2` => `ABC`, `3` => `DEF`, etc.)
- Repeated presses cycle through letters (wraps around)
- Space (` `) acts as a pause / word separator
- Asterisk (`*`) is a backspace (removes committed character)
- Hash (`#`) is the committed end marker; trailing buffered key without `#` is not committed
- Non-digit characters (other than ` `, `*`, `#`) are ignored

## Design highlights (concise)
- Single responsibility: `PhonePadService` implements translation and exposes `PhonePadBuilder(string)`.
- Dependency Injection: service registered via the Host (`Program.cs`) as `Singleton` to avoid transient state bugs.
- Logging: `Microsoft.Extensions.Logging` used for diagnostics.
- Test coverage: focused unit tests in `PhonePadTestCases` exercise valid conversions, buffering, wrap behavior, backspace semantics and invalid inputs.
- Safety: defensive input validation and exception logging.

## How it works (brief algorithm)
- Iterate input char-by-char.
- Buffer the current digit and how many times it's pressed consecutively.
- On change of digit, space, `*`, or `#`, commit the buffered digit:
  - Use mapping table `string[] DigitMap` where index = numeric digit.
  - Compute letter index as `(pressCount - 1) % mapping.Length` to implement wrap-around.
- `*` commits current buffer and then removes last committed character if any.
- `#` commits current buffer and terminates processing.

## Examples (taken from unit tests)
- Input: `4433555 555666#` => Output: `HELLO`
- Input: `8 88777444666*664#` => Output: `TURING`
- Input: `33#` => Output: `E`
- Input: `222 2 22#` => Output: `CAB`
- Trailing buffer without `#` is ignored: `2` => `""` (empty)

## Run locally

Using Visual Studio 2022
1. Open the solution.
2. Build the solution.
3. Start the console app using __Start Debugging__ or __Debug > Start Without Debugging__.
4. To run tests, open __Test Explorer__.

Using dotnet CLI
- Build:
  - `dotnet build`
- Run console app:
  - `dotnet run --project PhonePad`
- Run tests:
  - `dotnet test`

## Unit tests
Tests are in `PhonePadTestCases\PhonePadServiceTests.cs`. They rely on `NullLogger<PhonePadService>` to construct the service and cover:
- Normal translations
- Null/empty input behavior
- Missing terminator behavior
- Wrap-around presses
- Ignored invalid characters
- Backspace edge cases

## Public Access
- `string PhonePadBuilder(string input)` — translates the provided input into the output string. Returns `string.Empty` for `null` or empty input.

Example usage:
## Known behaviors & edge-cases
- The service only commits characters on delimiters (` `, `*`, `#`) or when the pressed digit changes. A trailing buffer with no `#` is intentionally not committed — this mirrors many legacy behaviors and prevents accidental commits.
- Wrap-around is done via modulo arithmetic so pressing beyond the number of letters cycles.
- Unsupported characters are ignored during processing; logging is used for diagnostics.
- Service is safe to register as singleton because internal state is not retained between calls (buffers are local to the call).

Details of Files:
- `PhonePad\Service\PhonePadService.cs` — translator implementation
- `PhonePad\Program.cs` — console host + DI wiring
- `PhonePad\Interface\IPhonePadService.cs` — public contract
- `PhonePadTestCases\PhonePadServiceTests.cs` — unit tests