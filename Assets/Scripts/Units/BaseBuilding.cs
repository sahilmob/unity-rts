
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RTS.Units
{
    public class BaseBuilding : AbstractCommandable
    {
        private const int MAX_QUEUE_SIZE = 5;
        private Queue<UnitSO> buildQueue = new(MAX_QUEUE_SIZE);
        public void BuildUnit(UnitSO unit)
        {
            if (buildQueue.Count >= MAX_QUEUE_SIZE) return;

            buildQueue.Enqueue(unit);

            if (buildQueue.Count == 1)
            {
                StartCoroutine(DoBuildUnits());
            }
        }

        private IEnumerator DoBuildUnits()
        {
            while (buildQueue.Count > 0)
            {
                UnitSO unit = buildQueue.Peek();
                yield return new WaitForSeconds(unit.BuildTime);
                Instantiate(unit.Prefab, transform.position, Quaternion.identity);
                buildQueue.Dequeue();
            }
        }
    }

}