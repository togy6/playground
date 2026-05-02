using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace PlaygroundDashboard.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    readonly SignInManager<IdentityUser> _signIn;

    public AccountController(SignInManager<IdentityUser> signIn) => _signIn = signIn;

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var result = await _signIn.PasswordSignInAsync(dto.Email, dto.Password, isPersistent: true, lockoutOnFailure: false);
        return result.Succeeded ? NoContent() : Unauthorized("Hatalı kullanıcı adı veya şifre.");
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _signIn.SignOutAsync();
        return NoContent();
    }

    [HttpGet("me")]
    public IActionResult Me() =>
        Ok(new { isAuthenticated = User.Identity?.IsAuthenticated ?? false });
}

public record LoginDto(string Email, string Password);
