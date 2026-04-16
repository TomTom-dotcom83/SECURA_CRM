using Microsoft.EntityFrameworkCore;
using SECURA.Domain.Entities;
using SECURA.Domain.Enums;

namespace SECURA.Infrastructure.Persistence;

/// <summary>
/// Populates the database with realistic sample data in Development mode.
/// Runs at startup and is idempotent (checks for existing data before inserting).
/// </summary>
public static class DevDataSeeder
{
    public static async Task SeedAsync(SecuraDbContext db)
    {
        // Already seeded if producers exist (agencies may have been created manually)
        if (await db.Producers.AnyAsync()) return;

        const string seedUser = "dev-seed";

        // ── Agencies ─────────────────────────────────────────────────────────
        var existingAgencyNames = await db.Agencies.Select(a => a.Name).ToHashSetAsync();

        var agencies = new (string Name, AgencyTier Tier, string State, AgencyStatus Status,
                            string? Phone, string? Email, string? Website)[]
        {
            ("Apex Insurance Group",      AgencyTier.Preferred,  "TX", AgencyStatus.Active,
                "512-555-0101", "ops@apexinsurance.com",        "https://apexinsurance.com"),
            ("BlueStar Risk Partners",    AgencyTier.Preferred,  "NY", AgencyStatus.Active,
                "212-555-0202", "info@bluestarrisk.com",        "https://bluestarrisk.com"),
            ("Canyon West Brokers",       AgencyTier.Standard,   "AZ", AgencyStatus.Active,
                "602-555-0303", "contact@canyonwest.com",       null),
            ("Mesa Commercial Lines",     AgencyTier.Standard,   "CO", AgencyStatus.Validation,
                "303-555-0404", "admin@mesacommercial.com",     null),
            ("Summit Re Advisors",        AgencyTier.Preferred,  "MA", AgencyStatus.Active,
                "617-555-0505", "team@summitre.com",            "https://summitre.com"),
            ("Tri-State Insurance Co.",   AgencyTier.Standard,   "FL", AgencyStatus.Active,
                "305-555-0606", "service@tristateco.com",       null),
            ("Heartland Specialty Group", AgencyTier.Emerging,   "IL", AgencyStatus.Intake,
                "312-555-0707", null,                           null),
        };

        var agencyEntities = new List<Agency>();

        // Also collect pre-existing agencies so branches/producers can reference them
        agencyEntities.AddRange(await db.Agencies.ToListAsync());

        foreach (var (name, tier, state, status, phone, email, website) in agencies)
        {
            if (existingAgencyNames.Contains(name)) continue;  // skip duplicates
            var a = Agency.Create(name, tier, state);
            a.CreatedBy = seedUser;
            a.Update(name, tier, state, phone, email, website, null, seedUser);

            // Transition through intermediate statuses for non-Intake agencies
            if (status >= AgencyStatus.Validation)
                a.Transition(AgencyStatus.Validation, seedUser);
            if (status >= AgencyStatus.Contracting)
                a.Transition(AgencyStatus.Contracting, seedUser);
            if (status >= AgencyStatus.Appointment)
                a.Transition(AgencyStatus.Appointment, seedUser);
            if (status >= AgencyStatus.Training)
                a.Transition(AgencyStatus.Training, seedUser);
            if (status == AgencyStatus.Active)
                a.Transition(AgencyStatus.Active, seedUser);

            agencyEntities.Add(a);
            await db.Agencies.AddAsync(a);
        }

        await db.SaveChangesAsync();

        // ── Branches ─────────────────────────────────────────────────────────
        var branchData = new Dictionary<string, string[]>
        {
            ["Apex Insurance Group"]      = ["Austin HQ", "Dallas Branch"],
            ["BlueStar Risk Partners"]    = ["New York HQ", "Albany Office"],
            ["Canyon West Brokers"]       = ["Phoenix HQ"],
            ["Mesa Commercial Lines"]     = ["Denver HQ"],
            ["Summit Re Advisors"]        = ["Boston HQ", "Springfield Branch"],
            ["Tri-State Insurance Co."]   = ["Miami HQ", "Orlando Branch"],
            ["Heartland Specialty Group"] = ["Chicago HQ"],
        };

        var branches = new List<Branch>();
        foreach (var agency in agencyEntities)
        {
            if (!branchData.TryGetValue(agency.Name, out var branchNames)) continue;
            foreach (var bName in branchNames)
            {
                var branch = Branch.Create(agency.Id, bName);
                branch.CreatedBy = seedUser;
                await db.Branches.AddAsync(branch);
                branches.Add(branch);
            }
        }

        await db.SaveChangesAsync();

        // ── Producers ─────────────────────────────────────────────────────────
        var producerData = new (string AgencyName, string BranchName, string Npn,
                                 string First, string Last, string? Email,
                                 LicenseStatus LicStatus, bool Active)[]
        {
            ("Apex Insurance Group", "Austin HQ",       "1234567", "James",   "Thornton",  "jthornton@apex.com",    LicenseStatus.Active,    true),
            ("Apex Insurance Group", "Austin HQ",       "2345678", "Sarah",   "Chen",      "schen@apex.com",        LicenseStatus.Active,    true),
            ("Apex Insurance Group", "Dallas Branch",   "3456789", "Marcus",  "Williams",  "mwilliams@apex.com",    LicenseStatus.Pending,   true),
            ("BlueStar Risk Partners","New York HQ",    "4567890", "Emily",   "Rodriguez", "erodriguez@bluestar.com",LicenseStatus.Active,   true),
            ("BlueStar Risk Partners","New York HQ",    "5678901", "Daniel",  "Park",      "dpark@bluestar.com",    LicenseStatus.Active,    true),
            ("BlueStar Risk Partners","Albany Office",  "6789012", "Linda",   "Okafor",    "lokafor@bluestar.com",  LicenseStatus.Expired,   false),
            ("Canyon West Brokers",  "Phoenix HQ",      "7890123", "Carlos",  "Mendoza",   "cmendoza@canyonwest.com",LicenseStatus.Active,   true),
            ("Canyon West Brokers",  "Phoenix HQ",      "8901234", "Priya",   "Patel",     "ppatel@canyonwest.com", LicenseStatus.Active,    true),
            ("Mesa Commercial Lines","Denver HQ",       "9012345", "Kevin",   "Walsh",     "kwalsh@mesa.com",       LicenseStatus.Pending,   true),
            ("Summit Re Advisors",   "Boston HQ",       "0123456", "Rachel",  "Kim",       "rkim@summitre.com",     LicenseStatus.Active,    true),
            ("Summit Re Advisors",   "Boston HQ",       "1122334", "Thomas",  "Baker",     "tbaker@summitre.com",   LicenseStatus.Active,    true),
            ("Summit Re Advisors",   "Springfield Branch","2233445","Olivia", "Davis",     "odavis@summitre.com",   LicenseStatus.Suspended, false),
            ("Tri-State Insurance Co.","Miami HQ",      "3344556", "Michael", "Nguyen",    "mnguyen@tristate.com",  LicenseStatus.Active,    true),
            ("Tri-State Insurance Co.","Miami HQ",      "4455667", "Jessica", "Brown",     "jbrown@tristate.com",   LicenseStatus.Active,    true),
            ("Tri-State Insurance Co.","Orlando Branch","5566778", "Robert",  "Johnson",   "rjohnson@tristate.com", LicenseStatus.Active,    true),
        };

        var producers = new List<(Producer Entity, string AgencyName)>();
        foreach (var (agencyName, branchName, npn, first, last, email, licStatus, active) in producerData)
        {
            var branch = branches.FirstOrDefault(b =>
                b.Name == branchName &&
                agencyEntities.Any(a => a.Name == agencyName && a.Id == b.AgencyId));
            if (branch is null) continue;

            var p = Producer.Create(branch.Id, npn, first, last);
            p.CreatedBy = seedUser;
            if (email is not null)
                p.Update(first, last, email, null, seedUser);
            if (!active)
                p.Update(first, last, email, null, seedUser);

            await db.Producers.AddAsync(p);
            producers.Add((p, agencyName));
        }

        await db.SaveChangesAsync();

        // ── Licenses ──────────────────────────────────────────────────────────
        var rnd = new Random(42);
        var licenseData = new (string AgencyName, string ProducerNpn,
                                string State, LobType Lob, LicenseStatus Status,
                                int ExpiryOffsetDays)[]
        {
            // Apex – active producers
            ("Apex Insurance Group", "1234567", "TX", LobType.CommercialAuto, LicenseStatus.Active, 365),
            ("Apex Insurance Group", "1234567", "TX", LobType.GeneralLiability,             LicenseStatus.Active, 400),
            ("Apex Insurance Group", "2345678", "TX", LobType.CommercialProperty,       LicenseStatus.Active, 180),
            ("Apex Insurance Group", "2345678", "TX", LobType.WorkersCompensation,    LicenseStatus.Active, 200),
            ("Apex Insurance Group", "3456789", "TX", LobType.CommercialAuto, LicenseStatus.Active, 90),
            // BlueStar – mix of good/expiring/expired
            ("BlueStar Risk Partners","4567890","NY", LobType.GeneralLiability,             LicenseStatus.Active, 500),
            ("BlueStar Risk Partners","4567890","NY", LobType.CommercialProperty,       LicenseStatus.Active, 45),  // expiring soon
            ("BlueStar Risk Partners","5678901","NY", LobType.CommercialUmbrella,       LicenseStatus.Active, 300),
            ("BlueStar Risk Partners","6789012","NY", LobType.CommercialAuto, LicenseStatus.Expired, -10), // already expired
            // Canyon West
            ("Canyon West Brokers",  "7890123","AZ", LobType.CommercialProperty,       LicenseStatus.Active, 600),
            ("Canyon West Brokers",  "7890123","AZ", LobType.GeneralLiability,             LicenseStatus.Active, 550),
            ("Canyon West Brokers",  "8901234","AZ", LobType.CommercialAuto, LicenseStatus.Active, 730),
            // Summit Re
            ("Summit Re Advisors",   "0123456","MA", LobType.GeneralLiability,             LicenseStatus.Active, 400),
            ("Summit Re Advisors",   "0123456","MA", LobType.CommercialProperty,       LicenseStatus.Active, 25),  // expiring < 30d
            ("Summit Re Advisors",   "1122334","MA", LobType.WorkersCompensation,    LicenseStatus.Active, 300),
            ("Summit Re Advisors",   "2233445","MA", LobType.CommercialUmbrella,       LicenseStatus.Suspended, 100),
            // Tri-State
            ("Tri-State Insurance Co.","3344556","FL", LobType.CommercialAuto, LicenseStatus.Active, 450),
            ("Tri-State Insurance Co.","3344556","FL", LobType.GeneralLiability,             LicenseStatus.Active, 420),
            ("Tri-State Insurance Co.","4455667","FL", LobType.CommercialProperty,       LicenseStatus.Active, 380),
            ("Tri-State Insurance Co.","5566778","FL", LobType.WorkersCompensation,    LicenseStatus.Active, 300),
        };

        foreach (var (agencyName, npn, state, lob, status, expiryDays) in licenseData)
        {
            var producer = producers.FirstOrDefault(p =>
                p.AgencyName == agencyName &&
                p.Entity.Npn.Value == npn).Entity;
            if (producer is null) continue;

            var expiryDate = DateTime.UtcNow.AddDays(expiryDays);
            // For expired licenses, force past date
            if (status == LicenseStatus.Expired)
                expiryDate = DateTime.UtcNow.AddDays(-30);

            var licNum = $"LIC-{state}-{rnd.Next(100000, 999999)}";
            var lic = License.Create(producer.Id, state, lob, status, expiryDate, licNum);
            lic.CreatedBy = seedUser;
            producer.AddLicense(lic);
        }

        await db.SaveChangesAsync();

        // ── Submissions ───────────────────────────────────────────────────────
        var subData = new (string AgencyName, LobType Lob, string State,
                            string Insured, SubmissionStatus Status, int ReceivedDaysAgo)[]
        {
            ("Apex Insurance Group",      LobType.CommercialAuto, "TX", "Thornton Trucking LLC",      SubmissionStatus.Bound,    30),
            ("Apex Insurance Group",      LobType.GeneralLiability,             "TX", "Apex Retail Stores",          SubmissionStatus.Quoted,   15),
            ("Apex Insurance Group",      LobType.CommercialProperty,       "TX", "Dallas Warehouse Co.",        SubmissionStatus.InReview,  8),
            ("Apex Insurance Group",      LobType.WorkersCompensation,    "TX", "Texas Staffing Partners",     SubmissionStatus.New,       2),
            ("BlueStar Risk Partners",    LobType.GeneralLiability,             "NY", "Manhattan Bistro Group",      SubmissionStatus.Bound,    45),
            ("BlueStar Risk Partners",    LobType.CommercialUmbrella,       "NY", "BlueStar Hotel Holdings",     SubmissionStatus.Quoted,   20),
            ("BlueStar Risk Partners",    LobType.CommercialProperty,       "NY", "Albany Office Park",          SubmissionStatus.InReview, 10),
            ("BlueStar Risk Partners",    LobType.CommercialAuto, "NY", "NY Metro Logistics",          SubmissionStatus.Triaged,   5),
            ("BlueStar Risk Partners",    LobType.GeneralLiability,             "NJ", "Garden State Retailers",      SubmissionStatus.Declined, 60),
            ("Canyon West Brokers",       LobType.CommercialProperty,       "AZ", "Desert Solar Farms",          SubmissionStatus.InReview, 12),
            ("Canyon West Brokers",       LobType.CommercialAuto, "AZ", "Scottsdale Delivery Co.",     SubmissionStatus.Quoted,   18),
            ("Summit Re Advisors",        LobType.GeneralLiability,             "MA", "Summit Tech Campus",          SubmissionStatus.Bound,    50),
            ("Summit Re Advisors",        LobType.CommercialProperty,       "MA", "Boston Harbor Properties",    SubmissionStatus.Quoted,   22),
            ("Summit Re Advisors",        LobType.WorkersCompensation,    "MA", "Springfield Manufacturing",   SubmissionStatus.Referred, 14),
            ("Tri-State Insurance Co.",   LobType.CommercialAuto, "FL", "Miami Port Logistics",        SubmissionStatus.Bound,    40),
            ("Tri-State Insurance Co.",   LobType.GeneralLiability,             "FL", "Orlando Theme Services",      SubmissionStatus.Bound,    35),
            ("Tri-State Insurance Co.",   LobType.CommercialProperty,       "FL", "Coastal Hospitality Group",   SubmissionStatus.InReview,  9),
            ("Tri-State Insurance Co.",   LobType.WorkersCompensation,    "FL", "Tri-State Temp Agency",       SubmissionStatus.New,       1),
        };

        foreach (var (agencyName, lob, state, insured, targetStatus, daysAgo) in subData)
        {
            var agency = agencyEntities.FirstOrDefault(a => a.Name == agencyName);
            if (agency is null) continue;

            var received = DateTime.UtcNow.AddDays(-daysAgo);
            var sub = Submission.Create(agency.Id, lob, state, received, insured);
            sub.CreatedBy = seedUser;

            // Advance through statuses
            var transitions = new[] {
                SubmissionStatus.Triaged,
                SubmissionStatus.InReview,
                SubmissionStatus.Referred,
                SubmissionStatus.Quoted,
                SubmissionStatus.Bound,
                SubmissionStatus.Declined,
            };
            foreach (var ts in transitions)
            {
                if (!sub.CanTransitionTo(ts)) continue;
                if ((int)ts > (int)targetStatus) break;
                sub.Transition(ts, seedUser);
                if (sub.Status == targetStatus) break;
            }

            await db.Submissions.AddAsync(sub);
        }

        await db.SaveChangesAsync();
    }
}
