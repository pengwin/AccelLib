using System;

namespace AccelLib.Models
{
    public enum PacketSignatures : byte
    {
        AccelData = 0xA,
        AccelParams = 0xAC
    }

    public interface IPacket
    {
        PacketSignatures Signature { get; }
        int Size { get;  }
        DateTime Date { get; set; }

        void FromBuffer(DataBuffer buffer);
        void ToBuffer(DataBuffer buffer);
    }
}
