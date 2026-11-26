using Logistico.Modules.Merchants.Contracts;
using Logistico.Modules.Merchants.Domain;
using Logistico.SharedKernel;
using Logistico.SharedKernel.Geo;
using Microsoft.EntityFrameworkCore;

namespace Logistico.Modules.Merchants.Infrastructure;

/// <summary>Development-only demo sellers: Instagram shops, in Iraq or in Saudi Arabia to match the operating country.</summary>
internal sealed class MerchantsDemoSeeder(MerchantsDbContext db, ITenantContext tenant, TimeProvider clock)
{
    private static readonly (string Name, string Handle, string Contact, string Phone, string Gov)[] IraqShops =
    [
        ("بوتيك لمار", "lamar.boutique", "لمار أحمد", "07701234501", "IQ-BG"),
        ("Baghdad Kicks", "baghdad_kicks", "Ali Hassan", "07801234502", "IQ-BG"),
        ("زهرة للتجميل", "zahra.cosmetics", "زهراء علي", "07501234503", "IQ-BA"),
        ("Erbil Gadgets", "erbil.gadgets", "Karwan Aziz", "07501234504", "IQ-AR"),
        ("عطور الفرات", "alfurat.perfumes", "حيدر كاظم", "07811234505", "IQ-NA"),
        ("Little Steps Kids", "littlesteps.iq", "Noor Salim", "07711234506", "IQ-BG"),
        ("مطبخ ماما", "mama.kitchen.iq", "أم يوسف", "07901234507", "IQ-KA"),
        ("Mosul Handmade", "mosul.handmade", "Sara Younis", "07721234508", "IQ-NI"),
        ("دار الأناقة", "dar.alanaqa", "مريم جاسم", "07831234509", "IQ-BB"),
        ("Phone Case Hub", "phonecase.hub", "Mustafa Raad", "07741234510", "IQ-BG"),
        ("كتب ومكتبة الرصيف", "alrasif.books", "عمر فاضل", "07851234511", "IQ-BG"),
        ("Suli Fashion", "suli.fashion", "Rozh Omer", "07501234512", "IQ-SU"),
    ];

    private static readonly (string Name, string Handle, string Contact, string Phone, string Gov)[] SaudiShops =
    [
        ("بوتيك لمار", "lamar.boutique", "لمار أحمد", "0551234501", "SA-01"),
        ("Riyadh Kicks", "riyadh_kicks", "عبدالله سعد", "0561234502", "SA-01"),
        ("زهرة للتجميل", "zahra.cosmetics", "زهراء علي", "0551234503", "SA-02"),
        ("Jeddah Gadgets", "jeddah.gadgets", "ريان عمر", "0561234504", "SA-02"),
        ("عطور الريان", "alrayan.perfumes", "فهد كاظم", "0551234505", "SA-01"),
        ("Little Steps Kids", "littlesteps.sa", "نورة سالم", "0551234506", "SA-04"),
        ("مطبخ أم فهد", "umfahad.kitchen", "أم فهد", "0591234507", "SA-01"),
        ("Dammam Handmade", "dammam.handmade", "سارة يونس", "0561234508", "SA-04"),
        ("دار الأناقة", "dar.alanaqa", "مريم جاسم", "0551234509", "SA-02"),
        ("Phone Case Hub", "phonecase.hub", "مصطفى راكان", "0551234510", "SA-01"),
        ("مكتبة الرياض", "riyadh.books", "عمر فاضل", "0551234511", "SA-01"),
        ("Khobar Fashion", "khobar.fashion", "رزان عمر", "0561234512", "SA-04"),
    ];

    public async Task SeedAsync(string country, CancellationToken ct)
    {
        if (await db.Merchants.AnyAsync(ct))
        {
            return;
        }

        var saudi = Countries.IsSaudi(country);
        var shops = saudi ? SaudiShops : IraqShops;
        var homeGov = saudi ? "SA-01" : "IQ-BG";
        var homeAddress = saudi ? "العليا، طريق الملك فهد" : "الكرادة، شارع 62";

        var now = clock.GetUtcNow().AddDays(-120);
        foreach (var (name, handle, contact, phone, gov) in shops)
        {
            now = now.AddHours(7);
            db.Merchants.Add(Merchant.Create(tenant.TenantId, new MerchantRequest
            {
                Name = name,
                InstagramHandle = handle,
                ContactName = contact,
                Phone = phone,
                Governorate = gov,
                Address = gov == homeGov ? homeAddress : null,
            }, now));
        }

        await db.SaveChangesAsync(ct);
    }
}
