using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using TodoBot.Server.Services;

[assembly: FunctionsStartup(typeof(TodoBot.Server.Startup))]
namespace TodoBot.Server
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton<ITodoRepository>(provider => 
            {
                var configuration = provider.GetRequiredService<IConfiguration>();

                var accountEndpoint = new Uri(configuration.GetValue<string>("Cosmos:AccountEndpoint"));
                var accountKey = configuration.GetValue<string>("Cosmos:AccountKey");
                
                return new CosmosDbTodoRepository(accountEndpoint, accountKey);
            });
        }
    }
}
