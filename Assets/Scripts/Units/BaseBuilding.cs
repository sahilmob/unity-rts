
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RTS.Units
{
    public class BaseBuilding : AbstractCommandable
    {
        private const int MAX_QUEUE_SIZE = 5;
        [field: SerializeField] public float CurrentQueueStartTime { get; private set; }
        [field: SerializeField] public UnitSO BuildingUnit { get; private set; }
        private Queue<UnitSO> buildQueue = new(MAX_QUEUE_SIZE);
        public int QueueSize => buildQueue.Count;
        public delegate void QueueUpdatedEvent(UnitSO[] unitsInQueue);
        public event QueueUpdatedEvent OnQueueUpdated;
        public void BuildUnit(UnitSO unit)
        {
            if (buildQueue.Count >= MAX_QUEUE_SIZE) return;

            buildQueue.Enqueue(unit);

            if (buildQueue.Count == 1)
            {
                StartCoroutine(DoBuildUnits());
            }
            else
            {
                OnQueueUpdated?.Invoke(buildQueue.ToArray());
            }
        }

        private IEnumerator DoBuildUnits()
        {
            while (buildQueue.Count > 0)
            {
                BuildingUnit = buildQueue.Peek();
                CurrentQueueStartTime = Time.time;
                OnQueueUpdated?.Invoke(buildQueue.ToArray());
                yield return new WaitForSeconds(BuildingUnit.BuildTime);
                Instantiate(BuildingUnit.Prefab, transform.position, Quaternion.identity);
                buildQueue.Dequeue();
            }
            OnQueueUpdated.Invoke(buildQueue.ToArray());
        }
    }

}