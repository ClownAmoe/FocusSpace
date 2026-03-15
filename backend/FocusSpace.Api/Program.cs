using FocusSpace.Application.Interfaces;
using FocusSpace.Application.Services;
using FocusSpace.Infrastructure.Data;
using FocusSpace.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace FocusSpace.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateBootstrapLogger();

            try
            {
                Log.Information("Starting FocusSpace API...");

                var builder = WebApplication.CreateBuilder(args);

                // Підключаємо Serilog з конфігурації appsettings.json
                builder.Host.UseSerilog((context, services, configuration) =>
                    configuration
                        .ReadFrom.Configuration(context.Configuration)
                        .ReadFrom.Services(services)
                        .Enrich.FromLogContext()
                );

                // ── Database ──────────────────────────────────────────
                builder.Services.AddDbContext<AppDbContext>(options =>
                    options.UseNpgsql(
                        builder.Configuration.GetConnectionString("DefaultConnection"),
                        npgsql => npgsql.EnableRetryOnFailure(3)
                    ));

                // ── Repositories & Services ───────────────────────────
                builder.Services.AddScoped<ITaskRepository, TaskRepository>();
                builder.Services.AddScoped<ITaskService, TaskService>();

                // ── MVC + Swagger ─────────────────────────────────────
                builder.Services.AddControllersWithViews();
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen();

                var app = builder.Build();

                // ── Auto-migrate on startup ───────────────────────────
                using (var scope = app.Services.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    try
                    {
                        Log.Information("Applying database migrations...");
                        db.Database.Migrate();
                        Log.Information("Migrations applied successfully.");
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Failed to apply migrations. Is PostgreSQL running?");
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
                app.UseAuthorization();

                app.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Tasks}/{action=Index}/{id?}");

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
    }
}