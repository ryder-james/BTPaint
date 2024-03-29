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
    public class GuestClient : Client
    {
        private Socket clientSocket;
        private IPEndPoint serverEndPoint;

        public void BeginConnect(IPEndPoint targetAddress)
        {
            StateObject state = new StateObject();
            state.workSocket = connectionSocket;

            connectionSocket.BeginConnect(targetAddress, DefaultConnectCallback, state);
        }

        public override void Send(byte[] buffer, SocketFlags flags = SocketFlags.None)
        {
            if (clientSocket == null || !clientSocket.Connected)
            {
                return;
            }

            StateObject state = new StateObject();
            state.workSocket = clientSocket;
            state.buffer = buffer;

            // Sends a packet to the remote end point, in this case the server
            clientSocket.BeginSend(state.buffer, 0, state.buffer.Length, flags, DefaultSendCallback, state);
        }

        public override void Close()
        {
            base.Close();

            if (clientSocket != null)
                clientSocket.Close();
        }

        protected void DefaultConnectCallback(IAsyncResult result)
        {
            StateObject state = (StateObject)result.AsyncState;
            try 
            {
                state.workSocket.EndConnect(result);
            }
            catch (ObjectDisposedException ex)
            {
                return;
            }
            clientSocket = state.workSocket;
            serverEndPoint = (IPEndPoint) clientSocket.RemoteEndPoint;

            base.FireRemoteConnected(serverEndPoint);

            state.workSocket = clientSocket;
            state.buffer = new byte[StateObject.BufferSize];

            // gets us ready to receive the first packet from the new client
            clientSocket.BeginReceive(state.buffer, 0, state.buffer.Length, SocketFlags.None, OnPacketReceivedFromServer, state);
        }

        private void OnPacketReceivedFromServer(IAsyncResult result)
        {
            bool realPacket = false;
            StateObject state = (StateObject)result.AsyncState;
            for (int i = 0; i < Client.BlockPacket.Length; i++)
            {
                if (state.buffer[i] != Client.BlockPacket[i])
                {
                    realPacket = true;
                    break;
                }
            }

            if (!realPacket)
            {
                state.workSocket.EndReceive(result);
                base.FireConnectionFailed();
                return;
            }

            base.OnPacketReceived(result);

            int packetSize;
            try
            {
                packetSize = state.workSocket.EndReceive(result);
            }
            catch (Exception e) when (e is ObjectDisposedException || e is SocketException) 
            {
                base.FireRemoteDisconnected(serverEndPoint);
                return;
            }

            state.buffer = new byte[packetSize];

            // this "workSocket" is essentially the connection to the client
            // gets us ready to receive something from the client once again
            state.workSocket.BeginReceive(state.buffer, 0, state.buffer.Length, SocketFlags.None, OnPacketReceivedFromServer, state);
        }
    }
}
