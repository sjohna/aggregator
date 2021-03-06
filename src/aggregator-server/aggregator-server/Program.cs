using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace aggregator_server
{
    public class Program
    {
        private static readonly ILog configLog = LogManager.GetLogger($"Config.{typeof(Program)}");

        private static readonly string LogDirectory = "Logs";

        private static RollingFileAppender CreateRollingFileAppender(string logFilePath, ILayout layout)
        {
            RollingFileAppender appender = new RollingFileAppender();
            appender.AppendToFile = true;
            appender.File = logFilePath;
            appender.Layout = layout;
            appender.MaxSizeRollBackups = 5;
            appender.MaximumFileSize = "10MB";
            appender.RollingStyle = RollingFileAppender.RollingMode.Size;
            appender.StaticLogFileName = true;
            appender.ActivateOptions();

            return appender;
        }

        public static void ConfigureLogging()
        {
            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();

            PatternLayout patternLayout = new PatternLayout();
            patternLayout.ConversionPattern = "%date [%thread] %-5level %logger - %message%newline";
            patternLayout.ActivateOptions();

            RollingFileAppender startupAppender = CreateRollingFileAppender(Path.Combine(LogDirectory, "All-StartupAndConfigure.log"), patternLayout);

            var startupLogger = LogManager.GetLogger("Config").Logger as Logger;
            startupLogger.AddAppender(startupAppender);

            RollingFileAppender anomalousLog = CreateRollingFileAppender(Path.Combine(LogDirectory, "All-Anomalous.log"), patternLayout);

            ForwardingAppender allToAnomalousforwarder = new ForwardingAppender();
            allToAnomalousforwarder.AddAppender(anomalousLog);
            allToAnomalousforwarder.AddFilter(new log4net.Filter.LevelRangeFilter() { AcceptOnMatch = true, LevelMin=Level.Warn, LevelMax=Level.Fatal });
            hierarchy.Root.AddAppender(allToAnomalousforwarder);

            RollingFileAppender infoLog = CreateRollingFileAppender(Path.Combine(LogDirectory, "All-Info.log"), patternLayout);

            ForwardingAppender allToInfoForwarder = new ForwardingAppender();
            allToInfoForwarder.AddAppender(infoLog);
            allToInfoForwarder.AddFilter(new log4net.Filter.LevelRangeFilter() { AcceptOnMatch = true, LevelMin = Level.Info, LevelMax = Level.Fatal });
            hierarchy.Root.AddAppender(allToInfoForwarder);

            RollingFileAppender debugLog = CreateRollingFileAppender(Path.Combine(LogDirectory, "All-Debug.log"), patternLayout);

            ForwardingAppender allToDebugForwarder = new ForwardingAppender();
            allToDebugForwarder.AddAppender(debugLog);
            allToDebugForwarder.AddFilter(new log4net.Filter.LevelRangeFilter() { AcceptOnMatch = true, LevelMin = Level.Debug, LevelMax = Level.Fatal });
            hierarchy.Root.AddAppender(allToDebugForwarder);

            hierarchy.Root.Level = Level.Debug;
            hierarchy.Configured = true;
        }

        public static void Main(string[] args)
        {
            ConfigureLogging();

            configLog.Info("***** Application Started *****");

            var host = CreateHostBuilder(args).Build();
            host.Run();

            configLog.Info("Application exiting.");
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
