using Microsoft.EntityFrameworkCore;
using InventarioActivos.Data;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Configuration.AddUserSecrets<Program>(optional: true);
var app = builder.Build();
builder.Services.AddDbContext<InventarioContext>(options =>
options.UseNpgsql(builder.Configuration.GetConnectionString("InventariosActivos")));

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.MapControllers();
app.Run();