using Logistico.SharedKernel.Geo;
using Logistico.SharedKernel.Saudi;

namespace Logistico.Modules.Shipments.Tests;

/// <summary>Saudi mobile numbers are accepted in their local forms and stored as E.164.</summary>
public class SaudiPhoneTests
{
    [Theory]
    [InlineData("0551234567", "+966551234567")]        // local, leading zero
    [InlineData("966551234567", "+966551234567")]       // country code, no plus
    [InlineData("00966551234567", "+966551234567")]     // international prefix
    [InlineData("+966 55 123 4567", "+966551234567")]   // spaced, with plus
    [InlineData("٠٥٥١٢٣٤٥٦٧", "+966551234567")]          // Arabic-Indic digits
    [InlineData("0501234567", "+966501234567")]         // another operator prefix
    public void Local_and_international_forms_normalise(string input, string expected) =>
        Assert.Equal(expected, SaudiPhone.Normalize(input));

    [Theory]
    [InlineData("0771234567")]   // Iraqi mobile, not Saudi
    [InlineData("0661234567")]   // does not start with 5
    [InlineData("05512345")]     // too short
    [InlineData("")]
    public void Non_saudi_numbers_are_rejected(string input) => Assert.Null(SaudiPhone.Normalize(input));

    [Fact]
    public void Displays_in_the_local_form() => Assert.Equal("055 123 4567", SaudiPhone.Display("+966551234567"));

    [Fact]
    public void The_shared_phone_helper_accepts_both_countries()
    {
        Assert.Equal("+966551234567", PhoneNumbers.Normalize("0551234567"));   // Saudi
        Assert.Equal("+9647701234567", PhoneNumbers.Normalize("07701234567")); // Iraqi still works
        Assert.Equal("055 123 4567", PhoneNumbers.Display("+966551234567"));
        Assert.Equal("0770 123 4567", PhoneNumbers.Display("+9647701234567"));
    }
}
