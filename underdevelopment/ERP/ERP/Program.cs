using ERP.Data;
using ERP.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

public class Program
{
    public static async Task Main(string[] args)
    {
        // 1. PostgreSQL Dátumkezelés javítása (Ez kell a Selejtezõ robothoz!)
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        var builder = WebApplication.CreateBuilder(args);

        // 2. ADATBÁZIS KONFIGURÁCIÓ (Összevont és javított logika)
        var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
        string finalConnectionString;

         if (!string.IsNullOrEmpty(databaseUrl))
        {
            // --- FELHÕS MÓD (Render / PostgreSQL) ---
            var databaseUri = new Uri(databaseUrl);
            var userInfo = databaseUri.UserInfo.Split(':');

            // PORT JAVÍTÁS: Ha a port -1, akkor az alapértelmezett 5432-t használjuk
            var port = databaseUri.Port == -1 ? 5432 : databaseUri.Port;

            finalConnectionString = $"Host={databaseUri.Host};Port={port};Database={databaseUri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(finalConnectionString));
        }
        else
        {
            // --- HELYI MÓD (Saját gép / SQL Server) ---
            finalConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(finalConnectionString));
        }

        // 3. JSON ÉS KONTROLLEREK
        builder.Services.AddControllersWithViews()
            .AddJsonOptions(options => {
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        // 4. IDENTITY (Csak egyszer regisztráljuk!)
        builder.Services.AddIdentity<User, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        // 5. SZERVIZEK REGISZTRÁCIÓJA
        builder.Services.AddScoped<IProductService, ProductService>();
        builder.Services.AddScoped<InvoiceService>();
        builder.Services.AddScoped<ReportService>();
        builder.Services.AddScoped<OrderService>();
        builder.Services.AddScoped<LogisticsService>();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<AuditService>();
        builder.Services.AddHostedService<ERP.Services.ExpiredStockCleanupService>();

        // 6. HITELÉSÍTÉS ÉS COOKIE-K
        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.Events.OnRedirectToLogin = context => {
                context.Response.StatusCode = 401;
                return Task.CompletedTask;
            };
            options.Events.OnRedirectToAccessDenied = context => {
                context.Response.StatusCode = 403;
                return Task.CompletedTask;
            };
        });

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
            };
        });

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("VercelPolicy", policy =>
            {
                policy.SetIsOriginAllowed(origin =>
                        // Ez minden Vercel-es címedet engedélyezni fogja automatikusan
                        new Uri(origin).Host.EndsWith("vercel.app") ||
                        origin.Contains("localhost")
                      )
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials();
            });
        });

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        // 7. ADATBÁZIS MIGRÁCIÓ ÉS SEED (Automatikus!)
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            try
            {
                var context = services.GetRequiredService<ApplicationDbContext>();
                // Automatikusan létrehozza a táblákat Renderen!
                await context.Database.MigrateAsync();

                await DbInitializer.SeedRolesAndAdminAsync(services);
                Console.WriteLine("--- Adatbázis inicializálva és seedelve ---");
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "Hiba történt az adatbázis indításakor.");
            }
        }

        app.UseRouting();
        app.UseCors("VercelPolicy");
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseSwagger();
        app.UseSwaggerUI();

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        await app.RunAsync();
    }
}