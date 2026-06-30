using Reflex.Attributes;
using SerializableInterface.Runtime;
using TileMatch3.Core.Global;
using TileMatch3.Core.Tile;
using UnityEngine;

namespace TileMatch3.Core.IdleAnimationSystem
{
    public class IdleAnimationRegister : MonoBehaviour
    {
        [SerializeField] private InterfaceReference<IIdleAnimated> rackIdleAnimated;
        [SerializeField] private ScriptableEventTile eventTileGet;
        [SerializeField] private ScriptableEventTile eventTileRelease;
        [Inject] private IdleAnimationController idleAnimationController;

        private void OnEnable()
        {
            if (rackIdleAnimated != null)
            {
                idleAnimationController.AddSpecial(rackIdleAnimated.Value);
            }

            if (eventTileGet != null)
            {
                eventTileGet.OnRaised += OnTileGet;
            }

            if (eventTileRelease != null)
            {
                eventTileRelease.OnRaised += OnTileRelease;
            }
        }

        private void OnTileGet(TileRuntime tile)
        {
            var idle = tile.GetComponent<TileIdleAnimated>();
            if (idle == null) return;

            idleAnimationController.Add(idle);
        }

        private void OnTileRelease(TileRuntime tile)
        {
            var idle = tile.GetComponent<TileIdleAnimated>();
            if (idle == null) return;

            idleAnimationController.Remove(idle);
        }

        private void OnDisable()
        {
            if (rackIdleAnimated != null)
            {
                idleAnimationController.RemoveSpecial(rackIdleAnimated.Value);
            }

            if (eventTileGet != null)
            {
                eventTileGet.OnRaised -= OnTileGet;
            }

            if (eventTileRelease != null)
            {
                eventTileRelease.OnRaised -= OnTileRelease;
            }
        }
    }
}