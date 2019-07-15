using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MyTaskBot.Model
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class MyTask
    {
        public string Id { get; set; }
        public string GroupId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public Status Status { get; set; }
        public string RequestedUserId { get; set; }
        public string AssignedUserId { get; set; }

        public MyTask()
        {

        }
        
    }
}