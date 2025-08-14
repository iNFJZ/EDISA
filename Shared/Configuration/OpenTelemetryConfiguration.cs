namespace Shared.Configuration;

public class OpenTelemetryConfiguration
{
    public string ServiceName { get; set; } = "EDISA";
    public string ServiceVersion { get; set; } = "1.0.0";
    public string ServiceNamespace { get; set; } = "EDISA";
    public string ServiceInstanceId { get; set; } = string.Empty;
    
    public JaegerConfig Jaeger { get; set; } = new();
    public ZipkinConfig Zipkin { get; set; } = new();
    public OtlpConfig Otlp { get; set; } = new();
    
    public bool EnableConsoleExporter { get; set; } = true;
    public bool EnableAspNetCoreInstrumentation { get; set; } = true;
    public bool EnableHttpClientInstrumentation { get; set; } = true;
    public bool EnableSqlClientInstrumentation { get; set; } = true;
}

public class JaegerConfig
{
    public string AgentHost { get; set; } = "localhost";
    public int AgentPort { get; set; } = 6831;
    public bool Enabled { get; set; } = false;
}

public class ZipkinConfig
{
    public string Endpoint { get; set; } = "http://localhost:9411/api/v2/spans";
    public bool Enabled { get; set; } = false;
}

public class OtlpConfig
{
    public string Endpoint { get; set; } = "http://localhost:4317";
    public bool Enabled { get; set; } = false;
}
