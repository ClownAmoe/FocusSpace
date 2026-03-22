using DomainTask = FocusSpace.Domain.Entities.Task;
using FocusSpace.Application.Interfaces;
using FocusSpace.Application.Services;
using FocusSpace.Domain.Entities;
using FocusSpace.Infrastructure.Data;
using FocusSpace.Infrastructure.Repositories;
using FocusSpace.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace FocusSpace.Api
{
    public class Program
    {
        public static async System.Threading.Tasks.Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateBootstrapLogger();

            try
            {
                Log.Information("Starting FocusSpace API...");

                var builder = WebApplication.CreateBuilder(args);

                // ── Serilog ───────────────────────────────────────────
                builder.Host.UseSerilog((context, services, configuration) =>
                    configuration
                        .ReadFrom.Configuration(context.Configuration)
                        .ReadFrom.Services(services)
                        .Enrich.FromLogContext());

                // ── Database ──────────────────────────────────────────
                builder.Services.AddDbContext<AppDbContext>(options =>
                    options.UseNpgsql(
                        builder.Configuration.GetConnectionString("DefaultConnection"),
                        npgsql => npgsql.EnableRetryOnFailure(3)));

                // ── ASP.NET Core Identity ─────────────────────────────
                builder.Services
                    .AddIdentity<User, ApplicationRole>(options =>
                    {
                        options.Password.RequireDigit = true;
                        options.Password.RequireLowercase = true;
                        options.Password.RequireUppercase = true;
                        options.Password.RequireNonAlphanumeric = false;
                        options.Password.RequiredLength = 8;

                        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                        options.Lockout.MaxFailedAccessAttempts = 5;
                        options.Lockout.AllowedForNewUsers = true;

                        options.User.RequireUniqueEmail = true;

                        options.SignIn.RequireConfirmedEmail = !builder.Environment.IsDevelopment();
                    })
                    .AddEntityFrameworkStores<AppDbContext>()
                    .AddDefaultTokenProviders();

                // ── Cookie Authentication ─────────────────────────────
                builder.Services.ConfigureApplicationCookie(options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.LogoutPath = "/Account/Logout";
                    options.AccessDeniedPath = "/Account/AccessDenied";
                    options.SlidingExpiration = true;
                    options.ExpireTimeSpan = TimeSpan.FromHours(8);
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
                    options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
                });

                // ── Authorization Policies ────────────────────────────
                builder.Services.AddAuthorization(options =>
                {
                    options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
                    options.AddPolicy("Authenticated", p => p.RequireRole("User", "Admin"));
                });

                // ── Repositories & Services ───────────────────────────
                builder.Services.AddScoped<ITaskRepository, TaskRepository>();
                builder.Services.AddScoped<ITaskService, TaskService>();
                builder.Services.AddScoped<IEmailService, EmailService>();

                // ── MVC + Swagger ─────────────────────────────────────
                builder.Services.AddControllersWithViews();
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen();

                var app = builder.Build();

                // ── Migrations & Seed ─────────────────────────────────
                using (var scope = app.Services.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

                    try
                    {
                        Log.Information("Applying database migrations...");
                        db.Database.Migrate();
                        Log.Information("Migrations applied successfully.");

                        await SeedAdminAsync(userManager, app.Configuration);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Failed during startup migration/seed.");
                        throw;
                    }
                }

                // ── Middleware ────────────────────────────────────────
                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }

                app.UseSerilogRequestLogging(options =>
                {
                    options.MessageTemplate =
                        "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
                });

                app.UseHttpsRedirection();
                app.UseStaticFiles();
                app.UseRouting();

                app.UseAuthentication();
                app.UseAuthorization();

                app.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "FocusSpace API terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static async System.Threading.Tasks.Task SeedAdminAsync(
            UserManager<User> userManager,
            IConfiguration config)
        {
            var section = config.GetSection("AdminSeed");
            var email = section["Email"];
            var password = section["Password"];
            var username = section["Username"] ?? "admin";

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                Log.Warning("AdminSeed configuration missing — skipping admin seed.");
                return;
            }

            if (await userManager.FindByEmailAsync(email) is not null)
                return;

            var admin = new User
            {
                UserName = username,
                Email = email,
                EmailConfirmed = true,
                IsApproved = true,
                Role = FocusSpace.Domain.Enums.UserRole.Admin,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(admin, password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Admin");
                Log.Information("Admin user seeded: {Email}", email);
            }
            else
            {
                Log.Error("Failed to seed admin user: {Errors}",
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }
}