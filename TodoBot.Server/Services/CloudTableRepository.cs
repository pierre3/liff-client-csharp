using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TableStorage.Abstractions.POCO;
using TodoBot.Shared;

namespace TodoBot.Server.Services
{
    public class CloudTableRepository : ITodoRepository
    {
        private readonly PocoTableStore<Todo, string, string> tableStore;
        private readonly string tableName = "TodoList";
        
        public CloudTableRepository(string connectionString)
        {
            tableStore = new PocoTableStore<Todo, string, string>(
                tableName, 
                connectionString,
                partitionProperty: todo => todo.UserId, 
                rowProperty: todo => todo.Id);
            if (!tableStore.TableExists())
            {
                tableStore.CreateTable();
            }
        }

        public async Task CreateTodoAsync(Todo todo)
        {
            try
            {
                await tableStore.InsertAsync(todo);               
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
                await tableStore.UpdateAsync(todo);
            }
            catch (Exception e)
            {
                throw new TodoRepositoryException($"An error occurred in the {nameof(UpdateTodoAsync)} methods.", e);
            }
        }

        public async Task<IList<Todo>> GetTodoListAsync(string userId)
        {
            try
            {
                var query = await tableStore.GetByPartitionKeyAsync(userId);
                return query.OrderBy(todo => todo.DueDate).ToList();
                
            }
            catch (Exception e)
            {
                throw new TodoRepositoryException($"An error occurred in the {nameof(GetTodoListAsync)} methods.", e);
            }
        }

        public Task<Todo> GetTodoAsync(string userId, string id)
        {
            try
            {
                return tableStore.GetRecordAsync(userId, id);
            }
            catch (Exception e)
            {
                throw new TodoRepositoryException($"An error occurred in the {nameof(GetTodoAsync)} methods.", e);
            }
        }

        public async Task DeleteTodoAsync(string userId, string id)
        {
            try
            {
                await tableStore.DeleteAsync(new Todo() { UserId = userId, Id = id });
            }
            catch (Exception e)
            {
                throw new TodoRepositoryException($"An error occurred in the {nameof(DeleteTodoAsync)} methods.", e);
            }
        }
    }
}
