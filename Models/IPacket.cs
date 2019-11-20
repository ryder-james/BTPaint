using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Networking.Models
{
    public delegate byte[] PacketReadyEventHandler();

    public interface IPacket
    {
        byte[] ToByteArray();
        IPacket Restore();
        int ByteSize();
    }
}
