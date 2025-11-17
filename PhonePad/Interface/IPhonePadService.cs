namespace OldPhoneKeypad.Interface;

/// <summary>
/// Defines the contract for processing legacy phone pad input strings.
/// Adheres to the Dependency Inversion Principle (DIP) and Interface Segregation Principle (ISP).
/// </summary>
public interface IPhonePadService
    {

    /// <summary>
    /// Olds the phone pad builder.
    /// </summary>
    /// <param name="input">The input.</param>
    /// <returns></returns>
    public string PhonePadBuilder(string input);
    }