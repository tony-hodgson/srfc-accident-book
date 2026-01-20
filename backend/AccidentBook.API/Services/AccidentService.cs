using Microsoft.EntityFrameworkCore;
using AccidentBook.API.Data;
using AccidentBook.API.Models;

namespace AccidentBook.API.Services;

public class AccidentService : IAccidentService
{
    private readonly AccidentDbContext _context;

    public AccidentService(AccidentDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Accident>> GetAllAccidentsAsync()
    {
        return await _context.Accidents
            .OrderByDescending(a => a.DateOfAccident)
            .ThenByDescending(a => a.TimeOfAccident)
            .ToListAsync();
    }

    public async Task<Accident?> GetAccidentByIdAsync(int id)
    {
        return await _context.Accidents.FindAsync(id);
    }

    public async Task<Accident> CreateAccidentAsync(Accident accident)
    {
        accident.CreatedAt = DateTime.UtcNow;
        _context.Accidents.Add(accident);
        await _context.SaveChangesAsync();
        return accident;
    }

    public async Task<Accident?> UpdateAccidentAsync(int id, Accident accident)
    {
        var existingAccident = await _context.Accidents.FindAsync(id);
        if (existingAccident == null)
            return null;

        existingAccident.DateOfAccident = accident.DateOfAccident;
        existingAccident.TimeOfAccident = accident.TimeOfAccident;
        existingAccident.Location = accident.Location;
        existingAccident.Opposition = accident.Opposition;
        existingAccident.PersonInvolved = accident.PersonInvolved;
        existingAccident.Age = accident.Age;
        existingAccident.PersonReporting = accident.PersonReporting;
        existingAccident.Description = accident.Description;
        existingAccident.NatureOfInjury = accident.NatureOfInjury;
        existingAccident.TreatmentGiven = accident.TreatmentGiven;
        existingAccident.ActionTaken = accident.ActionTaken;
        existingAccident.Witnesses = accident.Witnesses;
        existingAccident.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return existingAccident;
    }

    public async Task<bool> DeleteAccidentAsync(int id)
    {
        var accident = await _context.Accidents.FindAsync(id);
        if (accident == null)
            return false;

        _context.Accidents.Remove(accident);
        await _context.SaveChangesAsync();
        return true;
    }
}

