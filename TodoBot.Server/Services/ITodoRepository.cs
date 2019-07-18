using System.Threading.Tasks;
using TodoBot.Shared;

namespace TodoBot.Server.Services
{
    public interface ITodoRepository
    {
        Task<string> CreateTodoAsync(Todo todo);
        Task UpdateTodoAsync(string id, Todo todo);
    }
}