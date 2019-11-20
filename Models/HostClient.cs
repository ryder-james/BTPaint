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
        private List<Socket> clientSockets;
        private IPHostEntry ipHost;
        private IPAddress ipAddr;

        public HostClient()
        {
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clientSockets = new List<Socket>();
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

            Socket stateSocket = (Socket)result.AsyncState;
            Socket newConnection = stateSocket.EndAccept(result);

            if (newConnection.Connected)
            {
                Debug.WriteLine("Connection established");

                AsyncPacket packet = new AsyncPacket();
                packet.result = result;
                packet.socket = newConnection;

                byte[] data = new byte[256];
                newConnection.BeginReceive(data, 0, data.Length, SocketFlags.Multicast, base.OnPacketReceived, packet);

                clientSockets.Add(newConnection);
            }

            stateSocket.BeginAccept(OnConnectionEstablished, stateSocket);
        }

        public override void Close()
        {
            foreach (Socket s in clientSockets)
            {
                s.Close();
            }
            serverSocket.Close();
            base.Close();
        }
    }
}
