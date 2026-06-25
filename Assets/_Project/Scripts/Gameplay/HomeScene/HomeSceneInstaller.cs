using Reflex.Core;
using UnityEngine;

namespace TileMatch3.Gameplay.HomeScene
{
    public class HomeSceneInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<HomeSceneController>().AsSingle();
        }
    }
}
