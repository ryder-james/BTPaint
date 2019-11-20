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

        Point pointA;
        Point pointB;
        Color color;
        int size;

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

        public IPacket Restore()
        {
            return null;
        }

        public int ByteSize()
        {
            return 1;
        }
    }
}
