using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MyTaskBot.Model
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class User
    {
        public string Id { get; set; }
        public string GroupId { get; set; }
        public string UserId { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public User()
        {

        }
    }
}