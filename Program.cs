using Microsoft.EntityFrameworkCore;
using InventarioActivos.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
var builder = WebApplication.CreateBuilder(args);


builder.Services.AddRazorPages(options =>
{
    
    options.Conventions.AuthorizeFolder("/");
    options.Conventions.AllowAnonymousToPage("/Login");
    options.Conventions.AllowAnonymousToPage("/Error");
});
builder.Services.AddControllers();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });
builder.Configuration.AddUserSecrets<Program>(optional: true);
builder.Services.AddDbContext<InventarioContext>(options =>
options.UseNpgsql(builder.Configuration.GetConnectionString("InventariosActivos")));
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 50 * 1024 * 1024; // 50 MB
});
var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles(); 
app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.MapControllers();
app.Run();