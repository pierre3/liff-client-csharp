using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

[assembly: FunctionsStartup(typeof(MyTaskBot.Server.Startup))]
namespace MyTaskBot.Server
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton<IDocumentClient>(provider => 
            {
                var configuration = provider.GetRequiredService<IConfiguration>();

                var accountEndpoint = new Uri(configuration.GetValue<string>("Cosmos:AccountEndpoint"));
                var accountKey = configuration.GetValue<string>("Cosmos:AccountKey");
                
                return new DocumentClient(accountEndpoint, accountKey);
            });
        }
    }
}
