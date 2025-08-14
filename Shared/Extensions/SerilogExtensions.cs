using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;
using Shared.Services;

namespace Shared.Extensions;

public static class SerilogExtensions
{
    public static IHostBuilder ConfigureSerilog(this IHostBuilder builder)
    {
        return builder.UseSerilog((context, services, configuration) =>
        {
            var elasticsearchUrl = context.Configuration.GetValue<string>("Elasticsearch:Url") ?? "http://localhost:9200";
            var elasticsearchIndexFormat = context.Configuration.GetValue<string>("Elasticsearch:IndexFormat") ?? "edisa-logs-{0:yyyy.MM}";
            var logLevel = context.Configuration.GetValue<string>("Logging:LogLevel:Default") ?? "Information";

            var loggerConfiguration = new LoggerConfiguration()
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.WithEnvironmentName()
                .Enrich.WithMachineName()
                .Enrich.WithProcessId()
                .Enrich.WithThreadId()
                .Enrich.WithProperty("Application", context.HostingEnvironment.ApplicationName)
                .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
                .Enrich.WithProperty("Version", context.Configuration.GetValue<string>("Application:Version") ?? "1.0.0");

            loggerConfiguration.WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}");

            loggerConfiguration.WriteTo.File(
                path: "logs/edisa-{Date}.log",
                rollingInterval: RollingInterval.Day,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}",
                retainedFileCountLimit: 30);

            loggerConfiguration.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(elasticsearchUrl))
            {
                IndexFormat = "logs-edisa-{0:yyyy.MM.dd}",
                AutoRegisterTemplate = true,
                AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
                NumberOfReplicas = 1,
                NumberOfShards = 2,
                BufferFileSizeLimitBytes = 5242880,
                BufferLogShippingInterval = TimeSpan.FromSeconds(5),
                Period = TimeSpan.FromSeconds(2),
                FailureCallback = e => Console.WriteLine("Unable to submit event " + e.MessageTemplate),
                EmitEventFailure = EmitEventFailureHandling.WriteToSelfLog |
                                   EmitEventFailureHandling.WriteToFailureSink |
                                   EmitEventFailureHandling.RaiseCallback
            });

            loggerConfiguration.MinimumLevel.Is(GetLogLevel(logLevel));

            loggerConfiguration.MinimumLevel.Override("Microsoft", LogEventLevel.Warning);
            loggerConfiguration.MinimumLevel.Override("System", LogEventLevel.Warning);
            loggerConfiguration.MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning);

            configuration.WriteTo.Logger(lc => lc
                .Filter.ByIncludingOnly(e => e.Level >= LogEventLevel.Information)
                .WriteTo.Console()
                .WriteTo.File("logs/edisa-info-.log", rollingInterval: RollingInterval.Day));

            configuration.WriteTo.Logger(lc => lc
                .Filter.ByIncludingOnly(e => e.Level >= LogEventLevel.Warning)
                .WriteTo.File("logs/edisa-warning-.log", rollingInterval: RollingInterval.Day));

            configuration.WriteTo.Logger(lc => lc
                .Filter.ByIncludingOnly(e => e.Level >= LogEventLevel.Error)
                .WriteTo.File("logs/edisa-error-.log", rollingInterval: RollingInterval.Day));
        });
    }

    public static IServiceCollection AddLoggingService(this IServiceCollection services)
    {
        services.AddSingleton<ILoggingService, LoggingService>();
        return services;
    }

    private static LogEventLevel GetLogLevel(string level)
    {
        return level.ToLower() switch
        {
            "verbose" => LogEventLevel.Verbose,
            "debug" => LogEventLevel.Debug,
            "information" => LogEventLevel.Information,
            "warning" => LogEventLevel.Warning,
            "error" => LogEventLevel.Error,
            "fatal" => LogEventLevel.Fatal,
            _ => LogEventLevel.Information
        };
    }
}
