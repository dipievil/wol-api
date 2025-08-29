using System.Text.Json.Serialization;
using WolServer.Models;

namespace WolServer.Serialization
{
    [JsonSerializable(typeof(WakeOnLanRequest))]
    [JsonSerializable(typeof(WakeOnLanResponse))]
    public partial class AppJsonSerializerContext : JsonSerializerContext
    {

    }
}

