using System;

namespace TodoBot.Server.Services
{

    public class TodoRepositoryException: Exception
    {
        public TodoRepositoryException(Exception innerException) : base(innerException.Message, innerException)
        {
        }

        public TodoRepositoryException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
