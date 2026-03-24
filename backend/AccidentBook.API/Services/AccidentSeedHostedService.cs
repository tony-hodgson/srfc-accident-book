using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using AccidentBook.API.Data;
using AccidentBook.API.Models;
using AccidentBook.API.Options;

namespace AccidentBook.API.Services;

/// <summary>
/// Seeds 10 test accidents when the API starts (if enabled) and removes them on graceful shutdown.
/// </summary>
internal sealed class AccidentSeedHostedService : IHostedService
{
    private readonly IServiceProvider _services;
    private readonly IOptions<AppOptions> _appOptions;
    private readonly AccidentSeedState _seedState;
    private readonly ILogger<AccidentSeedHostedService> _logger;
    private bool _seeded;

    public AccidentSeedHostedService(
        IServiceProvider services,
        IOptions<AppOptions> appOptions,
        AccidentSeedState seedState,
        ILogger<AccidentSeedHostedService> logger)
    {
        _services = services;
        _appOptions = appOptions;
        _seedState = seedState;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_appOptions.Value.SeedTestAccidentsOnStartup)
            return;

        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AccidentDbContext>();

        var accidents = CreateTestAccidents();
        db.Accidents.AddRange(accidents);
        await db.SaveChangesAsync(cancellationToken);

        var ids = accidents.Select(a => a.Id).ToList();
        _seedState.SetIds(ids);
        _seeded = true;

        _logger.LogInformation(
            "Seeded {Count} test accident records (ids: {Ids}).",
            ids.Count,
            string.Join(", ", ids));
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (!_seeded)
            return;

        var ids = _seedState.GetIds();
        if (ids.Count == 0)
            return;

        try
        {
            using var scope = _services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AccidentDbContext>();

            var toRemove = await db.Accidents
                .Where(a => ids.Contains(a.Id))
                .ToListAsync(cancellationToken);

            if (toRemove.Count > 0)
            {
                db.Accidents.RemoveRange(toRemove);
                await db.SaveChangesAsync(cancellationToken);
            }

            _logger.LogInformation(
                "Removed {Count} seeded test accident records on shutdown.",
                toRemove.Count);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not remove seeded test accidents on shutdown.");
        }
    }

    private static List<Accident> CreateTestAccidents()
    {
        var utc = DateTime.UtcNow;
        var templates = new (string Location, string Opposition, string Person, int? Age, string Reporter, string Desc, string Injury, string Treatment, string Action, string Witnesses)[]
        {
            ("Main pitch", "Middlesbrough RFC", "J. Smith", 24, "Team physio", "Tackle during open play; player stayed down.", "Suspected ankle sprain", "RICE, assessed on pitch", "Substituted; GP referral", "Linesman, opposition coach"),
            ("Training ground", "Internal session", "A. Brown", 19, "Coach", "Stumbled in conditioning drill.", "Cut to knee", "Cleaned and bandaged", "Returned to light training", "Assistant coach"),
            ("Away — Durham", "Durham City RFC", "C. Wilson", 31, "Club medic", "Head contact in a maul.", "Minor headache", "HIA protocol completed — pass", "Played on after review", "Match referee"),
            ("Main pitch", "Newcastle Falcons Amateurs", "D. Taylor", 22, "Parent volunteer", "High ball contest — shoulder pain.", "Bruising to shoulder", "Ice pack", "Wing moved to bench", "Touch judge"),
            ("Indoor hall (youth)", "Bishop Auckland U16", "E. Jones", 16, "Youth lead", "Collision in touch rugby.", "Nosebleed", "Pinch + gauze", "Parent informed", "Both team coaches"),
            ("Main pitch", "Hartlepool Rovers", "F. Davies", 28, "First aider", "Scrum collapse — neck stiffness reported.", "Cervical strain (minor)", "C-spine precautions, assessed", "Subbed; A&E advised if symptoms worsen", "Opposition medic"),
            ("Training ground", "Internal session", "G. Evans", 26, "S&C coach", "Hamstring pull in sprint.", "Hamstring tightness", "Stretch, ice", "Modified training 7 days", "None"),
            ("Main pitch", "Alnwick RFC", "H. Hughes", 20, "Club captain", "Ruck — finger dislocation reduced on field.", "Finger dislocation (reduced)", "Buddy tape", "Off for remainder", "Referee + medic"),
            ("Away — Gateshead", "Gateshead RFC", "I. Thomas", 29, "Physio", "Knee twist in lineout lift.", "Medial knee pain", "Brace, ice", "MRI booked", "Lifting pod + lifter"),
            ("Main pitch", "Percy Park RFC", "K. Roberts", 23, "Match day medic", "Clash of heads in tackle.", "Small laceration eyebrow", "Steri-strips", "Blood bin; returned", "TMO review N/A")
        };

        var list = new List<Accident>(templates.Length);
        for (var i = 0; i < templates.Length; i++)
        {
            var t = templates[i];
            var day = utc.Date.AddDays(-i);
            var time = day.AddHours(14 + (i % 3)).AddMinutes(15 * (i % 4));

            list.Add(new Accident
            {
                DateOfAccident = day,
                TimeOfAccident = time,
                Location = t.Location,
                Opposition = t.Opposition,
                PersonInvolved = t.Person,
                Age = t.Age,
                PersonReporting = t.Reporter,
                Description = t.Desc,
                NatureOfInjury = t.Injury,
                TreatmentGiven = t.Treatment,
                ActionTaken = t.Action,
                Witnesses = t.Witnesses,
                CreatedAt = utc.AddMinutes(-i)
            });
        }

        return list;
    }
}
