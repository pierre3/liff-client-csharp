using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Converters;
namespace MyTaskBot.Model
{
    [JsonConverter(typeof(StringEnumConverter), new object[] { true })]
    public enum Status
    {
        New,
        Performed,
        Close,
        Canceled
    }
}