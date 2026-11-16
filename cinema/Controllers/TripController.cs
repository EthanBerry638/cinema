using System.Security.Claims;
using cinema.DTOs;
using cinema.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace cinema.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TripController (TripService tripService) : ControllerBase
{
    private readonly TripService _tripService = tripService;
    
    [HttpGet]
    public async Task<ActionResult<List<TripSummaryResponse>>> MyTrips()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var trips = await _tripService.MyTrips(userId);

        if (trips.Count == 0) return NotFound();
        
        return Ok(trips);
    }
}