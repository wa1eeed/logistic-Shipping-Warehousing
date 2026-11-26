using Logistico.Modules.Shipments.Contracts;
using Logistico.Modules.Shipments.Contracts.Rules;
using Logistico.SharedKernel.Geo;

namespace Logistico.Modules.Shipments.Infrastructure;

/// <summary>
/// The country-specific data the demo seeder and journeys use: places, people, couriers, catalogue and currency.
/// Iraq keeps exactly what the demo has always had; Saudi Arabia mirrors it with its own regions, names and riyals.
/// Cosmetic shared details (bins, shelves, truck plates/drivers) stay in the seeder/journeys.
/// </summary>
internal sealed class DemoLocale
{
    public required string Country { get; init; }
    public required HubNetwork Network { get; init; }

    /// <summary>The busy primary hub and the area of its city that gets its own sort shelf and door-GPS proofs.</summary>
    public required string PrimaryHub { get; init; }
    public required string PrimaryPlace { get; init; }
    public required string PrimaryArea { get; init; }

    public required string CodCurrency { get; init; }

    /// <summary>Catalogue prices are kept in thousands for Iraq (IQD), in whole units for Saudi (SAR).</summary>
    public required decimal PriceScale { get; init; }
    public required decimal LocalFee { get; init; }
    public required decimal CrossFee { get; init; }

    public required string[] Customers { get; init; }
    public required Dictionary<string, string[]> Areas { get; init; }
    public required (string Code, int Weight)[] Demand { get; init; }
    public required Dictionary<string, (double Latitude, double Longitude)> AreaCentres { get; init; }
    public required Dictionary<string, (string Item, int MinK, int MaxK)[]> Catalogue { get; init; }
    public required string[] Landmarks { get; init; }
    public required string[] Items { get; init; }
    public required CourierRequest[] Couriers { get; init; }
    public required Dictionary<string, string[]> CouriersByHub { get; init; }
    public required Dictionary<string, string> Operators { get; init; }
    public required (string Place, string City, string Address) ChangeAddressSample { get; init; }
    public required PayoutMethod[] PayoutMethods { get; init; }
    public required Func<Random, string> ConsigneePhone { get; init; }
    public required Func<Random, string> AddressLine { get; init; }

    /// <summary>True for countries whose demo includes the international-freight and customs journeys (Iraq only for now).</summary>
    public bool SeedsInternational => Countries.IsIraq(Country);

    public static DemoLocale For(string country) => Countries.IsSaudi(country) ? Saudi : Iraq;

