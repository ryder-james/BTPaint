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
        Point pointA;
        Point pointB;
        Color color;
        int size;

        public byte[] ToByteArray()
        {
            throw new NotImplementedException();
        }
    }
}
