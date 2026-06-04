using Microsoft.AspNetCore.Mvc;
using SecondAssignment.Auth.Models;
using SecondAssignment.Auth.Services;

namespace SecondAssignment.Auth.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new AuthResponse
            {
                Success = false,
                Message = "Username and password are required"
            });
        }

        var result = await _authService.LoginAsync(request);
        
        if (!result.Success)
            return Unauthorized(result);

        return Ok(result);
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new AuthResponse
            {
                Success = false,
                Message = "Username and password are required"
            });
        }

        if (request.Password.Length < 6)
        {
            return BadRequest(new AuthResponse
            {
                Success = false,
                Message = "Password must be at least 6 characters"
            });
        }

        var result = await _authService.RegisterAsync(request);
        
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
