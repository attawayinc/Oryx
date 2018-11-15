﻿// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.BuildScriptGeneratorCli.Logging;

namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    /// <summary>
    /// Configures the service provider, where all dependency injection is setup.
    /// </summary>
    internal class ServiceProviderBuilder
    {
        private IServiceCollection _serviceCollection;

        public ServiceProviderBuilder()
        {
            var configuration = GetConfiguration();

            _serviceCollection = new ServiceCollection();
            _serviceCollection
                .AddBuildScriptGeneratorServices()
                .AddLogging(loggingBuilder =>
                {
                    loggingBuilder
                    .SetMinimumLevel(LogLevel.Trace)
                    .AddFile();
                });
        }

        public ServiceProviderBuilder ConfigureServices(Action<IServiceCollection> configure)
        {
            configure(_serviceCollection);
            return this;
        }

        public ServiceProviderBuilder ConfigureScriptGenerationOptions(Action<BuildScriptGeneratorOptions> configure)
        {
            _serviceCollection.Configure<BuildScriptGeneratorOptions>(options =>
            {
                configure(options);
            });
            return this;
        }

        public IServiceProvider Build()
        {
            return _serviceCollection.BuildServiceProvider();
        }

        private static IConfiguration GetConfiguration()
        {
            var executingAssemblyFileInfo = new FileInfo(Assembly.GetExecutingAssembly().Location);
            var basePath = executingAssemblyFileInfo.Directory.FullName;

            // The order of 'Add' is important here.
            // The values provided at commandline override any values provided in environment variables
            // and values provided in environment variables override any values provided in appsettings.json.
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder
                .SetBasePath(basePath)
                .AddJsonFile(path: "appsettings.json", optional: true)
                .AddEnvironmentVariables();

            return configurationBuilder.Build();
        }
    }
}