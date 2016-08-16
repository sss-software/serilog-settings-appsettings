﻿using System;
using System.Configuration;
using System.IO;
using Xunit;
using Serilog.Events;
using Serilog.Tests.Support;
using Serilog.Context;

namespace Serilog.Tests.AppSettings.Tests
{
    public class AppSettingsTests
    {
        static AppSettingsTests()
        {
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            var config = Path.GetFullPath(Path.Combine(basePath, "app.config"));
            if (!File.Exists(config))
                throw new InvalidOperationException($"Can't find app.config in {basePath}");

            AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", config);
        }

        [Fact]
        public void EnvironmentVariableExpansionIsApplied()
        {
            // Make sure we have the expected key in the App.config
            Assert.Equal("%PATH%", ConfigurationManager.AppSettings["serilog:enrich:with-property:Path"]);

            LogEvent evt = null;
            var log = new LoggerConfiguration()
                .ReadFrom.AppSettings() 
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            log.Information("Has a Path property with value expanded from the environment variable");

            Assert.NotNull(evt);
            Assert.NotEmpty((string)evt.Properties["Path"].LiteralValue());
            Assert.NotEqual("%PATH%", evt.Properties["Path"].LiteralValue());
        }

        [Fact]
        public void CanUseCustomPrefixToConfigureSettings()
        {
            const string prefix1 = "custom1";
            const string prefix2 = "custom2";

            // Make sure we have the expected keys in the App.config
            Assert.Equal("Warning", ConfigurationManager.AppSettings[prefix1 + ":serilog:minimum-level"]);
            Assert.Equal("Error", ConfigurationManager.AppSettings[prefix2 + ":serilog:minimum-level"]);

            var log1 = new LoggerConfiguration()
                .WriteTo.Observers(o => { })
                .ReadFrom.AppSettings(prefix1)
                .CreateLogger();

            var log2 = new LoggerConfiguration()
                .WriteTo.Observers(o => { })
                .ReadFrom.AppSettings(prefix2)
                .CreateLogger();

            Assert.False(log1.IsEnabled(LogEventLevel.Information));
            Assert.True(log1.IsEnabled(LogEventLevel.Warning));

            Assert.False(log2.IsEnabled(LogEventLevel.Warning));
            Assert.True(log2.IsEnabled(LogEventLevel.Error));
        }

        [Fact]
        public void CustomPrefixCannotContainColon()
        {
            Assert.Throws<ArgumentException>(() =>
                new LoggerConfiguration().ReadFrom.AppSettings("custom1:custom2"));
        }

        [Fact]
        public void CustomPrefixCannotBeSerilog()
        {
            Assert.Throws<ArgumentException>(() =>
                new LoggerConfiguration().ReadFrom.AppSettings("serilog"));
        }

        [Fact]
        public void ThreadIdEnricherIsApplied()
        {
            // Make sure we have the expected key in the App.config
            Assert.NotNull(ConfigurationManager.AppSettings["serilog:enrich:WithThreadId"]);

            LogEvent evt = null;
            var log = new LoggerConfiguration()
                .ReadFrom.AppSettings()
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            log.Information("Has a ThreadId property with value generated by ThreadIdEnricher");

            Assert.NotNull(evt);
            Assert.NotNull(evt.Properties["ThreadId"]);
            Assert.NotNull(evt.Properties["ThreadId"].LiteralValue() as int?);
        }

        [Fact]
        public void MachineNameEnricherIsApplied()
        {
            // Make sure we have the expected key in the App.config
            Assert.NotNull(ConfigurationManager.AppSettings["serilog:enrich:WithMachineName"]);

            LogEvent evt = null;
            var log = new LoggerConfiguration()
                .ReadFrom.AppSettings()
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            log.Information("Has a MachineName property with value generated by MachineNameEnricher");

            Assert.NotNull(evt);
            Assert.NotNull(evt.Properties["MachineName"]);
            Assert.NotEmpty((string)evt.Properties["MachineName"].LiteralValue());
        }

        [Fact]
        public void EnrivonmentUserNameEnricherIsApplied()
        {
            // Make sure we have the expected key in the App.config
            Assert.NotNull(ConfigurationManager.AppSettings["serilog:enrich:WithEnvironmentUserName"]);

            LogEvent evt = null;
            var log = new LoggerConfiguration()
                .ReadFrom.AppSettings()
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            log.Information("Has a EnrivonmentUserName property with value generated by EnrivonmentUserNameEnricher");

            Assert.NotNull(evt);
            Assert.NotNull(evt.Properties["EnvironmentUserName"]);
            Assert.NotEmpty((string)evt.Properties["EnvironmentUserName"].LiteralValue());
        }

        [Fact]
        public void ProcessIdEnricherIsApplied()
        {
            // Make sure we have the expected key in the App.config
            Assert.NotNull(ConfigurationManager.AppSettings["serilog:enrich:WithProcessId"]);

            LogEvent evt = null;
            var log = new LoggerConfiguration()
                .ReadFrom.AppSettings()
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            log.Information("Has a ProcessId property with value generated by ProcessIdEnricher");

            Assert.NotNull(evt);
            Assert.NotNull(evt.Properties["ProcessId"]);
            Assert.NotNull(evt.Properties["ProcessId"].LiteralValue() as int?);
        }

        [Fact]
        public void LogContextEnricherIsApplied()
        {
            // Make sure we have the expected key in the App.config
            Assert.NotNull(ConfigurationManager.AppSettings["serilog:enrich:FromLogContext"]);

            LogEvent evt = null;
            var log = new LoggerConfiguration()
                .ReadFrom.AppSettings()
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            using (LogContext.PushProperty("A", 1))
            {
                log.Information("Has a LogContext property with value generated by LogContextEnricher");
            }

            Assert.NotNull(evt);
            Assert.NotNull(evt.Properties["A"]);
            Assert.NotNull(evt.Properties["A"].LiteralValue() as int?);
            Assert.Equal(1, (int)evt.Properties["A"].LiteralValue());
        }
    }
}
