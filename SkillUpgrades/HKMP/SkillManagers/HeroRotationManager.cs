using System;
using System.Collections.Generic;
using Hkmp.Api.Client;
using Hkmp.Api.Client.Networking;
using Modding;
using Modding.Utils;
using UnityEngine.SceneManagement;
using SkillUpgrades.Components;
using SkillUpgrades.HKMP.Packets;

namespace SkillUpgrades.HKMP.SkillManagers
{
    public class HeroRotationManager
    {
        public Dictionary<ushort, float> PlayerRotationValues = new();

        protected IClientApi clientApi;

        public void Initialize(IClientApi clientApi, IClientAddonNetworkReceiver<PacketId.Enum> netReceiver)
        {
            this.clientApi = clientApi;

            clientApi.ClientManager.ConnectEvent += OnConnect;
            clientApi.ClientManager.DisconnectEvent += OnDisconnect;


            netReceiver.RegisterPacketHandler<HeroRotationPacket>(PacketId.Enum.HeroRotation, RecordRotation);
        }

        private void OnConnect()
        {
            SendCurrentRotation();

            clientApi.ClientManager.PlayerConnectEvent += SendCurrentRotation;
            clientApi.ClientManager.PlayerEnterSceneEvent += SendCurrentRotation;
            clientApi.ClientManager.PlayerEnterSceneEvent += RotatePlayerOnEnterScene;
            HeroRotator.OnHeroRotate += SendRotation;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnSceneChange;
        }
        private void OnDisconnect()
        {
            clientApi.ClientManager.PlayerConnectEvent -= SendCurrentRotation;
            clientApi.ClientManager.PlayerEnterSceneEvent -= SendCurrentRotation;
            clientApi.ClientManager.PlayerEnterSceneEvent -= RotatePlayerOnEnterScene;
            HeroRotator.OnHeroRotate -= SendRotation;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= OnSceneChange;
        }

        private void OnSceneChange(Scene oldScene, Scene newScene)
        {
            // Check for gameplay scene

            foreach ((ushort playerId, float rotation) in PlayerRotationValues)
            {
                if (clientApi.ClientManager.TryGetPlayer(playerId, out IClientPlayer player) && player.IsInLocalScene)
                {
                    DoRotate(player, rotation);
                }
            }
        }

        private void RotatePlayerOnEnterScene(IClientPlayer player)
        {
            if (!PlayerRotationValues.TryGetValue(player.Id, out float rotation))
            {
                PlayerRotationValues[player.Id] = rotation = 0;
            }

            if (rotation != 0)
            {
                DoRotate(player, rotation);
            }
        }

        private void RecordRotation(HeroRotationPacket packet)
        {
            PlayerRotationValues[packet.PlayerId] = packet.Rotation;

            if (clientApi.ClientManager.TryGetPlayer(packet.PlayerId, out IClientPlayer player) && player.IsInLocalScene)
            {
                DoRotate(player, packet.Rotation);
            }
        }

        private void DoRotate(IClientPlayer player, float rotation)
        {
            player.PlayerObject.GetOrAddComponent<StableRotator>().SetRotation(rotation);
        }

        private void SendCurrentRotation(IClientPlayer player) => SendCurrentRotation();
        private void SendCurrentRotation() => SendRotation(HeroRotator.Instance.GetCurrentRotation());

        private void SendRotation(float angle)
        {
            if (!clientApi.NetClient.IsConnected) return;

            clientApi.NetClient
                .GetNetworkSender<PacketId.Enum>(SkillUpgradesClientAddon.Instance)
                .SendSingleData(PacketId.Enum.HeroRotation, new HeroRotationPacket() { Rotation = angle });
        }
    }
}
