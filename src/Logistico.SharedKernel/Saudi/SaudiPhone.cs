namespace Logistico.SharedKernel.Saudi;

/// <summary>Saudi mobile numbers (STC/Mobily/Zain, 05XXXXXXXX) normalised to +9665XXXXXXXX.</summary>
public static class SaudiPhone
{
    public static string? Normalize(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }

        var digits = new string(SearchText.LatinDigits(input)!.Where(char.IsAsciiDigit).ToArray());
        if (digits.StartsWith("00966", StringComparison.Ordinal))
        {
            digits = digits[5..];
        }
        else if (digits.StartsWith("966", StringComparison.Ordinal))
        {
            digits = digits[3..];
        }
        else if (digits.StartsWith('0'))
        {
            digits = digits[1..];
        }

        return digits.Length == 9 && digits[0] == '5' ? "+966" + digits : null;
    }

    public static bool IsValid(string? input) => Normalize(input) is not null;

    /// <summary>Local display form: 05X XXX XXXX.</summary>
    public static string Display(string? e164) =>
        e164 is { Length: 13 } && e164.StartsWith("+9665", StringComparison.Ordinal)
            ? $"0{e164[4..6]} {e164[6..9]} {e164[9..]}"
            : e164 ?? "";
}
