#if !NETSTANDARD1_0
namespace Simple.BotUtils;

using Simple.BotUtils.Controllers;
using Simple.BotUtils.Data;
using Simple.BotUtils.Jobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

public class BotBuilder : IDisposable
{
    private readonly CancellationTokenSource cancelationSource;
    private readonly Scheduler tasker;
    private readonly ControllerManager ctrl;

    public event EventHandler<LoggerArguments> BotStartupLogEvents;
    public event EventHandler<LoggerErrorArguments> BotEngineLogErrorEvents;

    public CancellationToken CancelationToken { get; }
    public IConfigBase Config { get; private set; }
    public IDB[] DBs { get; private set; } = [];
    public IService[] Services { get; private set; } = [];
    public Scheduler Scheduler => tasker;
    public ControllerManager ControllerManager => ctrl;

    public BotBuilder()
    {
        cancelationSource = new CancellationTokenSource();
        CancelationToken = cancelationSource.Token;

        tasker = new Scheduler();
        ctrl = new ControllerManager();

        DI.Injector.AddSingleton(this);
    }

    public BotBuilder Setup1Config<T>(string configFile, string[] args)
        where T : IConfigBase, new()
    {
        var cfg = ConfigBase<T>.Load(configFile);

        if (args.Length > 0)
        {
            Startup.ArgumentParser.ParseInto(args, cfg);
            // and save to next boot
            cfg.Save();
        }

        DI.Injector.AddSingleton(cfg);
        Config = cfg;
        return this;
    }

    public BotBuilder Setup2Logs<T>(Func<BotBuilder, T> logBuilder)
    {
        var logObj = logBuilder(this);
        DI.Injector.AddSingleton(logObj);
        startupLog("[SETUP] Logs init");

        AppDomain.CurrentDomain.UnhandledException += (object sender, UnhandledExceptionEventArgs e) =>
        {
            errorLog(e.ExceptionObject as Exception, "[GLOBAL] Domain UnhandledException {@sender} {@e}", [sender, e]);
        };

        return this;
    }

    public BotBuilder Setup3DB(Func<BotBuilder, IEnumerable<IDB>> dbBuilders)
    {
        DBs = dbBuilders(this).ToArray();
        // Startup all in parallel
        var allTasks = DBs.Select(o => Task.Run(() =>
        {
            o.Startup();
            DI.Injector.AddSingleton(o.GetType(), o); // Add instance type, not interface
        })).ToArray();
        Task.WaitAll(allTasks);

        startupLog("[SETUP] Database init {dbCount} DBs", allTasks.Length);

        return this;
    }
    public BotBuilder Setup4Scheduler(Func<BotBuilder, Scheduler, IEnumerable<IJob>> jobsSource)
    {
        tasker.Error += (s, e) =>
        {
            errorLog(e.Exception, "[SCHEDULER] Error {@job}", [e.Info]);
        };
        foreach (var j in jobsSource(this, tasker)) tasker.Add(j);

        startupLog("[SETUP] Scheduler init {TaskCount} tasks", tasker.JobCount);
        DI.Injector.AddSingleton(tasker);

        return this;
    }
    public BotBuilder Setup5Controllers(Action<BotBuilder, ControllerManager> ctrlSource)
    {
        ctrlSource(this, ctrl);

        startupLog("[SETUP] Controllers init {count} methods", ctrl.GetMethodsName().Length);
        DI.Injector.AddSingleton(ctrl);

        return this;
    }
    public BotBuilder Setup6Services(Func<BotBuilder, IEnumerable<IService>> services, bool addServicesToDI = false)
    {
        Services = services(this).ToArray();
        foreach (var s in Services)
        {
            s.Startup();
            if (addServicesToDI) DI.Injector.AddSingleton(s.GetType(), s);
        }

        startupLog("[SETUP] Services init {count} services", ctrl.GetMethodsName().Length);

        return this;
    }
    public BotBuilder SetupMisc(Action<BotBuilder> action)
    {
        action(this);
        return this;
    }

    public void Run(bool restartOnError = false)
    {
        startupLog("[SETUP] INIT complete");

        while (!CancelationToken.IsCancellationRequested)
        {
            startupLog("[Scheduler] Startup");
            try
            {
                tasker.RunJobsSynchronously(CancelationToken);
            }
            catch (Exception ex)
            {
                errorLog(ex, "[Scheduler] Error", []);

                if (!restartOnError) break;
            }
        }
        startupLog("[BOT] Cancell");

        Dispose();
    }
    public void Stop()
    {
        startupLog("[BOT] Stop()");
        cancelationSource.Cancel();
    }

    public void Dispose()
    {
        startupLog("[BOT] Dispse()");
        if (!cancelationSource.IsCancellationRequested) cancelationSource.Cancel();

        foreach (var db in DBs) db.Dispose();
        foreach (var s in Services) s.Dispose();

        cancelationSource.Dispose();
    }

    private bool startupLog(string message, params object[] values)
    {
        if (BotStartupLogEvents == null) return false;
        BotStartupLogEvents(this, new() { MessageTemplate = message, Data = values });

        return true;
    }
    private bool errorLog(Exception exception, string message, params object[] values)
    {
        if (BotEngineLogErrorEvents == null) return false;
        BotEngineLogErrorEvents(this, new() { Exception = exception, MessageTemplate = message, Data = values });

        return true;
    }
}
public interface IDB : IDisposable
{
    void Startup();
}
public interface IService : IDisposable
{
    void Startup();
}
public class LoggerArguments : EventArgs
{
    public string MessageTemplate { get; set; }
    public object[] Data { get; set; }
}
public class LoggerErrorArguments : EventArgs
{
    public Exception Exception { get; set; }
    public string MessageTemplate { get; set; }
    public object[] Data { get; set; }
}

#endif