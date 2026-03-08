using Serilog;

namespace FocusSpace.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Bootstrap logger — спрацює до побудови DI-контейнера
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

                builder.Services.AddControllers();
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen();

                var app = builder.Build();

                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }

                // Логування кожного HTTP-запиту
                app.UseSerilogRequestLogging(options =>
                {
                    options.MessageTemplate =
                        "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
                });

                app.UseHttpsRedirection();
                app.UseAuthorization();
                app.MapControllers();

                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "FocusSpace API terminated unexpectedly");
            }
            finally
            {
                // Гарантуємо що всі логи відправляться до Seq перед виходом
                Log.CloseAndFlush();
            }
        }
    }
}