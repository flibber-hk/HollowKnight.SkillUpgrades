using System;
using Hkmp.Networking.Packet;

namespace SkillUpgrades.HKMP.Packets
{
    public static class PacketId
    {
        public enum Enum
        {
            HeroRotation,
        }

        public static IPacketData Instantiator(Enum packetId)
        {
            switch (packetId)
            {
                case Enum.HeroRotation:
                    return new HeroRotationPacket();

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
