﻿using Modding.Utils;
using Hkmp.Api.Client;
using Hkmp.Api.Client.Networking;
using SkillUpgrades.Components;
using SkillUpgrades.HKMP.SkillManagers;
using SkillUpgrades.HKMP.Packets;

namespace SkillUpgrades.HKMP
{
    public class SkillUpgradesClientAddon : ClientAddon
    {
        public static SkillUpgradesClientAddon Instance { get; internal set; }

        protected override string Version => SkillUpgrades.GetSkillUpgradesVersion();
        protected override string Name => nameof(SkillUpgrades);
        public override bool NeedsNetwork => true;

        public override void Initialize(IClientApi clientApi)
        {
            HeroRotationManager.Initialize(clientApi);

            IClientAddonNetworkReceiver<PacketId.Enum> netReceiver = clientApi.NetClient.GetNetworkReceiver<PacketId.Enum>(this, PacketId.Instantiator);

            netReceiver.RegisterPacketHandler<HeroRotationPacket>(PacketId.Enum.HeroRotation, RotateRemotePlayer);
        }

        private void RotateRemotePlayer(HeroRotationPacket packet)
        {
            if (ClientApi.ClientManager.TryGetPlayer(packet.PlayerId, out IClientPlayer player) && player.IsInLocalScene)
            {
                player.PlayerObject.GetOrAddComponent<StableRotator>().SetRotation(packet.Rotation);
            }
        }
    }
}
