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
using System.Linq;
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

            if (string.IsNullOrEmpty(user?.GroupId))
            {
                return new BadRequestObjectResult(JsonConvert.SerializeObject(new { Message = $"{nameof(user.GroupId)} is required." }));
            }
            if (string.IsNullOrEmpty(user?.UserId))
            {
                return new BadRequestObjectResult(JsonConvert.SerializeObject(new { Message = $"{nameof(user.UserId)} is required." }));
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

        [FunctionName("UpdateUser")]
        public async Task<IActionResult> UpdateUser(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "users/{id}")] HttpRequest req, 
            string id,
            ILogger log)
        {
            try
            {
                log.LogInformation($"{nameof(UpdateTask)} method prosessing...");

                var json = await req.ReadAsStringAsync();
                var user = JsonConvert.DeserializeObject<Model.User>(json);
                
                await documentClient.ReplaceDocumentAsync(
                    UriFactory.CreateDocumentUri("MyTaskBot", "Users", id),
                    user);

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

        [FunctionName("GetUsers")]
        public IActionResult GetUsers(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "{grpupId}/users")] string groupId, 
            ILogger log)
        {
            try
            {
                log.LogInformation($"{nameof(GetUsers)} method prosessing...");

                var query = documentClient.CreateDocumentQuery<User>(UriFactory.CreateDocumentCollectionUri("MyTaskBot", "Users"),
                    new FeedOptions() { PartitionKey = new PartitionKey(groupId) }).AsEnumerable();
                return new JsonResult(query.ToArray());
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

        [FunctionName("GetUser")]
        public async Task<IActionResult> GetUser(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "{grpupId}/users/{userId}")] string groupId,
            string userId,
            ILogger log)
        {
            try
            {
                log.LogInformation($"{nameof(GetUser)} method prosessing...");

                var response =
                    await documentClient.ReadDocumentAsync<User>(
                        UriFactory.CreateDocumentUri("MyTaskBot", "Users", userId),
                        new RequestOptions() { PartitionKey = new PartitionKey(groupId) });
                return new JsonResult(response.Document);
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

        [FunctionName("CreateTask")]
        public async Task<IActionResult> CreateTask(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "tasks")] HttpRequest req, 
            ILogger log)
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
        public async Task<IActionResult> UpdateTask(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "tasks/{id}")] HttpRequest req ,
            string id, 
            ILogger log)
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
