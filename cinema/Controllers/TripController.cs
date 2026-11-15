using System.Security.Claims;
using cinema.Models;
using cinema.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace cinema.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TripController (TripService _tripService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<Trip>>> MyTrips()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        return Created();
    }
}