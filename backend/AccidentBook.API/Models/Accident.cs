namespace AccidentBook.API.Models;

public class Accident
{
    public int Id { get; set; }
    public DateTime DateOfAccident { get; set; }
    public DateTime TimeOfAccident { get; set; }
    public string Location { get; set; } = string.Empty;
    public string Opposition { get; set; } = string.Empty;
    public string PersonInvolved { get; set; } = string.Empty;
    public int? Age { get; set; }
    public string PersonReporting { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string NatureOfInjury { get; set; } = string.Empty;
    public string TreatmentGiven { get; set; } = string.Empty;
    public string ActionTaken { get; set; } = string.Empty;
    public string Witnesses { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

