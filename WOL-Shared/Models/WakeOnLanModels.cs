namespace WolServer.Models
{
    public record WakeOnLanRequest(string MacAddress);

    public record WakeOnLanResponse(bool Success);
}

