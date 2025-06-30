
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using RTS.EventBus;
using RTS.Events;

namespace RTS.Units
{
    public class BaseBuilding : AbstractCommandable
    {
        private const int MAX_QUEUE_SIZE = 5;
        public AbstractUnitSO[] Queue => buildQueue.ToArray();
        [field: SerializeField] public float CurrentQueueStartTime { get; private set; }
        [field: SerializeField] public BuildingSO BuildingSO { get; private set; }
        [field: SerializeField] public AbstractUnitSO BuildingUnit { get; private set; }
        [field: SerializeField] public MeshRenderer MainRenderer { get; private set; }
        [SerializeField] private Material primaryMaterial;
        [SerializeField] private NavMeshObstacle navMeshObstacle;
        private List<AbstractUnitSO> buildQueue = new(MAX_QUEUE_SIZE);
        [field: SerializeField] public BuildingProgress Progress { get; private set; } = new(BuildingProgress.BuildingState.Destroyed, 0, 0);
        private IBuildingBuilder unitBuildingThis;
        public int QueueSize => buildQueue.Count;
        public delegate void QueueUpdatedEvent(AbstractUnitSO[] unitsInQueue);
        public event QueueUpdatedEvent OnQueueUpdated;
        private void Awake()
        {
            BuildingSO = UnitSO as BuildingSO;
        }

        protected override void Start()
        {
            base.Start();
            if (MainRenderer != null)
            {
                MainRenderer.material = primaryMaterial;
            }

            Progress = new BuildingProgress(BuildingProgress.BuildingState.Completed, Progress.StartTime, 1);
            unitBuildingThis = null;
            Bus<UnitDeathEvent>.onEvent -= HandleUnitDeath;
        }
        public void BuildUnit(AbstractUnitSO unit)
        {
            if (buildQueue.Count >= MAX_QUEUE_SIZE) return;

            Bus<SupplyEvent>.Raise(new(-unit.Cost.Gas, unit.Cost.GasSO));
            Bus<SupplyEvent>.Raise(new(-unit.Cost.Minerals, unit.Cost.MineralsSO));

            buildQueue.Add(unit);

            if (buildQueue.Count == 1)
            {
                StartCoroutine(DoBuildUnits());
            }
            else
            {
                OnQueueUpdated?.Invoke(buildQueue.ToArray());
            }
        }

        public void CancelBuildingUnit(int index)
        {
            if (index < 0 || index >= buildQueue.Count)
            {
                Debug.LogError($"Attempting to cancel building a unit outside the bounds of the queue");
                return;
            }

            buildQueue.RemoveAt(index);
            if (index == 0)
            {
                StopAllCoroutines();
                if (QueueSize > 0)
                {
                    StartCoroutine(DoBuildUnits());
                }
                else
                {
                    OnQueueUpdated?.Invoke(buildQueue.ToArray());
                }
            }
            else
            {
                OnQueueUpdated?.Invoke(buildQueue.ToArray());
            }
        }

        public void StartBuilding(IBuildingBuilder buildingBuilder)
        {
            unitBuildingThis = buildingBuilder;
            MainRenderer.material = BuildingSO.PlacementMaterial;

            Progress = new(BuildingProgress.BuildingState.Building, Time.time - BuildingSO.BuildTime * Progress.Progress, Progress.Progress);

            Bus<UnitDeathEvent>.onEvent -= HandleUnitDeath;
            Bus<UnitDeathEvent>.onEvent += HandleUnitDeath;
        }

        private void HandleUnitDeath(UnitDeathEvent e)
        {
            if (e.Unit.TryGetComponent(out IBuildingBuilder builder) && builder == unitBuildingThis)
            {
                Progress = new BuildingProgress(BuildingProgress.BuildingState.Paused, Progress.StartTime, (Time.time - Progress.StartTime) / BuildingSO.BuildTime);
                Bus<UnitDeathEvent>.onEvent -= HandleUnitDeath;
            }
        }

        private IEnumerator DoBuildUnits()
        {
            while (buildQueue.Count > 0)
            {
                BuildingUnit = buildQueue[0];
                CurrentQueueStartTime = Time.time;
                OnQueueUpdated?.Invoke(buildQueue.ToArray());
                yield return new WaitForSeconds(BuildingUnit.BuildTime);
                Instantiate(BuildingUnit.Prefab, transform.position, Quaternion.identity);
                buildQueue.RemoveAt(0);
            }
            OnQueueUpdated.Invoke(buildQueue.ToArray());
        }

        private void OnDestroy()
        {
            Bus<UnitDeathEvent>.onEvent -= HandleUnitDeath;
        }
    }

}