using Reflex.Core;
using TileMatch3.Core.IdleAnimationSystem;
using TileMatch3.Core.PowerUp;
using UnityEngine;

namespace TileMatch3.Gameplay.GameplayScene
{
    public class GameplaySubInstaller : MonoBehaviour, IInstaller
    {
        [Header("Gameplay References")] [SerializeField]
        private IdleAnimationController idleAnimationController;

        [SerializeField] private UndoRecordSystem undoRecordSystem;
        [SerializeField] private HintSystem hintSystem;
        [SerializeField] private PowerManager powerManager;

        public void InstallBindings(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterValue(idleAnimationController);
            containerBuilder.RegisterValue(undoRecordSystem);
            containerBuilder.RegisterValue(hintSystem);
            containerBuilder.RegisterValue(powerManager);
        }
    }
}