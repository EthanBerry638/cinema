using System.Security.Claims;
using cinema.DTOs;
using cinema.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace cinema.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TripController (TripService tripService) : ControllerBase
{
    private readonly TripService _tripService = tripService;
    
    [EnableRateLimiting("trips")]
    [HttpGet]
    public async Task<ActionResult<List<TripSummaryResponse>>> MyTrips()
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return Unauthorized();

        var trips = await _tripService.MyTrips(userId);
        
        return Ok(trips);
    }
}