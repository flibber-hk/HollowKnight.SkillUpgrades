using Hkmp.Api.Server;
using Hkmp.Api.Server.Networking;
using SkillUpgrades.HKMP.Packets;

namespace SkillUpgrades.HKMP
{
    public class SkillUpgradesServerAddon : ServerAddon
    {
        public static SkillUpgradesServerAddon Instance { get; internal set; }

        protected override string Name => nameof(SkillUpgrades);
        protected override string Version => SkillUpgrades.GetSkillUpgradesVersion();
        public override bool NeedsNetwork => true;
        public override void Initialize(IServerApi serverApi)
        {
            IServerAddonNetworkReceiver<PacketId> receiver = serverApi.NetServer.GetNetworkReceiver<PacketId>(this, SkillUpgradesPackets.Instantiator);

            receiver.RegisterPacketHandler<HeroRotationPacket>(PacketId.HeroRotation, Rebroadcast);
        }

        public void Rebroadcast(ushort id, HeroRotationPacket packet)
        {
            packet.PlayerId = id;

            ServerApi.NetServer.GetNetworkSender<PacketId>(this).BroadcastSingleData(PacketId.HeroRotation, packet);
        }
    }
}
