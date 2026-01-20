using AccidentBook.API.Models;

namespace AccidentBook.API.Services;

public interface IAccidentService
{
    Task<IEnumerable<Accident>> GetAllAccidentsAsync();
    Task<Accident?> GetAccidentByIdAsync(int id);
    Task<Accident> CreateAccidentAsync(Accident accident);
    Task<Accident?> UpdateAccidentAsync(int id, Accident accident);
    Task<bool> DeleteAccidentAsync(int id);
}

