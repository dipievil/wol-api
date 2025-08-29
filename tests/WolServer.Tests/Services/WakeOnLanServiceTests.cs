using System.Net;
using Moq;
using WolServer.Services;

namespace WolServer.Tests.Services;

public class WakeOnLanServiceTests
{
    [Fact]
    public void SendWakeOnLan_ValidMac_CallsSenderAndReturnsTrue()
    {
        var mock = new Mock<IUdpSender>();
        var service = new WakeOnLanService(mock.Object);
        var mac = "AA:BB:CC:DD:EE:FF";

        var result = service.SendWakeOnLan(mac);

        Assert.True(result);
        mock.Verify(m => m.Send(It.IsAny<byte[]>(), It.Is<IPEndPoint>(ep => ep.Port == 9)), Times.Once);
    }

    [Fact]
    public void SendWakeOnLan_InvalidMac_ReturnsFalse()
    {
        var mock = new Mock<IUdpSender>();
        var service = new WakeOnLanService(mock.Object);

        var result = service.SendWakeOnLan("invalid-mac");

        Assert.False(result);
        mock.Verify(m => m.Send(It.IsAny<byte[]>(), It.IsAny<IPEndPoint>()), Times.Never);
    }
}

