using Reflex.Core;
using UnityEngine;

namespace TileMatch3.Gameplay.GameplayScene
{
    public class GameplaySceneInstaller : MonoBehaviour, IInstaller
    {
        [SerializeField] private GameplaySceneController _controller;

        public void InstallBindings(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterValue(_controller);
        }
    }
}
