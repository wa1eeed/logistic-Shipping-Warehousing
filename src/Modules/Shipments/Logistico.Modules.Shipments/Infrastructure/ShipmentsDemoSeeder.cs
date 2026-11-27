using Logistico.Modules.Merchants.Contracts;
using Logistico.Modules.Shipments.Application;
using Logistico.Modules.Shipments.Contracts;
using Logistico.Modules.Shipments.Contracts.Rules;
using Logistico.Modules.Shipments.Domain;
using Logistico.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Logistico.Modules.Shipments.Infrastructure;

/// <summary>Development-only demo data: international freight and Instagram-shop deliveries in every stage across Iraq's hubs.</summary>
internal sealed class ShipmentsDemoSeeder(
    ShipmentsDbContext db,
    NumberGenerator numbers,
    IMerchantDirectory merchants,
    AutomationEngine automation,
    HubDirectory hubs,
    ITenantContext tenant,
    IOperatingNetwork network,
    TimeProvider clock)
{
    private DemoLocale L = DemoLocale.Iraq;

    private static readonly string[] Origins = ["CNSHA", "CNNGB", "CNSZX", "CNCAN", "TRIST", "TRMER", "AEJEA", "AEDXB", "DEHAM", "INNSA"];
    private static readonly string[] Ports = ["IQUQR", "IQBGW", "IQBSR", "IQEBL", "IQISU", "IQNJF"];
    private static readonly string[] Shippers = ["Ningbo Bright Lighting Co.", "Istanbul Textile Export A.S.", "Jebel Ali Trading FZE", "Guangzhou Auto Parts Ltd.", "Hamburg Medical GmbH", "Shenzhen Mobile Accessories"];
    private static readonly string[] Importers = ["شركة النور للتجارة العامة", "Erbil Mart", "Basra Oil Services", "مؤسسة الرافدين", "Baghdad Pharma Distribution", "شركة دجلة للإلكترونيات"];
    // Orders per hour of the day (Baghdad time): quiet mornings, busy evenings after work.
    private static readonly int[] HourWeights = [3, 2, 1, 1, 1, 1, 1, 2, 3, 4, 5, 6, 6, 6, 6, 6, 7, 8, 9, 11, 13, 14, 12, 7];

    private static readonly string[] Couriers = ["مصطفى (Baghdad-01)", "حيدر (Baghdad-02)", "علي (Baghdad-03)", "كرار (South-01)", "Aram (North-01)"];
    private static readonly string[] Bins = ["A-01-1", "A-01-2", "A-02-1", "A-03-2", "B-01-1", "B-02-3", "C-01-1", "C-04-2", "R-01 (returns)"];

    private (string Name, string Phone, string Governorate, string Area, string Address)[]? customers;

    public async Task SeedAsync(int count, CancellationToken ct)
    {
        if (count <= 0)
        {
            return;
        }

        // Demo journeys follow the standard network, so the company starts with it.
        await hubs.EnsureStandardAsync(tenant.TenantId, ct);

        // The demo mirrors the company's operating country: Iraq or Saudi Arabia.
        L = DemoLocale.For(await network.CountryAsync(tenant.TenantId, ct));

        var random = new Random(2026);
        await SeedCouriersAsync(ct);
        await SeedSortPlansAsync(ct);

        if (L.SeedsInternational && !await db.Shipments.AnyAsync(s => s.ServiceType == ServiceType.International, ct))
        {
            for (var i = 0; i < count / 3; i++)
            {
                db.Shipments.Add(await InternationalAsync(random, ct));
            }

            await db.SaveChangesAsync(ct);
        }

        // The hub network demo (trucks, returns, requests) is seeded once.
        if (!await db.Transfers.AnyAsync(ct))
        {
            var shops = await merchants.ListActiveAsync(ct);
            if (shops.Count > 0)
            {
                var orders = new List<Shipment>(count);
                for (var i = 0; i < count; i++)
                {
                    orders.Add(await DomesticAsync(random, shops[random.Next(shops.Count)], ct));
                }

                var journeys = new DemoJourneys(random, clock.GetUtcNow(), tenant.TenantId, numbers, L);
                await journeys.RunAsync(orders, ct);
                db.Shipments.AddRange(orders);
                db.Transfers.AddRange(journeys.Transfers);
                db.Requests.AddRange(journeys.Requests);
                db.Notifications.AddRange(journeys.Notifications);
                db.Alerts.AddRange(journeys.Alerts);
                await db.SaveChangesAsync(ct);
            }
        }

        if (L.SeedsInternational)
        {
            await SeedCustomsDeclarationsAsync(random, ct);
        }

        await SeedCashHandoversAsync(ct);
        await SeedCodSettlementsAsync(ct);
        await SeedRatingsAsync(random, ct);
        await SeedDoorPositionsAsync(random, ct);
        await automation.SweepAsync(tenant.TenantId, ct);
    }

    /// <summary>
    /// Every hub starts with a sort plan on the demo shelves: parcels for its couriers, parcels waiting for a truck,
    /// returns, and a catch-all; Baghdad keeps Karrada (its busiest area) on a shelf of its own.
    /// </summary>
    private async Task SeedSortPlansAsync(CancellationToken ct)
    {
        if (await db.ShelfRules.AnyAsync(ct))
        {
            return;
        }

        var now = clock.GetUtcNow();
        foreach (var hub in (await hubs.NetworkAsync(tenant.TenantId, ct)).All)
        {
            ShelfRuleRequest[] plan =
            [
                new() { Purpose = ShelfPurpose.Deliver, Shelves = ["A-01-1", "A-01-2", "A-02-1", "A-03-2"], CapacityPerShelf = 40, Note = "للمندوبين" },
                new() { Purpose = ShelfPurpose.Forward, Shelves = ["B-01-1", "B-02-3"], CapacityPerShelf = 60, Note = "بانتظار الشاحنة" },
                new() { Purpose = ShelfPurpose.Return, Shelves = ["R-01"], Note = "مرتجعات البائعين" },
                new() { Shelves = ["C-01-1", "C-04-2"] },
            ];
            foreach (var rule in plan)
            {
                rule.HubCode = hub.Code;
                db.ShelfRules.Add(ShelfRule.Create(tenant.TenantId, rule, null, "seed", now));
            }

            if (hub.Code == L.PrimaryHub)
            {
                db.ShelfRules.Add(ShelfRule.Create(tenant.TenantId, new ShelfRuleRequest
                {
                    HubCode = hub.Code,
                    Purpose = ShelfPurpose.Deliver,
                    Governorate = L.PrimaryPlace,
                    Area = L.PrimaryArea,
                    Shelves = ["K-01", "K-02"],
                    CapacityPerShelf = 25,
                }, null, "seed", now));
            }
        }

        await db.SaveChangesAsync(ct);
    }

    /// <summary>Customers rate about half of last fortnight's deliveries from the tracking page: mostly happy, a few not.</summary>
    /// <summary>
    /// Delivered Baghdad parcels get the proof the courier app records at the door (GPS within a few hundred metres of
    /// their area's middle), so the demo's courier-run plans know where customers are, as a company using the app does.
    /// </summary>
    private async Task SeedDoorPositionsAsync(Random random, CancellationToken ct)
    {
        if (await db.DeliveryProofs.AnyAsync(p => p.Latitude != null, ct))
        {
            return;
        }

        var delivered = await db.Shipments.AsNoTracking()
            .Where(s => s.ServiceType == ServiceType.Domestic && s.Status == ShipmentStatus.Delivered && s.DestinationCode == L.PrimaryPlace && s.DeliveredAt != null)
            .OrderBy(s => s.DeliveredAt)
            .Select(s => new { s.Id, s.DestinationCity, s.CourierName, s.DeliveredAt })
            .ToListAsync(ct);
        foreach (var parcel in delivered)
        {
            if (parcel.DestinationCity is not { } area || !L.AreaCentres.TryGetValue(area, out var centre))
            {
                continue;
            }

            var door = new GeoPoint
            {
                Latitude = Math.Round(centre.Latitude + (random.NextDouble() - 0.5) * 0.012, 6),
                Longitude = Math.Round(centre.Longitude + (random.NextDouble() - 0.5) * 0.014, 6),
                AccuracyMeters = 8 + random.Next(25),
            };
            db.DeliveryProofs.Add(DeliveryProof.ForDelivery(tenant.TenantId, parcel.Id, parcel.CourierName ?? L.CouriersByHub[L.PrimaryHub][0],
                new CourierDeliveryRequest { ReceivedBy = DeliveryReceiver.Customer, Location = door }, parcel.DeliveredAt!.Value));
        }

        await db.SaveChangesAsync(ct);
    }

    private async Task SeedRatingsAsync(Random random, CancellationToken ct)
    {
        if (await db.Shipments.AnyAsync(s => s.Rating != null, ct))
        {
            return;
        }

        string?[] happy = [null, null, "وصل بسرعة، شكراً", "المندوب محترم جداً", "Fast delivery, thank you", "التغليف ممتاز"];
        string?[] unhappy = ["تأخر يومين عن الموعد", "المندوب لم يتصل قبل الوصول", "The box arrived damaged"];
        var now = clock.GetUtcNow();
        var recent = await db.Shipments
            .Where(s => s.ServiceType == ServiceType.Domestic && s.Status == ShipmentStatus.Delivered && s.DeliveredAt > now.AddDays(-13))
            .OrderBy(s => s.DeliveredAt)
            .ToListAsync(ct);
        foreach (var shipment in recent.Where(_ => random.Next(100) < 45))
        {
            var stars = random.Next(100) switch { < 62 => 5, < 85 => 4, < 93 => 3, < 97 => 2, _ => 1 };
            var comments = stars >= 4 ? happy : unhappy;
            var ratedAt = shipment.DeliveredAt!.Value.AddHours(random.Next(1, 30));
            shipment.Rate(stars, comments[random.Next(comments.Length)], ratedAt < now ? ratedAt : now);
        }

        await db.SaveChangesAsync(ct);
    }

    private async Task SeedCouriersAsync(CancellationToken ct)
    {
        var now = clock.GetUtcNow();
        var existing = await db.Couriers.Select(c => c.Code).ToListAsync(ct);
        foreach (var req in L.Couriers.Where(f => !existing.Contains(f.Code)))
        {
            db.Couriers.Add(Courier.Create(tenant.TenantId, req, now));
        }

        await db.SaveChangesAsync(ct);
    }

    private async Task SeedCustomsDeclarationsAsync(Random random, CancellationToken ct)
    {
        if (await db.CustomsDeclarations.AnyAsync(ct))
        {
            return;
        }

        var hsCodes = new[] { "8517.13.00", "6204.42.00", "8708.99.00", "9018.90.00", "9405.42.00", "3303.00.00" };
        var goods = new[] { "معدات اتصالات وإكسسوارات هواتف", "ألبسة ومنسوجات قطنية", "قطع غيار سيارات", "مستلزمات وأجهزة طبية", "وحدات إنارة LED صناعية", "عطور ومستحضرات تجميل" };
        var brokers = new[] { "شركة البوابة الجمركية — أم قصر", "مكتب الرافدين للتخليص الجمركي", "Al-Nahrain Customs Brokerage" };

        var shipments = await db.Shipments
            .Include(s => s.Events)
            .Where(s => s.ServiceType == ServiceType.International)
            .ToListAsync(ct);

        var country = await network.CountryAsync(tenant.TenantId, ct);
        var regime = CustomsDutyCalculator.RegimesFor(country)[0];
        var seq = 10400;
        foreach (var s in shipments)
        {
            if (s.Status is ShipmentStatus.Booked or ShipmentStatus.PickedUp or ShipmentStatus.ReceivedAtOrigin or ShipmentStatus.Consolidated)
            {
                continue;
            }

            seq++;
            var now = s.UpdatedAt;
            var decl = CustomsDeclaration.CreateFor(s, now, country);
            var isHold = s.Status == ShipmentStatus.CustomsHold;
            var isReleased = s.Status is ShipmentStatus.Released or ShipmentStatus.AtDestinationWarehouse or ShipmentStatus.ReadyForDispatch or ShipmentStatus.OutForDelivery or ShipmentStatus.Delivered;
            var isAwaiting = s.Status is ShipmentStatus.DepartedOrigin or ShipmentStatus.InTransit or ShipmentStatus.ArrivedPortOfEntry;
            var idx = random.Next(hsCodes.Length);
            var rate = (idx % 3 + 1) * 5m;

            var req = new UpsertCustomsDeclarationRequest
            {
                CustomsOfficeCode = CustomsDutyCalculator.OfficesFor(country).Any(o => o.Code == s.DestinationCode) ? s.DestinationCode : CustomsDutyCalculator.DefaultOfficeCode(country),
                Regime = regime,
                DeclarationNumber = isAwaiting ? null : $"2026-{s.DestinationCode}-{regime}-{seq}",
                AssessmentNumber = isAwaiting ? null : $"ASM-2026-{seq}",
                ReceiptNumber = isReleased ? $"RCP-2026-{seq}" : null,
                ReleaseOrderNumber = isReleased ? $"REL-2026-{seq}" : null,
                SelectivityLane = isHold ? CustomsLane.Red : isReleased ? CustomsLane.Green : isAwaiting ? CustomsLane.Unassigned : CustomsLane.Yellow,
                HsCode = hsCodes[idx],
                GoodsDescription = goods[idx],
                CountryOfOrigin = s.OriginCode[..2],
                CooNumber = $"COO-{s.OriginCode[..2]}-2026-{seq}",
                CooEVerified = !isHold && !isAwaiting,
                CommercialInvoiceNumber = $"INV-2026-{seq}",
                InvoiceEVerified = !isAwaiting,
                CocNumber = $"COC-{country}-{seq}",
                CustomsValueUsd = s.DeclaredValue ?? 12_500m,
                DutyRatePercent = rate,
                HoldReason = isHold ? "Certificate of Origin pending electronic verification in Iraqi single window" : null,
                BrokerName = brokers[seq % brokers.Length],
                Action = CustomsAction.Save,
            };

            decl.Apply(req, s, "seed", now, country);
            db.CustomsDeclarations.Add(decl);
        }

        await db.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Couriers handed in each earlier day's cash in the evening (one short by 5,000 IQD); today's cash is still with them.
    /// </summary>
    private async Task SeedCashHandoversAsync(CancellationToken ct)
    {
        if (await db.CashHandovers.AnyAsync(ct))
        {
            return;
        }

        var baghdad = clock.GetUtcNow().ToOffset(TimeSpan.FromHours(3));
        var today = new DateTimeOffset(baghdad.Date, baghdad.Offset).ToUniversalTime();
        var couriers = await db.Couriers.ToListAsync(ct);
        var collected = await db.Shipments
            .Where(s => s.CodCollectedAt != null && s.CodHandedOverAt == null && s.CourierName != null && s.DeliveredAt < today)
            .ToListAsync(ct);
        var first = true;
        foreach (var day in collected.GroupBy(s => new { s.CourierName, Day = s.DeliveredAt!.Value.ToOffset(TimeSpan.FromHours(3)).Date }).OrderBy(g => g.Key.Day))
        {
            if (couriers.FirstOrDefault(c => c.DisplayLabel == day.Key.CourierName) is not { } courier)
            {
                continue;
            }

            var parcels = day.ToList();
            var expected = parcels.Sum(p => p.CodAmount ?? 0m);
            var at = parcels.Max(p => p.DeliveredAt)!.Value.AddHours(2);
            var shortBy = first ? 5m * L.PriceScale : 0m;
            first = false;
            db.CashHandovers.Add(CashHandover.Create(tenant.TenantId, await numbers.CashHandoverNumberAsync(ct), courier, parcels, expected - shortBy,
                shortBy > 0 ? $"نقص {shortBy:N0} {L.CodCurrency} — يُخصم من المندوب" : null, "seed", at));
        }

        await db.SaveChangesAsync(ct);
    }

    private async Task SeedCodSettlementsAsync(CancellationToken ct)
    {
        if (await db.CodSettlements.AnyAsync(ct))
        {
            return;
        }

        var now = clock.GetUtcNow();
        var cutoff = now.AddDays(-7);
        var olderDelivered = await db.Shipments
            .Where(s => s.ServiceType == ServiceType.Domestic && s.Status == ShipmentStatus.Delivered && s.CodHandedOverAt != null && s.CodSettlementId == null && s.CodAmount > 0 && s.MerchantId != null && s.DeliveredAt < cutoff)
            .OrderBy(s => s.DeliveredAt)
            .ToListAsync(ct);

        var seq = 0;
        var methods = L.PayoutMethods;
        foreach (var group in olderDelivered.GroupBy(s => new { MerchantId = s.MerchantId!.Value, MerchantName = s.MerchantName ?? "—" }).Take(4))
        {
            var batch = group.ToList();
            if (batch.Count == 0)
            {
                continue;
            }

            seq++;
            var settledAt = batch.Max(s => s.DeliveredAt)!.Value.AddHours(6);
            var settlement = CodSettlement.Create(
                tenant.TenantId,
                await numbers.SettlementNumberAsync(ct),
                group.Key.MerchantId,
                group.Key.MerchantName,
                batch,
                [],
                methods[(seq - 1) % methods.Length],
                $"REF-{settledAt:MMdd}-{1000 + seq}",
                "تسوية أسبوعية لمبالغ الدفع عند الاستلام",
                "seed",
                settledAt);
            db.CodSettlements.Add(settlement);
        }

        await db.SaveChangesAsync(ct);
    }

    private async Task<Shipment> InternationalAsync(Random random, CancellationToken ct)
    {
        var now = clock.GetUtcNow();
        var mode = (TransportMode)random.Next(0, 5);
        var bookedAt = now.AddDays(-random.Next(1, 90)).AddMinutes(-random.Next(0, 1440));
        var request = new CreateShipmentRequest
        {
            ServiceType = ServiceType.International,
            Mode = mode,
            ShipperName = Shippers[random.Next(Shippers.Length)],
            ConsigneeName = Importers[random.Next(Importers.Length)],
            ConsigneePhone = $"+96477{random.Next(10_000_000, 99_999_999)}",
            OriginCode = Origins[random.Next(Origins.Length)],
            DestinationCode = Ports[random.Next(Ports.Length)],
            Incoterm = random.Next(3) switch { 0 => "FOB", 1 => "CIF", _ => "EXW" },
            Pieces = random.Next(1, 40),
            GrossWeightKg = Math.Round((decimal)(random.NextDouble() * (mode is TransportMode.SeaFcl ? 18000 : 900)) + 1m, 3),
            VolumeCbm = Math.Round((decimal)(random.NextDouble() * (mode is TransportMode.SeaFcl ? 60 : 6)), 4),
            DeclaredValue = Math.Round((decimal)(random.NextDouble() * 50_000) + 100m, 2),
            DeclaredCurrency = "USD",
            CodCurrency = null,
            PromisedAt = bookedAt.AddDays(mode switch { TransportMode.Air or TransportMode.Courier => 7, TransportMode.Road => 12, _ => 40 }),
        };

        var shipment = Shipment.Book(tenant.TenantId, await numbers.TrackingNumberAsync(ct), request, null, "seed", bookedAt, IraqHubs.Default);
        ShipmentStatus[] path =
        [
            ShipmentStatus.ReceivedAtOrigin, ShipmentStatus.Consolidated, ShipmentStatus.DepartedOrigin, ShipmentStatus.InTransit,
            ShipmentStatus.ArrivedPortOfEntry, ShipmentStatus.CustomsPending, ShipmentStatus.Released, ShipmentStatus.AtDestinationWarehouse,
            ShipmentStatus.OutForDelivery, ShipmentStatus.Delivered,
        ];
        // Old freight has mostly arrived; recent bookings are spread along the route.
        var steps = (now - bookedAt).TotalDays > 30 && random.Next(10) > 0 ? path.Length : random.Next(0, path.Length + 1);
        var at = bookedAt;
        foreach (var next in path.Take(steps))
        {
            at = at.AddHours(random.Next(6, 96));
            if (at > now)
            {
                break;
            }

            if (next == ShipmentStatus.Released && random.Next(5) == 0)
            {
                shipment.AdvanceTo(ShipmentStatus.CustomsHold, at, new MilestoneDetails("Umm Qasr customs", "Certificate of Origin not e-verified"), "customs", "seed", at);
                break;
            }

            var hub = IraqHubs.Default.FindOrDefault(shipment.DestinationHubCode);
            switch (next)
            {
                case ShipmentStatus.AtDestinationWarehouse:
                    shipment.ReceiveAtHub(hub, Bins[random.Next(Bins.Length - 1)], Actor.Scan("seed"), at);
                    break;
                case ShipmentStatus.OutForDelivery:
                    shipment.DispatchToCourier(hub, hub.Code == IraqHubs.Basra ? "كرار (South-01)" : hub.Code == IraqHubs.Erbil ? "Aram (North-01)" : Couriers[random.Next(3)], Actor.Scan("seed"), at);
                    break;
                default:
                    var details = next is ShipmentStatus.ArrivedPortOfEntry or ShipmentStatus.CustomsPending
                        ? new MilestoneDetails(shipment.DestinationCode == "IQBGW" ? "Baghdad International Airport" : "Umm Qasr North Port")
                        : new MilestoneDetails();
                    shipment.AdvanceTo(next, at, details, "system", "seed", at);
                    break;
            }
        }

        return shipment;
    }

    private async Task<Shipment> DomesticAsync(Random random, MerchantLookupItem shop, CancellationToken ct)
    {
        var now = clock.GetUtcNow();

        // A pool of customers, a few of whom order often: the demo then has repeat buyers at the same address.
        customers ??= Enumerable.Range(0, 900)
            .Select(_ =>
            {
                var governorate = L.Demand[Weighted(random, L.Demand.Select(d => d.Weight).ToArray())].Code;
                var areas = L.Areas[governorate];
                return (Name: L.Customers[random.Next(L.Customers.Length)], Phone: L.ConsigneePhone(random),
                    Governorate: governorate, Area: areas[random.Next(areas.Length)],
                    Address: L.AddressLine(random));
            })
            .ToArray();
        var customer = customers[(int)(customers.Length * Math.Pow(random.NextDouble(), 1.5))];

        // Most orders are recent; the rest reach back almost three months so periods can be compared.
        var daysAgo = random.Next(10) < 6 ? random.Next(0, 21) : random.Next(21, 80);
        var day = Endpoints.Api.StartOfBaghdadDay(now).AddDays(-daysAgo);
        var weekend = day.ToOffset(TimeSpan.FromHours(3)).DayOfWeek is DayOfWeek.Thursday or DayOfWeek.Friday;
        var hour = Weighted(random, HourWeights.Select((w, h) => weekend && h >= 17 ? w * 3 / 2 : w).ToArray());
        var bookedAt = day.AddHours(hour).AddMinutes(random.Next(60));
        if (bookedAt > now)
        {
            bookedAt = now.AddMinutes(-random.Next(5, 600));
        }

        var products = L.Catalogue.TryGetValue(shop.Name, out var sold) ? sold : L.Items.Select(i => (Item: i, MinK: 12, MaxK: 120)).ToArray();
        var product = products[random.Next(products.Length)];
        var request = new CreateShipmentRequest
        {
            ServiceType = ServiceType.Domestic,
            Mode = TransportMode.Road,
            MerchantId = shop.Id,
            ExternalReference = $"IG-{random.Next(1000, 9999)}",
            ConsigneeName = customer.Name,
            ConsigneePhone = customer.Phone,
            OriginCode = shop.Governorate,
            DestinationCode = customer.Governorate,
            DestinationCity = customer.Area,
            DeliveryAddress = customer.Address,
            Landmark = L.Landmarks[random.Next(L.Landmarks.Length)],
            ItemsDescription = product.Item,
            PickupRequired = random.Next(4) != 0,
            Pieces = random.Next(1, 4),
            GrossWeightKg = Math.Round((decimal)(random.NextDouble() * 4) + 0.2m, 3),
            CodAmount = random.Next(5) == 0 ? null : random.Next(product.MinK, product.MaxK + 1) * L.PriceScale,
            CodCurrency = L.CodCurrency,
            DeliveryFee = customer.Governorate == shop.Governorate ? L.LocalFee : L.CrossFee,
        };

        return Shipment.Book(tenant.TenantId, await numbers.TrackingNumberAsync(ct), request, new MerchantSnapshot(shop.Id, shop.Name, shop.Governorate), "seed", bookedAt, L.Network);
    }

    /// <summary>An index picked with probability proportional to its weight.</summary>
    private static int Weighted(Random random, IReadOnlyList<int> weights)
    {
        var pick = random.Next(weights.Sum());
        for (var i = 0; i < weights.Count; i++)
        {
            pick -= weights[i];
            if (pick < 0)
            {
                return i;
            }
        }

        return weights.Count - 1;
    }
}
