using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using pdf_page_splitter.Data;
using pdf_page_splitter.Extensions;
using pdf_page_splitter.Infrastructure;
using pdf_page_splitter.Logging;
using pdf_page_splitter.Services;
using pdf_page_splitter.Services.Repositories;
using pdf_page_splitter.Services.Services;
using Serilog;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace pdf_page_splitter
{
    internal static class Program
    {
        private static async Task Main(string[] args)
        {
            var builder = new ConfigurationBuilder();
            BuildConfig(builder);

            var configurationRoot = builder.Build();
            var appSettings = configurationRoot.Get<AppSettings>() ?? new AppSettings();

            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                Log.Fatal((Exception)e.ExceptionObject, "Host terminated unexpectedly");
                Log.CloseAndFlush();
            };

            Log.Logger = new LoggerConfiguration().GetSerilogLoggerConfiguration(configurationRoot).CreateBootstrapLogger();

            Log.Information($"Running on: {RuntimeInformation.OSDescription}");
            try
            {
                Log.Logger.Information("pdf-page-splitter starting...");

                var databaseFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database");
                if (!Directory.Exists(databaseFolderPath))
                    Directory.CreateDirectory(databaseFolderPath);

                var host = Host.CreateDefaultBuilder(args)
                    .ConfigureServices((hostContext, services) =>
                    {
                        services.AddDbContext<PdfPageSplitterObjectContext>(options => options.UseSqlite($"DataSource={Path.Combine(databaseFolderPath, "PdfPageSplitter.db")}"));
                        services.AddTransient<IPdfOperationsService, PdfOperationsService>();
                        services.AddScoped<ISplittedFileRepository, SplittedFileRepository>();
                        services.AddScoped<IUploadedFileRepository, UploadedFileRepository>();
                        services.AddScoped<IUnitOfWork, UnitOfWork>();
                    })
                    .UseSerilog()
                    .Build()
                    .MigrateDatabase();

                var service = ActivatorUtilities.CreateInstance<PdfOperationsService>(host.Services, configurationRoot);
                await service.Run();

            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "pdf-page-splitter terminated unexpectedly.");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static void BuildConfig(IConfigurationBuilder builder)
        {
            builder.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
                .AddJsonFile("PdfPageSplitter.Settings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();
        }

        private static LoggerConfiguration GetSerilogLoggerConfiguration(this LoggerConfiguration logger, IConfiguration configuration = default)
        {
            var applicationName = "pdf-page-splitter";
            logger.ReadFrom.Configuration(configuration);
            logger.MinimumLevel.Debug().Enrich.FromLogContext().WriteTo.Console();
            logger.Enrich.WithProperty("app", applicationName);

            var appSettings = configuration?.Get<AppSettings>() ?? new AppSettings();
            if (!string.IsNullOrEmpty(appSettings.LogDNA.ApiKey))
                logger.WriteTo.LogDNA(appSettings.LogDNA.ApiKey, applicationName);

            return logger;
        }
    }
}
