using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Networking.Models
{
    public class HostClient : Client
    {
        private Socket serverSocket;
        private IPHostEntry ipHost;
        private IPAddress ipAddr;

        public HostClient()
        {
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ipHost = Dns.GetHostEntry(Dns.GetHostName());
            ipAddr = ipHost.AddressList[1];
        }

        public void BeginAccept(int maxConnections = 100, int port = 10000)
        {
            IPEndPoint ipEnd = new IPEndPoint(IPAddress.Parse(ipAddr.ToString()), port);
            Debug.WriteLine("Endpoint created at: " + ipAddr.ToString());

            serverSocket.Bind(ipEnd);
            serverSocket.Listen(maxConnections);
            serverSocket.BeginAccept(OnConnectionEstablished, serverSocket);
        }

        private void OnConnectionEstablished(IAsyncResult result)
        {
            Debug.WriteLine("Attempting Connection");

            Socket socket = ((Socket)result.AsyncState).EndAccept(result);

            if (socket.Connected)
            {
                Debug.WriteLine("Connection established");

                AsyncPacket packet = new AsyncPacket();
                packet.result = result;
                packet.socket = socket;

                byte[] data = new byte[256];
                socket.BeginReceive(data, 0, data.Length, SocketFlags.None, OnPacketReceived, packet);
            }

            //((Socket)result.AsyncState).BeginAccept(OnConnectionEstablished, ((Socket)result.AsyncState));
        }

        private void OnPacketReceived(IAsyncResult result)
        {
            Debug.WriteLine("Packet received");

            AsyncPacket state = (AsyncPacket)result.AsyncState;
            int packetSize = state.socket.EndReceive(result);

            state.result = result;

            byte[] data = new byte[packetSize];
            state.socket.BeginReceive(data, 0, data.Length, SocketFlags.None, OnPacketReceived, state);
        }

        private struct AsyncPacket
        {
            public IAsyncResult result;
            public Socket socket;
        }
    }
}
