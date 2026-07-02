using Reflex.Core;
using TileMatch3.Core.BoardSystem;
using TileMatch3.Core.Global;
using TileMatch3.Core.Level;
using UnityEngine;

namespace TileMatch3.Gameplay.GameplayScene
{
    public class GameplayCoreInstaller : MonoBehaviour, IInstaller
    {
        [Header("Data References")] [SerializeField]
        private LevelGeneratorConfig levelConfig;

        [SerializeField] private GlobalGameplayDataVariable globalDataVariable;

        [Header("Gameplay References")] [SerializeField]
        private BoardController boardController;

        [SerializeField] private RackController rackController;

        public void InstallBindings(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterValue(levelConfig);
            containerBuilder.RegisterValue(globalDataVariable);

            containerBuilder.RegisterValue(boardController);
            containerBuilder.RegisterValue(rackController);
        }
    }
}