using Moq;
using WolServer.Services;

namespace WolServer.Tests;

public class RunCommandTests
{
    [Fact]
    public void SendWakeOnLan_DelegatesToIWolService_ReturnsTrue()
    {
        var mock = new Mock<IWolService>();
        mock.Setup(m => m.SendWakeOnLan(It.IsAny<string>())).Returns(true);
        var run = new RunCommand(mock.Object);

        var result = run.SendWakeOnLan("AA:BB:CC:DD:EE:FF");

        Assert.True(result);
        mock.Verify(m => m.SendWakeOnLan(It.Is<string>(s => s.Contains("AA"))), Times.Once);
    }

    [Fact]
    public void SendWakeOnLan_DelegatesToIWolService_ReturnsFalse()
    {
        var mock = new Mock<IWolService>();
        mock.Setup(m => m.SendWakeOnLan(It.IsAny<string>())).Returns(false);
        var run = new RunCommand(mock.Object);

        var result = run.SendWakeOnLan("invalid-mac");

        Assert.False(result);
        mock.Verify(m => m.SendWakeOnLan(It.IsAny<string>()), Times.Once);
    }
}
