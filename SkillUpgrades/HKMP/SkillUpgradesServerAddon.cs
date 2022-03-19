using System;
using System.Linq;
using Hkmp.Api.Server;
using Hkmp.Api.Server.Networking;
using Hkmp.Networking.Packet;
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
            IServerAddonNetworkReceiver<PacketId.Enum> receiver = serverApi.NetServer.GetNetworkReceiver<PacketId.Enum>(this, PacketId.Instantiator);

            foreach (PacketId.Enum packetId in Enum.GetValues(typeof(PacketId.Enum)).Cast<PacketId.Enum>())
            {
                receiver.RegisterPacketHandler(packetId, GetRebroadcaster(packetId));
            }
        }

        // Any skill upgrades networking will be of the form "I want to broadcast the data for my relationship with this skill upgrade over the network".
        // So our handler will just take the packet, add the player id and then send it to everyone.
        public GenericServerPacketHandler<IRebroadcastablePacketData> GetRebroadcaster(PacketId.Enum packetId)
        {
            void rebroadcast(ushort id, IRebroadcastablePacketData packet)
            {
                packet.PlayerId = id;

                ServerApi.NetServer.GetNetworkSender<PacketId.Enum>(this).BroadcastSingleData(packetId, packet);
            }

            return rebroadcast;
        }
    }
}
