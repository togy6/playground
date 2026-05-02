using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PlaygroundDashboard.Data;
using PlaygroundDashboard.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<IdentityUser, IdentityRole>(o =>
{
    o.Password.RequireDigit            = false;
    o.Password.RequireNonAlphanumeric  = false;
    o.Password.RequireUppercase        = false;
    o.Password.RequireLowercase        = false;
    o.Password.RequiredLength          = 6;
}).AddEntityFrameworkStores<AppDbContext>();

builder.Services.ConfigureApplicationCookie(o =>
{
    o.LoginPath = "/";
    o.Events.OnRedirectToLogin = ctx =>
    {
        if (ctx.Request.Path.StartsWithSegments("/api"))
            ctx.Response.StatusCode = 401;
        else
            ctx.Response.Redirect(ctx.RedirectUri);
        return Task.CompletedTask;
    };
});

builder.Services.AddControllers();
builder.Services.AddSignalR();

var app = builder.Build();

await SeedAsync(app);

app.UseStaticFiles();

app.Use(async (ctx, next) =>
{
    ctx.Response.Headers["X-Robots-Tag"] = "noindex, nofollow, noarchive, nosnippet";
    await next();
});

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<PlaygroundHub>("/hubs/playground");
app.MapFallbackToFile("index.html");

app.Run();

static async Task SeedAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var db  = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var um  = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    var cfg = scope.ServiceProvider.GetRequiredService<IConfiguration>();

    await db.Database.EnsureCreatedAsync();

    var email = cfg["Admin:Email"]    ?? "admin@oyunalani.local";
    var pass  = cfg["Admin:Password"] ?? "Admin123!";

    if (await um.FindByEmailAsync(email) is null)
    {
        var user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
        await um.CreateAsync(user, pass);
    }
}
