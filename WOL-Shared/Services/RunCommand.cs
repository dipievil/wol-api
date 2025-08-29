using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace WolServer.Services
{
    public interface IUdpSender
    {
        void Send(byte[] payload, IPEndPoint endpoint);
    }

    public class UdpSender : IUdpSender, IDisposable
    {
        private readonly UdpClient _client = new();

        public void Send(byte[] payload, IPEndPoint endpoint)
        {
            _client.EnableBroadcast = true;
            _client.Send(payload, payload.Length, endpoint);
        }

        public void Dispose() => _client.Dispose();
    }

    public class RunCommand
    {
        private readonly IWolService _wol;

        public RunCommand(IWolService wol)
        {
            _wol = wol;
        }

        public bool SendWakeOnLan(string macAddress) => _wol.SendWakeOnLan(macAddress);
    }
}

