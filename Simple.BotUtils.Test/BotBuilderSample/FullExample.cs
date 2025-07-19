namespace BotBuilderSample;

using Simple.BotUtils;
using Simple.BotUtils.Controllers;
using Simple.BotUtils.Jobs;
using Simple.BotUtils.Logging;
using Simple.BotUtils.Test;
using Simple.BotUtils.Test.ControllerSample;
using Simple.BotUtils.Test.ScheduleSample;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class FullExample
{
    public static void ProgramMain(string[] args)
    {
        using var bot = new BotBuilder();

        bot.Setup1Config<MyConfig>("cfg.json", args)
           //.Setup2Logs(serilogBuilder)
           .Setup2Logs(SimpleLogBuilder)
           .Setup3DB(databaseBuilder)
           .Setup4Scheduler(schedulerBuilder)
           .Setup5Controllers(controllerBuilder)
           .Setup6Services(serviceBuilder)
           .SetupMisc(captureControlC)
           .SetupMiscAsync(runTask)
           .Run(restartOnError: false)
           ;
    }

    //private static Serilog.ILogger serilogBuilder(BotBuilder builder)
    //{
    //    ILogger log = new LoggerConfiguration()
    //        .MinimumLevel.Information()
    //        .WriteTo.Console()
    //        .WriteTo.File(((MyConfig)builder.Config).FilePath, rollingInterval: RollingInterval.Month)
    //        .CreateLogger();
    //
    //    builder.BotEngineLogErrorEvents += (sender, errorArgs) =>
    //    {
    //        log.Error(errorArgs.Exception, errorArgs.MessageTemplate, errorArgs.Data);
    //    };
    //    builder.BotStartupLogEvents += (sender, args) =>
    //    {
    //        log.Information(args.MessageTemplate, args.Data);
    //    };
    //    return log;
    //}
    private static ILogger SimpleLogBuilder(BotBuilder builder)
    {
        ILogger log = new LoggerBuilder()
            .SetMinimumLevel(LogEventLevel.Information)
            .LogToConsole()
            .LogToFile((builder.Config as MyConfig).LogPath)
            .CreateLogger();

        builder.BotEngineLogErrorEvents += (sender, errorArgs) =>
        {
            log.Error(errorArgs.Exception, errorArgs.MessageTemplate, errorArgs.Data);
        };
        builder.BotStartupLogEvents += (sender, args) =>
        {
            log.Information(args.MessageTemplate, args.Data);
        };
        return log;
    }
    private static IEnumerable<IDB> databaseBuilder(BotBuilder builder)
    {
        return [];
    }
    private static void schedulerBuilder(BotBuilder builder, Scheduler scheduler)
    {
        scheduler.Add<PingJob>();
    }
    private static void controllerBuilder(BotBuilder builder, ControllerManager manager)
    {
        manager.AcceptSlashInMethodName = true;
        manager.AddController<MyControllers>();
        // Or add entire assembly
        //manager.AddControllers(System.Reflection.Assembly.GetExecutingAssembly());

        manager.Filter += (s, f) =>
        {

        };
    }
    private static IEnumerable<IService> serviceBuilder(BotBuilder builder)
    {
        return [];
    }
    private static void captureControlC(BotBuilder builder)
    {
        Console.CancelKeyPress += (s, e) =>
        {
            e.Cancel = true; // Espera
            var logger = Simple.BotUtils.DI.Injector.Get<ILogger>();
            logger.Information("[CONSOLE] Break Key pressed");
            builder.Stop();
        };
    }
    private static async Task runTask(BotBuilder builder)
    {
        await Task.CompletedTask;
    }

}