    public static readonly DemoLocale Iraq = new()
    {
        Country = Countries.Iraq,
        Network = IraqHubs.Default,
        PrimaryHub = IraqHubs.Baghdad,
        PrimaryPlace = "IQ-BG",
        PrimaryArea = "الكرادة",
        CodCurrency = "IQD",
        PriceScale = 1_000m,
        LocalFee = 5_000m,
        CrossFee = 8_000m,
        Customers = ["علي حسين", "زينب كاظم", "محمد جاسم", "نور الهدى", "حسن عبد الله", "مريم سعد", "Ahmed Saleh", "سجاد فلاح", "رقية عباس", "Karwan Ali", "فاطمة ناصر", "يوسف مهدي", "هبة طارق", "عمر خالد", "Dilan Omer", "سارة محمود"],
        Areas = new()
        {
            ["IQ-BG"] = ["الكرادة", "المنصور", "زيونة", "الأعظمية", "الكاظمية", "حي الجامعة", "الدورة", "البلديات", "اليرموك", "الشعب"],
            ["IQ-BA"] = ["العشار", "الجزائر", "البراضعية", "الزبير", "الطويسة"],
            ["IQ-AR"] = ["Ankawa", "Dream City", "100m Road", "Shaqlawa Rd"],
            ["IQ-NA"] = ["حي الأمير", "حي الحسين", "الكوفة"],
            ["IQ-KA"] = ["حي الحسين", "باب بغداد", "حي العامل"],
            ["IQ-NI"] = ["الزهور", "المجموعة الثقافية", "حي الشرطة"],
            ["IQ-SU"] = ["Salim St", "Bakhtiari", "Raparin"],
            ["IQ-BB"] = ["الحلة - حي بابل", "الحلة - الجمعية"],
            ["IQ-DQ"] = ["الناصرية - حي أور", "الشطرة"],
            ["IQ-KI"] = ["حي العروبة", "رحيم آوه"],
            ["IQ-DI"] = ["بعقوبة - حي المعلمين", "بعقوبة - التحرير"],
            ["IQ-AN"] = ["الرمادي - حي الأندلس", "الفلوجة - حي الجولان"],
            ["IQ-WA"] = ["الكوت - حي الحسين", "الكوت - الجعفرية"],
            ["IQ-QA"] = ["الديوانية - حي العسكري", "الديوانية - العروبة"],
            ["IQ-MA"] = ["العمارة - حي الحسين", "العمارة - المعلمين"],
            ["IQ-DA"] = ["Malta", "Shindokha"],
            ["IQ-SD"] = ["تكريت - حي القادسية", "سامراء - المعتصم"],
        },
        Demand =
        [
            ("IQ-BG", 40), ("IQ-BA", 9), ("IQ-AR", 7), ("IQ-NI", 7), ("IQ-SU", 5), ("IQ-NA", 5), ("IQ-KA", 4), ("IQ-BB", 4), ("IQ-DQ", 4),
            ("IQ-KI", 3), ("IQ-DI", 3), ("IQ-AN", 2), ("IQ-WA", 2), ("IQ-QA", 2), ("IQ-MA", 1), ("IQ-DA", 1), ("IQ-SD", 1),
        ],
        AreaCentres = new(StringComparer.Ordinal)
        {
            ["الكرادة"] = (33.304, 44.426),
            ["المنصور"] = (33.315, 44.345),
            ["زيونة"] = (33.329, 44.456),
            ["الأعظمية"] = (33.369, 44.379),
            ["الكاظمية"] = (33.380, 44.340),
            ["حي الجامعة"] = (33.312, 44.313),
            ["الدورة"] = (33.254, 44.395),
            ["البلديات"] = (33.345, 44.479),
            ["اليرموك"] = (33.301, 44.336),
            ["الشعب"] = (33.406, 44.427),
        },
        Catalogue = new(StringComparer.Ordinal)
        {
            ["بوتيك لمار"] = [("فستان سهرة", 45, 160), ("عباية خليجية", 35, 120), ("فستان صيفي", 25, 70), ("طقم بيجامة نسائي", 18, 45)],
            ["Baghdad Kicks"] = [("حذاء رياضي", 30, 110), ("سنيكرز Nike Air", 60, 180), ("حذاء جلد رجالي", 40, 120), ("نعال صيفي", 10, 25)],
            ["زهرة للتجميل"] = [("طقم مكياج", 25, 90), ("كريم ترطيب", 10, 35), ("سيروم فيتامين سي", 15, 45), ("أحمر شفاه ×2", 8, 25)],
            ["Erbil Gadgets"] = [("سماعات بلوتوث", 15, 75), ("شاحن سريع 20W", 10, 30), ("ساعة ذكية", 35, 160), ("باوربانك 20000", 20, 55), ("AirPods Pro", 120, 320)],
            ["عطور الفرات"] = [("عطر 100 مل", 25, 140), ("عطر عود", 60, 260), ("بخور ومبخرة", 15, 50), ("مجموعة عطور هدية", 45, 150)],
            ["Little Steps Kids"] = [("ملابس أطفال", 15, 55), ("ألعاب أطفال", 12, 60), ("حذاء أطفال", 15, 40), ("عربانة أطفال", 90, 240)],
            ["مطبخ ماما"] = [("كيك عيد ميلاد", 20, 60), ("حلويات منزلية", 12, 40), ("أواني مطبخ", 25, 110), ("قدر ضغط", 35, 90)],
            ["Mosul Handmade"] = [("سجادة يدوية", 60, 300), ("ديكور منزلي", 20, 80), ("مفرش طاولة مطرز", 25, 70), ("مسبحة يدوية", 15, 60)],
            ["دار الأناقة"] = [("حقيبة نسائية", 25, 120), ("ساعة يد نسائية", 30, 200), ("اكسسوارات ذهبية", 40, 350), ("نظارة شمسية", 15, 60)],
            ["Phone Case Hub"] = [("كفر موبايل ×3", 8, 25), ("كفر ايفون 15", 10, 30), ("حامي شاشة", 5, 15), ("كيبل شحن", 5, 15)],
            ["كتب ومكتبة الرصيف"] = [("كتب ×2", 10, 35), ("رواية", 8, 20), ("دفاتر وقرطاسية", 6, 25), ("مصحف", 15, 60)],
            ["Suli Fashion"] = [("قميص رجالي", 18, 55), ("بنطرون جينز", 22, 60), ("جاكيت شتوي", 40, 130), ("Hoodie", 25, 70)],
        },
        Landmarks = ["قرب جامع", "مقابل مدرسة", "خلف المول", "قرب سوق", "مقابل صيدلية", "قرب الملعب"],
        Items = ["فستان سهرة", "حذاء رياضي", "عطر 100 مل", "سماعات بلوتوث", "طقم مكياج", "كفر موبايل ×3", "ملابس أطفال", "كتب ×2", "ساعة يد", "حقيبة نسائية"],
        Couriers =
        [
            new() { Code = "Baghdad-01", Name = "مصطفى", Phone = "07701112233", Governorate = "IQ-BG", VehicleType = VehicleType.Motorcycle, VehiclePlate = "BG-10421", Notes = "الكرخ — المنصور، اليرموك، حي الجامعة" },
            new() { Code = "Baghdad-02", Name = "حيدر", Phone = "07802223344", Governorate = "IQ-BG", VehicleType = VehicleType.Van, VehiclePlate = "BG-44890", Notes = "الرصافة — الكرادة، زيونة، الأعظمية" },
            new() { Code = "Baghdad-03", Name = "علي", Phone = "07713334455", Governorate = "IQ-BG", VehicleType = VehicleType.Motorcycle, VehiclePlate = "BG-31902", Notes = "بغداد — الكاظمية، الشعب، البلديات" },
            new() { Code = "South-01", Name = "كرار", Phone = "07814445566", Governorate = "IQ-BA", VehicleType = VehicleType.LightTruck, VehiclePlate = "BA-88210", Notes = "البصرة والجنوب — العشار، الجزائر، الزبير" },
            new() { Code = "North-01", Name = "Aram", Phone = "07505556677", Governorate = "IQ-AR", VehicleType = VehicleType.Van, VehiclePlate = "EBL-77310", Notes = "Erbil & Sulaymaniyah — Ankawa, Dream City" },
            new() { Code = "Najaf-01", Name = "مرتضى", Phone = "07806667788", Governorate = "IQ-NA", VehicleType = VehicleType.Motorcycle, VehiclePlate = "NJ-20511", Notes = "النجف وكربلاء وبابل" },
            new() { Code = "Mosul-01", Name = "يونس", Phone = "07707778899", Governorate = "IQ-NI", VehicleType = VehicleType.Van, VehiclePlate = "NI-61207", Notes = "الموصل — الجانب الأيسر والأيمن" },
        ],
        CouriersByHub = new()
        {
            [IraqHubs.Baghdad] = ["مصطفى (Baghdad-01)", "حيدر (Baghdad-02)", "علي (Baghdad-03)"],
            [IraqHubs.Basra] = ["كرار (South-01)"],
            [IraqHubs.Erbil] = ["Aram (North-01)"],
            [IraqHubs.Najaf] = ["مرتضى (Najaf-01)"],
            [IraqHubs.Mosul] = ["يونس (Mosul-01)"],
        },
        Operators = new()
        {
            [IraqHubs.Baghdad] = "مصطفى — مخزن بغداد",
            [IraqHubs.Basra] = "كرار — مخزن البصرة",
            [IraqHubs.Erbil] = "Aram — Erbil hub",
            [IraqHubs.Najaf] = "حيدر — مخزن النجف",
            [IraqHubs.Mosul] = "يوسف — مخزن الموصل",
        },
        ChangeAddressSample = ("IQ-KA", "حي الحسين", "قرب مستشفى الحسين، شارع 60"),
        PayoutMethods = [PayoutMethod.ZainCash, PayoutMethod.QiCard, PayoutMethod.Cash, PayoutMethod.BankTransfer],
        ConsigneePhone = random => $"07{random.Next(5, 10)}{random.Next(10_000_000, 99_999_999)}",
        AddressLine = random => $"محلة {random.Next(100, 999)}، زقاق {random.Next(1, 60)}، دار {random.Next(1, 120)}",
    };

