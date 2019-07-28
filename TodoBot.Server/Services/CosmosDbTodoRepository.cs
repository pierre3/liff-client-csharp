using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TodoBot.Shared;

namespace TodoBot.Server.Services
{
    public class CosmosDbTodoRepository : ITodoRepository
    {
        private readonly IDocumentClient documentClient;
        private readonly string dbName = "TodoBot";
        private readonly string collectionName = "TodoList";
        private readonly string todoListPartitionKey = "/userId";

        public CosmosDbTodoRepository(Uri accountEndpoint, string accountKey)
        {
            documentClient = new DocumentClient(accountEndpoint, accountKey);

            documentClient.CreateDatabaseIfNotExistsAsync(new Database { Id = dbName }).Wait();
            var todoList = new DocumentCollection() { Id = collectionName };
            todoList.PartitionKey.Paths.Add(todoListPartitionKey);
            documentClient.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri(dbName), todoList).Wait();
        }

        public async Task<string> CreateTodoAsync(Todo todo)
        {
            try
            {
                var doc = await documentClient.CreateDocumentAsync(
                        UriFactory.CreateDocumentCollectionUri(dbName, collectionName),
                        todo);

                return doc.Resource.Id;
            }
            catch (Exception e)
            {
                throw new TodoRepositoryException($"An error occurred in the {nameof(CreateTodoAsync)} methods.", e);
            }
        }

        public async Task UpdateTodoAsync(string id, Todo todo)
        {
            try
            {
                todo.Id = id;
                await documentClient.ReplaceDocumentAsync(
                    UriFactory.CreateDocumentUri(dbName, collectionName, id),
                    todo);
            }
            catch (Exception e)
            {
                throw new TodoRepositoryException($"An error occurred in the {nameof(UpdateTodoAsync)} methods.", e);
            }
        }

        public Task<IList<Todo>> GetTodoListAsync(string userId)
        {
            try
            {
                var query = documentClient.CreateDocumentQuery<Todo>(
                    UriFactory.CreateDocumentCollectionUri(dbName, collectionName),
                    new FeedOptions() { PartitionKey = new PartitionKey(userId) })
                    .AsEnumerable();
                    
                return Task.FromResult<IList<Todo>>(query.OrderBy(todo => todo.DueDate).ToList());
                
            }
            catch (Exception e)
            {
                throw new TodoRepositoryException($"An error occurred in the {nameof(GetTodoListAsync)} methods.", e);
            }
        }

        public async Task<Todo> GetTodoAsync(string userId, string id)
        {
            var doc = await documentClient.ReadDocumentAsync<Todo>(UriFactory.CreateDocumentUri(dbName, collectionName, id)
                ,new RequestOptions() { PartitionKey=new PartitionKey(userId)});
            return doc.Document;
        }

        public async Task DeleteTodoAsync(string userId, string id)
        {
            try
            {
                await documentClient.DeleteDocumentAsync(
                    UriFactory.CreateDocumentUri(dbName, collectionName, id),
                    new RequestOptions() { PartitionKey = new PartitionKey(userId)});
            }
            catch (Exception e)
            {
                throw new TodoRepositoryException($"An error occurred in the {nameof(DeleteTodoAsync)} methods.", e);
            }
        }
    }
}
