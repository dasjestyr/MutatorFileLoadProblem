using System;
using System.IO;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;

namespace NsbInterfaceTest.Producer.DependencyInjection
{
    public static class ConfigurationInstaller
    {
        public static void LoadConfigurationValues(this IConfigurationBuilder builder, IHostingEnvironment environment)
        {
            builder.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($"azurekeyvault.{environment.EnvironmentName}.json", true, true)
                .AddEnvironmentVariables();

            var config = builder.Build();

            // if running local then do not add vault
            if (environment.EnvironmentName.Equals("Local", StringComparison.OrdinalIgnoreCase))
                return;

            var vaultUri = config["AzureKeyVault:VaultUri"];
            var clientId = config["AzureKeyVault:ClientId"];
            var clientSecret = config["AzureKeyVault:ClientSecret"];

            builder.AddAzureKeyVault(vaultUri, clientId, clientSecret, new KeyVaultSecretManager());
        }
    }
}
