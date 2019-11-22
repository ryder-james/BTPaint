﻿using System;
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
        private List<Socket> clientSockets; // list of everybody connected to the service

        public HostClient(int maxConnections = 100, int port = Client.DefaultPort)
        {
            connectionSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clientSockets = new List<Socket>();

            IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddr = ipHost.AddressList[1];

            IPEndPoint ipEnd = new IPEndPoint(IPAddress.Parse(ipAddr.ToString()), port);

            connectionSocket.Bind(ipEnd);
            connectionSocket.Listen(maxConnections);
        }

        public void BeginAccept()
        {
            connectionSocket.BeginAccept(OnConnectionReceived, connectionSocket);
        }

        private void OnConnectionReceived(IAsyncResult result)
        {
            Socket stateSocket = (Socket)result.AsyncState;
            Socket newConnection = stateSocket.EndAccept(result);

            if (newConnection.Connected)
            {
                StateObject state = new StateObject();
                state.workSocket = newConnection;
                state.buffer = new byte[StateObject.BufferSize];

                // gets us ready to receive the first packet from the new client
                newConnection.BeginReceive(state.buffer, 0, state.buffer.Length, SocketFlags.None, OnPacketReceivedFromClient, state);

                clientSockets.Add(newConnection);
            }

            stateSocket.BeginAccept(OnConnectionReceived, stateSocket);
        }

        public override void Send(byte[] buffer, SocketFlags flags = SocketFlags.None)
        {
            if (clientSockets.Count > 0)
            {
                foreach (Socket client in clientSockets)
                {
                    StateObject state = new StateObject();
                    state.workSocket = client;
                    state.buffer = buffer;

                    client.BeginSend(state.buffer, 0, state.buffer.Length, SocketFlags.None, DefaultSendCallback, state);
                }
            }
        }

        public override void Close()
        {
            base.Close();

            foreach (Socket s in clientSockets)
            {
                if (s != null)
                    s.Close();
            }

            if (connectionSocket != null)
                connectionSocket.Close();
        }

        protected void OnPacketReceivedFromClient(IAsyncResult result)
        {
            base.OnPacketReceived(result);

            StateObject state = (StateObject)result.AsyncState;

            Send(state.buffer);

            int packetSize = state.workSocket.EndReceive(result);
            state.buffer = new byte[packetSize];


            // this "workSocket" is essentially the connection to the client
            // gets us ready to receive something from the client once again
            state.workSocket.BeginReceive(state.buffer, 0, state.buffer.Length, SocketFlags.None, OnPacketReceivedFromClient, state);
        }
    }
}
