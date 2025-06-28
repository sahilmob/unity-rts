
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;

namespace RTS.Units
{
    public class BaseBuilding : AbstractCommandable
    {
        private const int MAX_QUEUE_SIZE = 5;
        public AbstractUnitSO[] Queue => buildQueue.ToArray();
        [field: SerializeField] public float CurrentQueueStartTime { get; private set; }
        [field: SerializeField] public AbstractUnitSO BuildingUnit { get; private set; }
        private List<AbstractUnitSO> buildQueue = new(MAX_QUEUE_SIZE);
        public int QueueSize => buildQueue.Count;
        public delegate void QueueUpdatedEvent(AbstractUnitSO[] unitsInQueue);
        public event QueueUpdatedEvent OnQueueUpdated;
        public void BuildUnit(AbstractUnitSO unit)
        {
            if (buildQueue.Count >= MAX_QUEUE_SIZE) return;

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
    }

}