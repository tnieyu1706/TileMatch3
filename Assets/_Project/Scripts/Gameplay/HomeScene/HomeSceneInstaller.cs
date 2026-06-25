using Reflex.Core;
using UnityEngine;

namespace TileMatch3.Gameplay.HomeScene
{
    public class HomeSceneInstaller : MonoBehaviour, IInstaller
    {
        [SerializeField] private HomeSceneController homeSceneController;

        public void InstallBindings(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterValue(homeSceneController);
        }
    }
}