using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MyTaskBot.Server
{
    public class MyTaskService
    {
        private readonly IDocumentClient documentClient;
        private readonly Uri databaseUri;

        public MyTaskService(IDocumentClient documentClient)
        {
            this.documentClient = documentClient;
            this.documentClient.CreateDatabaseIfNotExistsAsync(new Database { Id = "MyTaskBot" });

            databaseUri = UriFactory.CreateDatabaseUri("MyTaskBot");

            var users = new DocumentCollection() { Id = "Users" };
            users.PartitionKey.Paths.Add("/grpupId");
            users.UniqueKeyPolicy.UniqueKeys.Add(
                new UniqueKey()
                {
                    Paths = new Collection<string>() { "/userId" }
                });
            this.documentClient.CreateDocumentCollectionAsync(databaseUri, users);

            var tasks = new DocumentCollection() { Id = "Tasks" };
            tasks.PartitionKey.Paths.Add("/grpupId");
            this.documentClient.CreateDocumentCollectionAsync(databaseUri, tasks);
        }

        [FunctionName("CreateUser")]
        public async Task<IActionResult> CreateUser(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "users")] Model.User user,
            ILogger log)
        {
            log.LogInformation($"{nameof(CreateUser)} method prosessing...");

            if (string.IsNullOrEmpty(user?.GroupId) || string.IsNullOrEmpty(user?.UserId))
            {
                return new BadRequestResult();
            }
            try
            {
                var doc = await documentClient.CreateDocumentAsync(
                    UriFactory.CreateDocumentCollectionUri("MyTaskBot", "Users"),
                    user);
                return new CreatedResult("", JsonConvert.SerializeObject(new { id = doc.Resource.Id }));
            }
            catch (DocumentClientException e)
            {
                return new BadRequestObjectResult(e.Message);
            }

        }

        [FunctionName("CreateTask")]
        public async Task<IActionResult> CreateTask([HttpTrigger(AuthorizationLevel.Function, "post", Route = "tasks")] HttpRequest req, ILogger log)
        {

            try
            {
                log.LogInformation($"{nameof(CreateTask)} method prosessing...");

                var json = await req.ReadAsStringAsync();
                var myTask = JsonConvert.DeserializeObject<Model.MyTask>(json);

                if (string.IsNullOrEmpty(myTask?.GroupId))
                {
                    return new BadRequestObjectResult(JsonConvert.SerializeObject(new { Message = $"{nameof(myTask.GroupId)} is required." }));
                }
                var doc = await documentClient.CreateDocumentAsync(
                    UriFactory.CreateDocumentCollectionUri("MyTaskBot", "Tasks"),
                    myTask);

                return new CreatedResult("", JsonConvert.SerializeObject(new { id = doc.Resource.Id }));
            }
            catch (JsonSerializationException e)
            {
                return new BadRequestObjectResult(e.Message);
            }
            catch (DocumentClientException e)
            {
                return new BadRequestObjectResult(e.Message);
            }

        }

        [FunctionName("UpdateTask")]
        public async Task<IActionResult> UpdateTask([HttpTrigger(AuthorizationLevel.Function, "put", Route = "tasks/{id}")] HttpRequest req ,string id, ILogger log)
        {
            try
            {
                log.LogInformation($"{nameof(UpdateTask)} method prosessing...");

                var json = await req.ReadAsStringAsync();
                var myTask = JsonConvert.DeserializeObject<Model.MyTask>(json);

                if (string.IsNullOrEmpty(myTask?.GroupId))
                {
                    return new BadRequestObjectResult(JsonConvert.SerializeObject(new { Message = $"{nameof(myTask.GroupId)} is required." }));
                }
                myTask.Id = id;
                await documentClient.ReplaceDocumentAsync(
                    UriFactory.CreateDocumentUri("MyTaskBot", "Tasks", id), 
                    myTask);

                return new OkResult();
            }
            catch (JsonSerializationException e)
            {
                return new BadRequestObjectResult(e.Message);
            }
            catch (DocumentClientException e)
            {
                return new BadRequestObjectResult(e.Message);
            }

        }

    }
}
