using Microsoft.AspNetCore.Mvc;
using AccidentBook.API.Models;
using AccidentBook.API.Services;

namespace AccidentBook.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccidentsController : ControllerBase
{
    private readonly IAccidentService _accidentService;
    private readonly ILogger<AccidentsController> _logger;

    public AccidentsController(IAccidentService accidentService, ILogger<AccidentsController> logger)
    {
        _accidentService = accidentService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Accident>>> GetAccidents()
    {
        try
        {
            var accidents = await _accidentService.GetAllAccidentsAsync();
            return Ok(accidents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving accidents");
            return StatusCode(500, "An error occurred while retrieving accidents");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Accident>> GetAccident(int id)
    {
        try
        {
            var accident = await _accidentService.GetAccidentByIdAsync(id);
            if (accident == null)
                return NotFound();

            return Ok(accident);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving accident {Id}", id);
            return StatusCode(500, "An error occurred while retrieving the accident");
        }
    }

    [HttpPost]
    public async Task<ActionResult<Accident>> CreateAccident([FromBody] Accident accident)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdAccident = await _accidentService.CreateAccidentAsync(accident);
            return CreatedAtAction(nameof(GetAccident), new { id = createdAccident.Id }, createdAccident);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating accident");
            return StatusCode(500, "An error occurred while creating the accident");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Accident>> UpdateAccident(int id, [FromBody] Accident accident)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updatedAccident = await _accidentService.UpdateAccidentAsync(id, accident);
            if (updatedAccident == null)
                return NotFound();

            return Ok(updatedAccident);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating accident {Id}", id);
            return StatusCode(500, "An error occurred while updating the accident");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAccident(int id)
    {
        try
        {
            var deleted = await _accidentService.DeleteAccidentAsync(id);
            if (!deleted)
                return NotFound();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting accident {Id}", id);
            return StatusCode(500, "An error occurred while deleting the accident");
        }
    }
}

