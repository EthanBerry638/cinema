using System.Security.Claims;
using cinema.DTOs;
using cinema.Services;
using cinema.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace cinema.Controllers;

[ApiController]
[Route("api/trips")]
[Authorize]
public class TripController (TripService tripService) : ControllerBase
{
    private readonly TripService _tripService = tripService;
    
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    [EnableRateLimiting(TripRateLimitSettings.PolicyName)]
    [HttpGet("/api/me/trips")]
    public async Task<ActionResult<List<TripSummaryResponse>>> MyTrips(CancellationToken ct = default)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return Challenge();

        var trips = await _tripService.MyTrips(userId, ct);
        
        return Ok(trips);
    }
}