using Logistico.SharedKernel.Iraq;
using Logistico.SharedKernel.Saudi;

namespace Logistico.SharedKernel.Geo;

/// <summary>
/// A phone the company can call: an Iraqi or Saudi mobile in any of the local forms, or an international number written
/// with a leading plus (E.164). Stored as E.164.
/// </summary>
public static class PhoneNumbers
{
    public static string? Normalize(string? input)
    {
        if (IraqPhone.Normalize(input) is { } iraqi)
        {
            return iraqi;
        }

        if (SaudiPhone.Normalize(input) is { } saudi)
        {
            return saudi;
        }

        if (string.IsNullOrWhiteSpace(input) || !input.Trim().StartsWith('+'))
        {
            return null;
        }

        var digits = new string(input.Where(char.IsAsciiDigit).ToArray());
        return digits.Length is >= 8 and <= 15 ? "+" + digits : null;
    }

    public static bool IsValid(string? input) => Normalize(input) is not null;

    /// <summary>Iraqi and Saudi numbers in their local form; anything else stays as stored.</summary>
    public static string Display(string? e164) =>
        e164 is not null && e164.StartsWith("+9665", StringComparison.Ordinal) ? SaudiPhone.Display(e164) : IraqPhone.Display(e164);
}
