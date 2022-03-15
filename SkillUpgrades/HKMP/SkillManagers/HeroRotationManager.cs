using System.Collections.Generic;
using UnityEngine.SceneManagement;
using SkillUpgrades.HKMP.Packets;
using SkillUpgrades.Components;
using Hkmp.Api.Client;

namespace SkillUpgrades.HKMP.SkillManagers
{
    public static class HeroRotationManager
    {
        public static Dictionary<ushort, float> PlayerRotationValues = new();

        private static IClientApi clientApi;

        public static void Initialize(IClientApi clientApi)
        {
            HeroRotationManager.clientApi = clientApi;

            clientApi.ClientManager.ConnectEvent += SendCurrentRotation;
            clientApi.ClientManager.PlayerConnectEvent += SendCurrentRotation;
            clientApi.ClientManager.PlayerEnterSceneEvent += SendCurrentRotation;

            HeroRotator.OnHeroRotate += SendRotation;

            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SendCurrentRotation;
        }

        private static void SendCurrentRotation(Scene oldScene, Scene newScene) => SendCurrentRotation();
        private static void SendCurrentRotation(IClientPlayer player) => SendCurrentRotation();
        private static void SendCurrentRotation() => SendRotation(HeroRotator.Instance.GetCurrentRotation());

        private static void SendRotation(float angle)
        {
            clientApi.NetClient
                .GetNetworkSender<PacketId>(SkillUpgradesClientAddon.Instance)
                .SendSingleData(PacketId.HeroRotation, new HeroRotationPacket() { Rotation = angle });
        }
    }
}
