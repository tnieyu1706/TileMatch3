using System.Collections.Generic;
using System.Linq;
using Reflex.Attributes;
using TileMatch3.Core.BoardSystem;
using TileMatch3.Core.Global;
using TileMatch3.Core.Tile;
using UnityEngine;

namespace TileMatch3.Core.IdleAnimationSystem
{
    public interface IIdleAnimated
    {
        public void Play();
    }

    public class IdleAnimationController : MonoBehaviour
    {
        [SerializeField] private float debounceTime = 3f;
        [SerializeField] private float specialDebounceTime = 4f;

        [Inject] BoardController boardController;

        private readonly List<TileIdleAnimated> tileAnimatedObjects = new ();
        private readonly List<IIdleAnimated> specialAnimatedObjects = new ();

        public void Add(TileIdleAnimated tileIdleAnimated) => tileAnimatedObjects.Add(tileIdleAnimated);
        public void Remove(TileIdleAnimated tileIdleAnimated) => tileAnimatedObjects.Remove(tileIdleAnimated);
        
        public void AddSpecial(IIdleAnimated idleAnimatedObject) => specialAnimatedObjects.Add(idleAnimatedObject);
        public void RemoveSpecial(IIdleAnimated idleAnimatedObject) => specialAnimatedObjects.Remove(idleAnimatedObject);

        private DebounceTimer debounceTimer;
        private DebounceTimer specialDebounceTimer;

        void Awake()
        {
            debounceTimer = new DebounceTimer(debounceTime);
            debounceTimer.OnTick += OnDebounceTimerTick;
            
            specialDebounceTimer = new DebounceTimer(specialDebounceTime);
            specialDebounceTimer.OnTick += OnSpecialDebounceTimerTick;
        }

        void Start()
        {
            debounceTimer.Start();
            specialDebounceTimer.Start();
        }

        private void OnSpecialDebounceTimerTick()
        {
            Debug.Log("[IdleAnimationController.SpecialDebounce] Timer ticked. Playing idle animation.");

            if (specialAnimatedObjects == null || specialAnimatedObjects.Count == 0) return;
            
            IIdleAnimated idleAnimatedObject = specialAnimatedObjects[Random.Range(0, specialAnimatedObjects.Count)];
            idleAnimatedObject.Play();
        }

        private void OnDebounceTimerTick()
        {
            Debug.Log("[IdleAnimationController.Debounce] Timer ticked. Playing idle animation.");

            if (tileAnimatedObjects == null || tileAnimatedObjects.Count == 0) return;
            
            var filterTiles = tileAnimatedObjects
                .Where(t => t.tileRuntime.IsActive)
                .ToList();
            
            IIdleAnimated idleAnimatedObject = filterTiles[Random.Range(0, filterTiles.Count)];
            idleAnimatedObject.Play();
        }

        private void OnDestroy()
        {
            debounceTimer.OnTick -= OnDebounceTimerTick;
            debounceTimer.Stop();
            debounceTimer.Dispose();
            
            specialDebounceTimer.OnTick -= OnSpecialDebounceTimerTick;
            specialDebounceTimer.Stop();
            specialDebounceTimer.Dispose();
        }

        private void OnEnable()
        {
            boardController.onTileClick += ResetDebounceTimers;
        }

        private void ResetDebounceTimers(TileRuntime _)
        {
            ResetDebounceTimers();
        }

        private void OnDisable()
        {
            boardController.onTileClick -= ResetDebounceTimers;
        }

        private void ResetDebounceTimers()
        {
            debounceTimer.Reset();
            specialDebounceTimer.Reset();
        }
    }
}