using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;

namespace TodoBot.Shared
{ 
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class Todo
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public Status Status { get; set; }
        public DateTime TimeLimit { get; set; }  
        public Todo()
        {

        }
        
    }
}