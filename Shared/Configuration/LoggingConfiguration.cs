namespace Shared.Configuration
{
    public class LoggingConfiguration
    {
        public string LogLevel { get; set; } = "Information";
        public string LogFilePath { get; set; } = "logs";
        public string LogFileName { get; set; } = "edisa-{Date}.log";
        public int RetainedFileCountLimit { get; set; } = 30;
        public string ElasticsearchUrl { get; set; } = "http://localhost:9200";
        public string ElasticsearchIndexFormat { get; set; } = "logs-edisa-{0:yyyy.MM.dd}";
        public bool EnableConsoleLogging { get; set; } = true;
        public bool EnableFileLogging { get; set; } = true;
        public bool EnableElasticsearchLogging { get; set; } = true;
        public bool EnableStructuredLogging { get; set; } = true;
        public bool EnableRequestLogging { get; set; } = true;
        public bool EnablePerformanceLogging { get; set; } = true;
        public bool EnableSecurityLogging { get; set; } = true;
        public bool EnableBusinessLogging { get; set; } = true;
        public bool EnableSystemLogging { get; set; } = true;
        public int LogBufferSize { get; set; } = 1000;
        public int LogFlushIntervalSeconds { get; set; } = 5;
        public bool IncludeScopes { get; set; } = true;
        public bool IncludeUserContext { get; set; } = true;
        public bool IncludeRequestContext { get; set; } = true;
        public bool IncludeMachineContext { get; set; } = true;
        public string[] ExcludedPaths { get; set; } = { "/health", "/metrics", "/favicon.ico" };
        public string[] SensitiveFields { get; set; } = { "password", "token", "secret", "key" };
    }
}
