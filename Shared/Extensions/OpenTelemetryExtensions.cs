using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
using Shared.Configuration;

namespace Shared.Extensions;

public static class OpenTelemetryExtensions
{
    public static IHostBuilder ConfigureOpenTelemetry(this IHostBuilder builder, OpenTelemetryConfiguration config)
    {
        return builder
            .ConfigureServices(services =>
            {
                services.AddOpenTelemetry()
                    .ConfigureResource(resourceBuilder =>
                    {
                        resourceBuilder
                            .AddService(serviceName: config.ServiceName, serviceVersion: config.ServiceVersion, serviceNamespace: config.ServiceNamespace);
                    })
                    .WithTracing(tracerProviderBuilder =>
                    {
                        tracerProviderBuilder
                            .AddSource(config.ServiceName);

                        if (config.EnableAspNetCoreInstrumentation)
                        {
                            tracerProviderBuilder.AddAspNetCoreInstrumentation();
                        }
                        
                        if (config.EnableHttpClientInstrumentation)
                        {
                            tracerProviderBuilder.AddHttpClientInstrumentation();
                        }
                        
                        if (config.EnableSqlClientInstrumentation)
                        {
                            tracerProviderBuilder.AddSqlClientInstrumentation();
                        }

                        if (config.EnableConsoleExporter)
                        {
                            tracerProviderBuilder.AddConsoleExporter();
                        }
                        
                        if (config.Jaeger.Enabled)
                        {
                            tracerProviderBuilder.AddJaegerExporter(jaegerOptions =>
                            {
                                jaegerOptions.AgentHost = config.Jaeger.AgentHost;
                                jaegerOptions.AgentPort = config.Jaeger.AgentPort;
                            });
                        }
                        
                        if (config.Zipkin.Enabled)
                        {
                            tracerProviderBuilder.AddZipkinExporter(zipkinOptions =>
                            {
                                zipkinOptions.Endpoint = new Uri(config.Zipkin.Endpoint);
                            });
                        }
                        
                        if (config.Otlp.Enabled)
                        {
                            tracerProviderBuilder.AddOtlpExporter(otlpOptions =>
                            {
                                otlpOptions.Endpoint = new Uri(config.Otlp.Endpoint);
                            });
                        }
                    })
                    .WithMetrics(metricsProviderBuilder =>
                    {
                        metricsProviderBuilder
                            .AddMeter(config.ServiceName);

                        if (config.EnableConsoleExporter)
                        {
                            metricsProviderBuilder.AddConsoleExporter();
                        }
                        
                        if (config.Otlp.Enabled)
                        {
                            metricsProviderBuilder.AddOtlpExporter(otlpOptions =>
                            {
                                otlpOptions.Endpoint = new Uri(config.Otlp.Endpoint);
                            });
                        }
                    });
            });
    }

    public static IHostBuilder ConfigureOpenTelemetry(this IHostBuilder builder, IConfiguration configuration)
    {
        var config = configuration.GetSection("OpenTelemetry").Get<OpenTelemetryConfiguration>() 
                    ?? new OpenTelemetryConfiguration();
        
        return builder.ConfigureOpenTelemetry(config);
    }
}
