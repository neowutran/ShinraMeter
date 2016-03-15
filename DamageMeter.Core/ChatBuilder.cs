using System;
using System.Linq;
using System.Windows.Media;
using Tera.Sniffing.Crypt;

namespace DamageMeter
{
    /*
    NOT USED, DO NOT USE.
    */

    public class ChatBuilder
    {
        private readonly byte[] _say = StringToByteArray("0A0000000000");

        private string _chatMessage = "";


       // public Cryptor EnCryptor => Session.Instance.ChatEncryptor;
        /*
       Some problem with predicting the next sequence number(but totaly possible as we know everything that goes on the network), but anyway, something like that will cause more harm than good.
       Was fun anyway, that thing should be interesting with a full console client, so you can send colorfull chat message.
       ABORDING.
    */

        public ChatBuilder Add(string text, Color color)
        {
            var colorHexa = HexColor(color);
            _chatMessage += "<FONT COLOR=\"" + colorHexa + "\" KERNING=\"0\" SIZE=\"18\" FACE=\"$ChatFont\">" + text +
                            "</FONT>";
            return this;
        }

        public ChatBuilder Add(string text)
        {
            _chatMessage += "<FONT>" + text + "</FONT>";
            return this;
        }

        private static string HexColor(Color color)
        {
            return "#" + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
        }

        /*
        public void Send()
        {
            var bytesChat = Encoding.Unicode.GetBytes(_chatMessage);
            var c_chatByte = BitConverter.GetBytes(NetworkController.Instance.TeraData.OpCodeNamer.GetCode("C_CHAT"));
            var packetPayload = c_chatByte.Concat(_say).Concat(bytesChat).ToArray();

            //Console.WriteLine("not encrypted");
            Console.WriteLine(BitConverter.ToString(packetPayload));
            EnCryptor.ApplyCryptor(packetPayload, packetPayload.Length);

            //Console.WriteLine("Encrypted");
            Console.WriteLine(BitConverter.ToString(packetPayload));

            
            var serverPort = (ushort) NetworkController.Instance.ServerIpEndPoint.Port;
            var clientPort = (ushort) NetworkController.Instance.ClientIpEndPoint.Port;
            var tcpPacket = new TcpPacket(clientPort, serverPort)
            {
                WindowSize = (ushort) (packetPayload.Count()*8),
                SequenceNumber = TcpConnection.NextSequenceNumber,
                PayloadData = packetPayload,
                Psh = true
            };

            // Console.WriteLine(TcpConnection.NextSequenceNumber);


            var ipPacket = new IPv4Packet(NetworkController.Instance.ClientIpEndPoint.Address,
                NetworkController.Instance.ServerIpEndPoint.Address)
            {
                Protocol = IPProtocolType.TCP,
                TimeToLive = 128,
                Id = (ushort) TcpConnection.NextSequenceNumber,
                FragmentFlags = 0x02,
                PayloadData = tcpPacket.BytesHighPerformance.Bytes
            };

            var networkDevice =
                NetworkInterface.GetAllNetworkInterfaces()
                    .FirstOrDefault(device => device.GetPhysicalAddress().Equals(TeraSniffer.Instance.Device.));
            var address = networkDevice.GetIPProperties().GatewayAddresses.FirstOrDefault();

            var all = IpHelper.GetAllDevicesOnLan();
            PhysicalAddress gatewayMac = null;
            foreach (var kvp in all.Where(kvp => kvp.Key.Equals(address.Address)))
            {
                gatewayMac = kvp.Value;
            }


            var ethernetPacket = new EthernetPacket(TeraSniffer.Instance.Device.MacAddress, gatewayMac,
                EthernetPacketType.IpV4) {PayloadPacket = ipPacket};


            ipPacket.ParentPacket = ethernetPacket;
            tcpPacket.ParentPacket = ipPacket;

            ipPacket.UpdateIPChecksum();
            ethernetPacket.UpdateCalculatedValues();
            //   Console.WriteLine("Send");
            TeraSniffer.Instance.Device.Send(ethernetPacket.Bytes);
        }
        */

        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                .Where(x => x%2 == 0)
                .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                .ToArray();
        }
    }
}