    public static readonly DemoLocale Saudi = new()
    {
        Country = Countries.Saudi,
        Network = SaudiHubs.Default,
        PrimaryHub = SaudiHubs.Riyadh,
        PrimaryPlace = "SA-01",
        PrimaryArea = "العليا",
        CodCurrency = "SAR",
        PriceScale = 1m,
        LocalFee = 15m,
        CrossFee = 25m,
        Customers = ["محمد العتيبي", "نورة القحطاني", "عبدالله الشمري", "سارة الدوسري", "فهد الغامدي", "ريم المطيري", "خالد الحربي", "لمياء السبيعي", "سلطان الزهراني", "هند العنزي", "ماجد الشهري", "أمل البقمي", "تركي الرشيد", "وجدان الخالدي", "ناصر العمري", "دانة الفيصل"],
        Areas = new()
        {
            ["SA-01"] = ["العليا", "النخيل", "الملقا", "الياسمين", "الروضة", "حي السفارات", "الربوة", "النسيم", "قرطبة", "حطين"],
            ["SA-02"] = ["حي الصفا", "حي الزهراء", "الحمراء", "أبحر الشمالية", "النعيم", "السلامة", "المرجان"],
            ["SA-04"] = ["الشاطئ", "الفيصلية", "النور", "الجلوية", "الدوحة", "العزيزية"],
            ["SA-03"] = ["قباء", "العوالي", "شظاة", "الدفاع"],
            ["SA-05"] = ["الصفراء", "الفايزية", "الإسكان"],
            ["SA-14"] = ["المنهل", "السد", "النميص"],
            ["SA-07"] = ["المروج", "السليمانية", "الورود"],
            ["SA-09"] = ["الروضة", "الصفا"],
            ["SA-06"] = ["البرزان", "صبحا"],
            ["SA-12"] = ["الفيصلية", "العزيزية"],
        },
        Demand =
        [
            ("SA-01", 36), ("SA-02", 24), ("SA-04", 16), ("SA-03", 7), ("SA-05", 4), ("SA-14", 4),
            ("SA-07", 3), ("SA-09", 2), ("SA-06", 2), ("SA-12", 2),
        ],
        AreaCentres = new(StringComparer.Ordinal)
        {
            ["العليا"] = (24.6906, 46.6848),
            ["النخيل"] = (24.7470, 46.6300),
            ["الملقا"] = (24.8060, 46.6300),
            ["الياسمين"] = (24.8330, 46.6480),
            ["الروضة"] = (24.7210, 46.7610),
            ["حي السفارات"] = (24.6870, 46.6210),
            ["الربوة"] = (24.7100, 46.7380),
            ["النسيم"] = (24.7530, 46.8300),
            ["قرطبة"] = (24.8100, 46.7700),
            ["حطين"] = (24.7830, 46.5880),
        },
        Catalogue = new(StringComparer.Ordinal)
        {
            ["بوتيك لمار"] = [("فستان سهرة", 200, 850), ("عباية مطرزة", 150, 600), ("فستان صيفي", 120, 350), ("طقم نسائي", 90, 280)],
            ["Riyadh Kicks"] = [("حذاء رياضي", 150, 600), ("سنيكرز Nike Air", 300, 900), ("حذاء جلد رجالي", 200, 700), ("شبشب طبي", 60, 150)],
            ["زهرة للتجميل"] = [("طقم مكياج", 120, 500), ("كريم ترطيب", 45, 180), ("سيروم فيتامين سي", 80, 250), ("أحمر شفاه ×2", 40, 130)],
            ["Jeddah Gadgets"] = [("سماعات بلوتوث", 80, 400), ("شاحن سريع 20W", 45, 150), ("ساعة ذكية", 180, 900), ("باوربانك 20000", 90, 280), ("AirPods Pro", 650, 1200)],
            ["عطور الريان"] = [("عطر 100 مل", 120, 700), ("دهن عود", 300, 1500), ("بخور ومبخرة", 80, 300), ("مجموعة عطور هدية", 250, 800)],
            ["Little Steps Kids"] = [("ملابس أطفال", 70, 280), ("ألعاب أطفال", 55, 300), ("حذاء أطفال", 70, 200), ("عربة أطفال", 450, 1300)],
            ["مطبخ أم فهد"] = [("كيك عيد ميلاد", 90, 300), ("حلويات منزلية", 55, 200), ("أواني مطبخ", 120, 550), ("قدر ضغط", 180, 450)],
            ["Dammam Handmade"] = [("سجادة يدوية", 300, 1500), ("ديكور منزلي", 90, 400), ("مفرش مطرز", 120, 350), ("مسبحة يدوية", 70, 300)],
            ["دار الأناقة"] = [("حقيبة نسائية", 150, 700), ("ساعة يد نسائية", 180, 1000), ("اكسسوارات", 90, 500), ("نظارة شمسية", 120, 450)],
            ["Phone Case Hub"] = [("كفر موبايل ×3", 40, 130), ("كفر ايفون 15", 55, 160), ("حامي شاشة", 30, 90), ("كيبل شحن", 30, 90)],
            ["مكتبة الرياض"] = [("كتب ×2", 55, 180), ("رواية", 40, 110), ("دفاتر وقرطاسية", 35, 140), ("مصحف فاخر", 90, 350)],
            ["Khobar Fashion"] = [("قميص رجالي", 90, 300), ("بنطلون جينز", 120, 350), ("جاكيت شتوي", 220, 700), ("Hoodie", 130, 400)],
        },
        Landmarks = ["قرب جامع", "مقابل مدرسة", "خلف المول", "قرب سوبرماركت", "مقابل صيدلية", "قرب محطة"],
        Items = ["فستان سهرة", "حذاء رياضي", "عطر 100 مل", "سماعات بلوتوث", "طقم مكياج", "كفر موبايل ×3", "ملابس أطفال", "كتب ×2", "ساعة يد", "حقيبة نسائية"],
        Couriers =
        [
            new() { Code = "Riyadh-01", Name = "محمد", Phone = "0551112233", Governorate = "SA-01", VehicleType = VehicleType.Motorcycle, VehiclePlate = "RUH 1042", Notes = "شمال الرياض — العليا، الملقا، الياسمين" },
            new() { Code = "Riyadh-02", Name = "عبدالعزيز", Phone = "0552223344", Governorate = "SA-01", VehicleType = VehicleType.Van, VehiclePlate = "RUH 4489", Notes = "وسط الرياض — الروضة، النسيم، الربوة" },
            new() { Code = "Riyadh-03", Name = "فيصل", Phone = "0553334455", Governorate = "SA-01", VehicleType = VehicleType.Motorcycle, VehiclePlate = "RUH 3190", Notes = "الرياض — السفارات، حطين، قرطبة" },
            new() { Code = "Jeddah-01", Name = "سعيد", Phone = "0564445566", Governorate = "SA-02", VehicleType = VehicleType.Van, VehiclePlate = "JED 8821", Notes = "جدة — الصفا، الزهراء، أبحر" },
            new() { Code = "Dammam-01", Name = "ناصر", Phone = "0505556677", Governorate = "SA-04", VehicleType = VehicleType.LightTruck, VehiclePlate = "DMM 7731", Notes = "الشرقية — الدمام والخبر والظهران" },
            new() { Code = "Abha-01", Name = "عسيري", Phone = "0556667788", Governorate = "SA-14", VehicleType = VehicleType.Motorcycle, VehiclePlate = "ABH 2051", Notes = "عسير — أبها وخميس مشيط" },
        ],
        CouriersByHub = new()
        {
            [SaudiHubs.Riyadh] = ["محمد (Riyadh-01)", "عبدالعزيز (Riyadh-02)", "فيصل (Riyadh-03)"],
            [SaudiHubs.Jeddah] = ["سعيد (Jeddah-01)"],
            [SaudiHubs.Dammam] = ["ناصر (Dammam-01)"],
            [SaudiHubs.Abha] = ["عسيري (Abha-01)"],
        },
        Operators = new()
        {
            [SaudiHubs.Riyadh] = "محمد — مركز الرياض",
            [SaudiHubs.Jeddah] = "سعيد — مركز جدة",
            [SaudiHubs.Dammam] = "ناصر — مركز الدمام",
            [SaudiHubs.Abha] = "عسيري — مركز أبها",
        },
        ChangeAddressSample = ("SA-02", "الحمراء", "طريق الملك، قرب مستشفى الملك فهد"),
        PayoutMethods = [PayoutMethod.BankTransfer, PayoutMethod.Cash],
        ConsigneePhone = random => $"+9665{random.Next(0, 10)}{random.Next(1_000_000, 9_999_999)}",
        AddressLine = random => $"طريق {random.Next(10, 90)}، مبنى {random.Next(1000, 9999)}، شقة {random.Next(1, 40)}",
    };
}
