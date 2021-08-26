using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ServiceRegistration.Configurations
{
    public static class ConfigurationRootBuilder
    {
        public static IConfigurationRoot Build()
        {
            // get the configuration from the app settings
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
            return config;
        }
    }
}
