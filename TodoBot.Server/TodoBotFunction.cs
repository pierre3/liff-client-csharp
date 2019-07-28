using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Threading.Tasks;
using TodoBot.Server.Services;
using TodoBot.Shared;

namespace TodoBot.Server
{
    public class TodoBotFunction
    {
        private readonly ITodoRepository todoRepository;
        
        public TodoBotFunction(ITodoRepository todoRepository)
        {
            this.todoRepository = todoRepository;
        }

        [FunctionName("CreateTodo")]
        public async Task<IActionResult> CreateTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "todoList")] HttpRequest req, 
            ILogger log)
        {
            log.LogInformation($"{nameof(CreateTodo)} method prosessing...");
            try
            {
                var json = await req.ReadAsStringAsync();
                var todo = JsonConvert.DeserializeObject<Todo>(json);

                if (string.IsNullOrEmpty(todo?.UserId))
                {
                    return new BadRequestObjectResult(JsonConvert.SerializeObject(new { Message = $"{nameof(todo.UserId)} is required." }));
                }

                var id = await todoRepository.CreateTodoAsync(todo);

                return new CreatedResult("", JsonConvert.SerializeObject(new { id }));
            }
            catch (JsonSerializationException e)
            {
                return new BadRequestObjectResult(e.Message);
            }
            catch (TodoRepositoryException e)
            {
                return new BadRequestObjectResult($"{e.Message}: {e.InnerException.Message}");
            }
        }

        [FunctionName("UpdateTodo")]
        public async Task<IActionResult> UpdateTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "todoList/{id}")] HttpRequest req ,
            string id, 
            ILogger log)
        {
            log.LogInformation($"{nameof(UpdateTodo)} method prosessing...");
            try
            {
                var json = await req.ReadAsStringAsync();
                var todo = JsonConvert.DeserializeObject<Todo>(json);

                await todoRepository.UpdateTodoAsync(id, todo);
                return new OkResult();
            }
            catch (JsonSerializationException e)
            {
                return new BadRequestObjectResult(e.Message);
            }
            catch (TodoRepositoryException e)
            {
                return new BadRequestObjectResult($"{e.Message}: {e.InnerException.Message}");
            }

        }

        [FunctionName("GetTodoList")]
        public async Task<IActionResult> GetTodoList(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{userId}/todoList")] HttpRequest req,
            string userId,
            ILogger log)
        {
            log.LogInformation($"{nameof(GetTodoList)} method prosessing...");
            try
            {
                var todolist = await todoRepository.GetTodoListAsync(userId);
                return new OkObjectResult(todolist);
            }
            catch (JsonSerializationException e)
            {
                return new BadRequestObjectResult(e.Message);
            }
            catch (TodoRepositoryException e)
            {
                return new BadRequestObjectResult($"{e.Message}: {e.InnerException.Message}");
            }
        }

        [FunctionName("GetTodo")]
        public async Task<IActionResult> GetTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{userId}/todoList/{id}")] HttpRequest req,
            string userId,
            string id,
            ILogger log)
        {
            log.LogInformation($"{nameof(GetTodoList)} method prosessing...");
            try
            {
                var todo = await todoRepository.GetTodoAsync(userId, id);
                return new OkObjectResult(todo);
            }
            catch(JsonSerializationException e)
            {
                return new BadRequestObjectResult(e.Message);
            }
            catch(TodoRepositoryException e)
            {
                return new BadRequestObjectResult($"{e.Message}: {e.InnerException.Message}");
            }
        }

        [FunctionName("DeleteTodo")]
        public async Task<IActionResult> DeleteTodoAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "{userId}/todoList/{id}")] HttpRequest req,
            string userId,
            string id,
            ILogger log)
        {
            log.LogInformation($"{nameof(DeleteTodoAsync)} method prosessing...");
            try
            {
                await todoRepository.DeleteTodoAsync(userId, id);
                return new OkResult();
            }
            catch (JsonSerializationException e)
            {
                return new BadRequestObjectResult(e.Message);
            }
            catch (TodoRepositoryException e)
            {
                return new BadRequestObjectResult($"{e.Message}: {e.InnerException.Message}");
            }
        }
    }

}
