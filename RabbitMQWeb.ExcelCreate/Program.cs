using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQWeb.ExcelCreate.Models;
using RabbitMQWeb.ExcelCreate.Services;
using System.Security.Authentication;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

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

// --- RabbitMQ Settings ---
builder.Services.Configure<RabbitMQSettings>(builder.Configuration.GetSection("RabbitMq"));

// --- RabbitMQ ConnectionFactory & Services ---
builder.Services.AddSingleton(sp =>
{
    var options = sp.GetRequiredService<IOptions<RabbitMQSettings>>().Value;

    // Eðer amqps URI kullanmak istersen config'e "Url" ekleyebilirsin; burada ayrý alanlarý kullanýyoruz.
    var factory = new ConnectionFactory
    {
        HostName = options.HostName,
        Port = options.Port,
        UserName = options.UserName,
        Password = options.Password,
        VirtualHost = options.VirtualHost,
        DispatchConsumersAsync = true,
        AutomaticRecoveryEnabled = true
    };

    if (options.SslEnabled)
    {
        factory.Ssl = new SslOption
        {
            Enabled = true,
            ServerName = options.HostName, // sertifika CN/SAN ile eþleþmeli
            Version = SslProtocols.Tls12,
            // Geliþtirme sýrasýnda sertifika doðrulamasýný atlamak istersen config'ten kontrol et
            CertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) =>
            {
                if (options.AllowInvalidCertificates)
                    return true; // Sadece dev/test için
                // Normalde sadece SSL policy errors yoksa true dön
                return sslPolicyErrors == SslPolicyErrors.None;
            }
        };
    }

    return factory;
});

builder.Services.AddSingleton<RabbitMQClientService>();
builder.Services.AddSingleton<RabbitMQPublisher>();

var app = builder.Build();

// --- Middleware ---
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

// --- Seed Admin User ---
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
