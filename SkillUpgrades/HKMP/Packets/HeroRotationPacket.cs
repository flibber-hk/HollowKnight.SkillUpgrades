using Hkmp.Networking.Packet;

namespace SkillUpgrades.HKMP.Packets
{
    public class HeroRotationPacket : IRebroadcastablePacketData
    {
        public ushort PlayerId { get; set; } = ushort.MaxValue;
        public float Rotation { get; set; }

        public void WriteData(IPacket packet)
        {
            packet.Write(PlayerId);
            packet.Write(Rotation);
        }
        public void ReadData(IPacket packet)
        {
            PlayerId = packet.ReadUShort();
            Rotation = packet.ReadFloat();
        }

        public bool IsReliable => true;
        public bool DropReliableDataIfNewerExists => true;
    }
}
