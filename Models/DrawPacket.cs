using Networking.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTPaint.Models
{
    public struct DrawPacket : IPacket
    {
        public const uint PacketHeader = 0xD34DC0D3;

        public Point pointA;
        public Point pointB;
        public Color color;
        public int size;

        public DrawPacket(Point A, Point B, Color Color, int Size)
        {
            pointA = A;
            pointB = B;
            color = Color;
            size = Size;
        }

        public byte[] ToByteArray()
        {
            List<byte> bytes = new List<byte>();

            bytes.AddRange(BitConverter.GetBytes(PacketHeader));

            byte[] AxBytes = BitConverter.GetBytes(pointA.X);
            byte[] AyBytes = BitConverter.GetBytes(pointA.Y);
            byte[] BxBytes = BitConverter.GetBytes(pointB.X);
            byte[] ByBytes = BitConverter.GetBytes(pointB.Y);
            byte[] colorBytes = BitConverter.GetBytes(color.ToArgb());
            byte[] sizeBytes = BitConverter.GetBytes(size);
            bytes.AddRange(AxBytes);
            bytes.AddRange(AyBytes);
            bytes.AddRange(BxBytes);
            bytes.AddRange(ByBytes);
            bytes.AddRange(colorBytes);
            bytes.AddRange(sizeBytes);

            return bytes.ToArray();
        }

        public static DrawPacket Restore(byte[] bytes)
        {
            int pointAX = BitConverter.ToInt32(bytes, 0);
            int pointAY = BitConverter.ToInt32(bytes, 4);
            int pointBX = BitConverter.ToInt32(bytes, 8);
            int pointBY = BitConverter.ToInt32(bytes, 12);
            Color kolor = Color.FromArgb(bytes[16], bytes[17], bytes[18], bytes[19]);
            int soze = BitConverter.ToInt32(bytes, 20);

            Point A = new Point(pointAX, pointAY);
            Point B = new Point(pointBX, pointBY);

            return new DrawPacket(pointAX, pointAY, pointBX, pointBY);
        }

        public int ByteSize()
        {
            return 1;
        }
    }
}
