using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQWeb.ExcelCreate.Models;
using RabbitMQWeb.ExcelCreate.Services;

var builder = WebApplication.CreateBuilder(args);

// --- SQL Server DbContext ---
var connectionString = builder.Configuration.GetConnectionString("SqlServer")
    ?? throw new InvalidOperationException("Connection string 'SqlServer' not found.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// --- Identity ---
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<AppDbContext>();

// --- MVC / Razor ---
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddDatabaseDeveloperPageExceptionFilter(); // Dev exception page for EF

// --- RabbitMQ ---
var rabbitMqSection = builder.Configuration.GetSection("RabbitMq");
builder.Services.AddSingleton(sp => new ConnectionFactory
{
    HostName = rabbitMqSection["HostName"] ?? "localhost",
    Port = int.Parse(rabbitMqSection["Port"] ?? "5672"),
    UserName = rabbitMqSection["UserName"] ?? "guest",
    Password = rabbitMqSection["Password"] ?? "guest",
    DispatchConsumersAsync = true
});
builder.Services.AddSingleton<RabbitMQClientService>();

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    var email = "admin@example.com";
    var password = "P@ssword123";

    if (await userManager.FindByEmailAsync(email) == null)
    {
        var user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
        var result = await userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            throw new Exception("Seed user creation failed: " + string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }
}

app.Run();
