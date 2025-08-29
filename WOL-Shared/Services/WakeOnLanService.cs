using System.Net;
using System.Text.RegularExpressions;

namespace WolServer.Services
{
    public interface IWolService
    {
        bool SendWakeOnLan(string macAddress);
    }

    public class WakeOnLanService : IWolService
    {
        private readonly IUdpSender _sender;

        public WakeOnLanService(IUdpSender sender)
        {
            _sender = sender;
        }

        public bool SendWakeOnLan(string macAddress)
        {
            if (string.IsNullOrWhiteSpace(macAddress)) return false;

            var cleaned = Regex.Replace(macAddress, "[-:]", "");
            if (cleaned.Length != 12) return false;

            byte[] macBytes = new byte[6];
            try
            {
                for (int i = 0; i < 6; i++)
                    macBytes[i] = Convert.ToByte(cleaned.Substring(i * 2, 2), 16);
            }
            catch
            {
                return false;
            }

            byte[] packet = new byte[6 + 16 * 6];
            for (int i = 0; i < 6; i++) packet[i] = 0xFF;
            for (int i = 0; i < 16; i++)
                Buffer.BlockCopy(macBytes, 0, packet, 6 + i * 6, 6);

            try
            {
                _sender.Send(packet, new IPEndPoint(IPAddress.Broadcast, 9));
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}

