using System;
using Hkmp.Networking.Packet;

namespace SkillUpgrades.HKMP.Packets
{
    public enum PacketId
    {
        HeroRotation,
    }
    public static class SkillUpgradesPackets
    {
        public static IPacketData Instantiator(PacketId packetId)
        {
            switch (packetId)
            {
                case PacketId.HeroRotation:
                    return new HeroRotationPacket();

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
