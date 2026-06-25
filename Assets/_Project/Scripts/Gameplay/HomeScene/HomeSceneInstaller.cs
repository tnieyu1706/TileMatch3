using Reflex.Core;
using UnityEngine;

namespace TileMatch3.Gameplay.HomeScene
{
    public class HomeSceneInstaller : MonoBehaviour, IInstaller
    {
        public void InstallBindings(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterValue(Object.FindFirstObjectByType<HomeSceneController>());
        }
    }
}
