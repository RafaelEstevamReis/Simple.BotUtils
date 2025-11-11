[![.NET](https://github.com/RafaelEstevamReis/Simple.BotUtils/actions/workflows/dotnet.yml/badge.svg)](https://github.com/RafaelEstevamReis/Simple.BotUtils/actions/workflows/dotnet.yml)

[![NuGet](https://img.shields.io/nuget/v/Simple.BotUtils)](https://www.nuget.org/packages/Simple.BotUtils)

- [Simple.BotUtils](#simplebotutils)
  - [Compatibility List:](#compatibility-list)
  - [BotBuilder class](#botbuilder-class)
- [Main Features](#main-features)
  - [Dependency Injection](#dependency-injection)
  - [Endpoint-like controllers](#endpoint-like-controllers)
  - [Command-Line parser](#command-line-parser)
  - [Job/Task Scheduler](#jobtask-scheduler)
  - [Memory Caching](#memory-caching)
  - [Simplified logging engine](#simplified-logging-engine)
  - [Other Tools](#other-tools)
    - [Randomizer](#randomizer)
    - [NotificationDebouncer](#notificationdebouncer)
  - [Xml and Json Serialization](#xml-and-json-serialization)
    - [XmlSerializer](#xmlserializer)
    - [JsonSerializer](#jsonserializer)
  - [Sample project](#sample-project)

# Simple.BotUtils

A lightweight, user-friendly toolkit designed for building bots with essential features like Dependency Injection, Job Scheduler, Memory Cache, and Command-Line Parsing.Clear, highly compatible, and relies only on Newtonsoft.Json.Perfect for small-scale projects on any platform, from modern x64 systems to older hardware like Raspberry 3 (see compatibility list).

## Compatibility List:

Direct targets:
* Net Core 3.1, and 2.1
* Net Standard 1.0*, and 2.0
* Net 8.0, and 9.0
* Net Framework 4.6.1, 4.7.2, and 4.8
  
Indirect Support from Net Standard 2.0:
* Net Core 2.0+
* Net Framework 4.6.1+
* Mono 5.4+
* Xamarin.iOS 10.14+
* Xamarin.Android 8.0+
* UWP 10.0.16299+
* Unity 2018.1

## BotBuilder class

A beginner-friendly bot builder to help with boilerplate code integrating all tools and features

Instead of mannualy remembering what to add to a new project, the BotBuilder class helps you with the list of options and in the order:
~~~C#
public static void ProgramMain(string[] args)
{
    using var bot = new BotBuilder();

    bot.Setup1Config<MyConfig>("cfg.json", args) // Load my cofig first
       .Setup2Logs(SimpleLogBuilder) // Setup logs
       .Setup3DB(databaseBuilder)    // Setup dabases
       .SetupMisc(captureControlC)   // Add misc functions
       .Setup4Scheduler(schedulerBuilder)   // Add jobs
       .Setup5Controllers(controllerBuilder)// Add controllers
       .Setup6Services(serviceBuilder) // Add services
       .SetupMiscAsync(runTask)      // Add Async functions like APIs or Telegram/WhatsApp client
       .Run(restartOnError: false)   // Run the bot with optional restart-on-errors
       ;
}
~~~

# Main Features

## Dependency Injection

Simple Dependency Injection implementation supporting Singleton and Transient

**Singleton** are objects instantiated once and reused for the entire application life-time

Setup the object `tasker` as `Scheduler`
~~~ C#
  var scheduler = new Scheduler();
  Injector.AddSingleton<Scheduler>(scheduler);
~~~

Retrieving the scheduler object previous instantiated
~~~ C#
  var scheduler = Injector.Get<Scheduler>();
~~~

**Transient** are objects re-created for every use

Setup the class `Config` setting up a Func to create a new instance
~~~ C#
  Injector.AddTransient<Config>(() => Config.Load());
~~~

Retrieve a new instance for each use
~~~C#
  using(Config cfg = Injector.Get<Config>()){
    // Do stuff
  }
~~~

## Endpoint-like controllers

A simple mechanism for endpoint creation similar to ASP.net controllers with DI support

Create some endpoints
```C#
public class MyMethods : IController
{
  public void ShowInfo(string info) => Console.WriteLine(info);
  public void ShowNumber(int number) => Console.WriteLine(number);
  public void ShowDouble(double number) => Console.WriteLine(number);
}
```

and then easily call them
```C#
ctrl.Execute("ShowInfo", "Bla bla bla bla");
ctrl.Execute("ShowNumber", "42"); // string
ctrl.Execute("ShowNumber", 42); // Native
ctrl.Execute("ShowDouble", "42.42"); // string
ctrl.Execute("ShowDouble", 42.42); // Native
```

Each controller is an instance allowing multiple clusters of endpoints or what is sometimes called "namespace" (A kind of "route")

Is possible to parse the entire text from a Bot or external access command line

```C#
string message = "ShowCallerInfo \"Bla bla bla bla\"";
ctrl.ExecuteFromText(message);
```

In addition to DI support, is possible to pass `Context` values when calling `ExecuteFromText`

```C#
public void ShowCallerInfo(int contextParam, string textParams, [FromDI] MyConfig cfg)
          => Console.WriteLine($"ShowCallerInfo[{contextParam}] {textParams}");
```

Then called by
```C#
ctrl.ExecuteFromText(context: 42, text: message);
```

## Command-Line parser

A simple parser for program arguments

Allows access and selection with an Argument object, a dictionary or a NameValueCollection

Is also possible to map the arguments to an Object/Class

Read arguments as a Dictionary
~~~C#
  // argument access
  public static void Main(string[] args)
  {
    // app.exe -a AA --bb BBBB cccc
    var arguments = ArgumentParser.Parse(args);
    ...
    var a = arguments.Get("-a"); // "AA"
    var bb = arguments.Get("--bb"); // "BBBB"
    // Only one [Empty/""] is allowed
    var empty = arguments.Get(""); // "cccc"
  }
~~~

Create a class with the arguments
~~~C#
  // app.exe -n Name --number 12345
  public static void Main(string[] args)
  { 
    var data = Startup.ArgumentParser.ParseAs<MyData>(args);
    ...
  }
  class MyData{
    [Startup.ArgumentKey("-n", "--name")]
    public string MyName { get; set; }
    [Startup.ArgumentKey("-nb", "--number")]
    public int MyInt { get; set; }
  }
~~~

// Fill an existing class with arguments
~~~C#
  // app.exe -n Name --number 12345
  public static void Main(string[] args)
  { 
    // Load existing configuration
    var cfg = ConfigBase.Load<Config>("config.xml");
    // Update config with arguments, if any
    if (args.Length > 0)
    {
        ArgumentParser.ParseInto(args, cfg);
        // and save to next boot
        cfg.Save();
    }
    ...
  }
  class Config : ConfigBase{
    [Startup.ArgumentKey("-n", "--name")]
    public string MyName { get; set; }
    [Startup.ArgumentKey("-nb", "--number")]
    public int MyInt { get; set; }
  }
~~~

## Job/Task Scheduler

Execute timed jobs in pré-defined intervals

Create jobs
~~~C# 
  class PingJob : IJob
  {
    public bool CanBeInvoked { get; set; } = false;
    public bool CanBeScheduled { get; set; } = true;
    public bool RunOnStartUp { get; set; } = false;
    public TimeSpan StartEvery { get; set; } = TimeSpan.FromSeconds(30);

    public async Task ExecuteAsync(ExecutionTrigger trigger, object parameter)
    {
        var ping = new System.Net.NetworkInformation.Ping();
        await ping.SendPingAsync("localhost");
    }
  }
~~~

Setup the scheduler
~~~C#
  // create a scheduler
  var scheduler = new Scheduler();
  // All your tasks
  scheduler.Add<PingJob>(new PingJob());

  // This method will block execution
  scheduler.RunJobsSynchronously(cancellationToken);
~~~

## Memory Caching

A simple and versatile Memory Caching class, supports renew based on LastAccess and LastWrite

~~~C#
  var cache = new MemoryCache();
  // Setup a cache that expires only if not accessed for 1 hour
  cache.Add("cache-key", new CacheOptions()
  {
      ExpirationPolicy = ExpirationPolicy.LastAccess,
      ExpirationValue = TimeSpan.FromHours(1),
      // Setup a update method
      UpdateCallback = () => db.Query<Data>(),
  });
  ...
  // For use, simply calls Get<T>("key")
  var data = cache.Get<Data[]>("cache-key");
~~~

## Simplified logging engine

A simplified logging engine that behaves like Serilog for small applications and proof-of-concepts and is easy to switch to a production logging solution

~~~C#
ILogger log = new LoggerBuilder()
    .SetMinimumLevel(LogEventLevel.Information)
    .LogToConsole()
    .LogToFile(logPath)
    .CreateLogger();
...
try
{
    log.Information("Doing my stuff with {p}", p);
}
catch (Exception ex)
{
    log.Error(ex, "Stuff {s} went wrong", suff);
}
~~~

## Other Tools

### Randomizer

Offers a static Random instance and this set of tools:

* Choose One
* ChoseMany
* Shuffle

### NotificationDebouncer

A simple way to feed events and only notify when something changes

~~~C#
var notification = new NotificationDebouncer<bool>();
notification.NewValue += (sender, args) => Console.WriteLine($"Old: {args.OldValue} New: {args.NewValue}");
notification.SetValue(false); // Do not notify
notification.SetValue(false); // Do not notify
notification.SetValue(true); //  Do notify!
//Console: Old: false New: true
notification.SetValue(true); //  Do not notify
~~~

## Xml and Json Serialization

*Not available in Net Standard 1.0*

### XmlSerializer
A Static class wrapped around .Net native XmlSerializer to load and save objects in Xml Files

### JsonSerializer
A Static class wrapped around Newtonsoft Json to load and save objects in json Files


Saving file example:
~~~C#
var data = new Data();
...
XmlSerializer.ToFile("myData.xml", data);
JsonSerializer.ToFile("myData.json", data);
~~~

Loading from file or create a new instance
~~~C#
// load data or creates a new
var data = XmlSerializer.LoadOrCreate("myData.xml", new Data());
var data = JsonSerializer.LoadOrCreate("myData.json", new Data());
~~~

Test if a file exists and loads if it do exist
~~~C#
// load data or creates a new
if(XmlSerializer.TryLoadFile("myData.xml", out Data myData)){...}
if(JsonSerializer.TryLoadFile("myData.json", out Data myData)){...}
~~~

## Sample project

See a bot example with dependency injection, configuration file, JobScheduler and a Telegram-bot interface

> [BotEngine](https://github.com/RafaelEstevamReis/BotEngine-Demo)
