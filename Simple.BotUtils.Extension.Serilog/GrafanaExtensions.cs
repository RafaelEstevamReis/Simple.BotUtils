namespace Simple.BotUtils.Extension.SerilogExtensions;

using Serilog;
using Serilog.Sinks.Grafana.Loki;
using System;
using System.Reflection;

public static class GrafanaExtensions
{
    public static LoggerConfiguration AddLoki(this LoggerConfiguration logger, Serilog.Events.LogEventLevel minimumLevel = Serilog.Events.LogEventLevel.Information)
    {
        var url = Environment.GetEnvironmentVariable("LOKI_ENDPOINT");
        if (string.IsNullOrEmpty(url))
        {
            return logger;
        }

        var loki = logger.WriteTo
            .GrafanaLoki(url,
                [
                    new LokiLabel("application", GetApplicationName()),
                    new LokiLabel("environment", GetEnviroment()),
                    new LokiLabel("host_name", GetHostName()),
                    new LokiLabel("os", Environment.GetEnvironmentVariable("OS") ?? Environment.OSVersion.ToString()),
                ],
                restrictedToMinimumLevel: minimumLevel)
            .Enrich.FromLogContext()
            ;

        return loki;
    }

    private static string GetHostName()
    {
        var host = Environment.GetEnvironmentVariable("HOSTNAME")
            ?? Environment.GetEnvironmentVariable("HOST_NAME")
            ?? Environment.GetEnvironmentVariable("USERDOMAIN")
            ;
        return host ?? Environment.MachineName;
    }

    private static string GetApplicationName()
    {
        var entry = Assembly.GetEntryAssembly();
        var asmName = entry?.FullName ?? "UNKOWN";
        return Environment.GetEnvironmentVariable("LOKI_APP_NAME")
            ?? Environment.GetEnvironmentVariable("APP_NAME")
            ?? Environment.GetEnvironmentVariable("APPNAME")
            ?? asmName.Split(',')[0];
    }

    private static string GetEnviroment()
    {
        var a = Environment.GetEnvironmentVariables();

        var vsVersion = Environment.GetEnvironmentVariable("ENVIRONMENT");
        var envName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
                 ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
                 ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                 ?? (vsVersion != null ? "DEV" : null);
     
        if (System.Diagnostics.Debugger.IsAttached)
        {
            envName = "DEBUG";
        }

        return envName ?? "PRODUCTION";
    }

}
