using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using backend.Data;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<Product>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Product") ?? throw new InvalidOperationException("Connection string 'Product' not found.")));

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add CORS Po
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendCors", policy =>
    {
        policy
            .WithOrigins("http://localhost:3000", "http://localhost:5173", "http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("FrontendCors");

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
