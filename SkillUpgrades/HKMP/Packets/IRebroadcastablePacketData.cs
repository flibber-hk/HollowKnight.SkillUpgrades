using Hkmp.Networking.Packet;

namespace SkillUpgrades.HKMP.Packets
{
    public interface IRebroadcastablePacketData : IPacketData
    {
        public ushort PlayerId { get; set; }
    }
}
