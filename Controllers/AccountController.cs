using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using PlaygroundDashboard.Services;
using System.Text;

namespace PlaygroundDashboard.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    readonly SignInManager<IdentityUser> _signIn;
    readonly UserManager<IdentityUser>  _userManager;
    readonly EmailService               _email;

    public AccountController(SignInManager<IdentityUser> signIn,
                              UserManager<IdentityUser>  userManager,
                              EmailService               email)
    {
        _signIn      = signIn;
        _userManager = userManager;
        _email       = email;
    }

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

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest("E-posta ve şifre zorunludur.");

        var user   = new IdentityUser { UserName = dto.Email, Email = dto.Email, EmailConfirmed = true };
        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return BadRequest(MapError(result.Errors));

        return NoContent();
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is not null)
        {
            var token        = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var resetLink    = $"{Request.Scheme}://{Request.Host}/?resetToken={encodedToken}&email={Uri.EscapeDataString(dto.Email)}";
            await _email.SendPasswordResetEmailAsync(dto.Email, resetLink);
        }
        // Her zaman 204 döndür — e-posta numaralandırmasını önler
        return NoContent();
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null)
            return BadRequest("Geçersiz istek.");

        byte[] tokenBytes;
        try   { tokenBytes = WebEncoders.Base64UrlDecode(dto.Token); }
        catch { return BadRequest("Geçersiz veya süresi dolmuş bağlantı."); }

        var result = await _userManager.ResetPasswordAsync(user, Encoding.UTF8.GetString(tokenBytes), dto.NewPassword);
        if (!result.Succeeded)
            return BadRequest(MapError(result.Errors));

        return NoContent();
    }

    static string MapError(IEnumerable<IdentityError> errors) =>
        errors.First().Code switch
        {
            "DuplicateEmail" or "DuplicateUserName" => "Bu e-posta adresi zaten kullanımda.",
            "InvalidEmail"                          => "Geçersiz e-posta adresi.",
            "PasswordTooShort"                      => "Şifre en az 6 karakter olmalıdır.",
            "InvalidToken"                          => "Geçersiz veya süresi dolmuş bağlantı.",
            _                                       => "İşlem gerçekleştirilemedi."
        };
}

public record LoginDto(string Email, string Password);
public record RegisterDto(string Email, string Password);
public record ForgotPasswordDto(string Email);
public record ResetPasswordDto(string Email, string Token, string NewPassword);
