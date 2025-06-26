using System;
using System.Collections;
using RTS.UI.Components;
using RTS.Units;
using UnityEngine;

namespace RTS.UI.Container
{
    public class BuildingBuildingUI : MonoBehaviour, IUIElement<BaseBuilding>
    {
        [SerializeField] private ProgressBar progressBar;
        private BaseBuilding building;
        private Coroutine updateUnitProgressCo;
        public void Disable()
        {
            if (building != null)
            {
                building.OnQueueUpdated -= HandleQueueUpdated;

            }
            gameObject.SetActive(false);
            building = null;
            updateUnitProgressCo = null;
        }

        public void EnableFor(BaseBuilding building)
        {
            gameObject.SetActive(true);
            this.building = building;
            building.OnQueueUpdated += HandleQueueUpdated;
            updateUnitProgressCo = StartCoroutine(UpdateUnitProgress());
        }

        private void HandleQueueUpdated(UnitSO[] unitsInQueue)
        {
            if (updateUnitProgressCo == null && unitsInQueue.Length == 1)
            {
                updateUnitProgressCo = StartCoroutine(UpdateUnitProgress());
            }
        }

        private IEnumerator UpdateUnitProgress()
        {
            while (building != null && building.QueueSize > 0)
            {
                float startTime = building.CurrentQueueStartTime;
                float endTime = startTime + building.BuildingUnit.BuildTime;
                float progress = Mathf.Clamp01((Time.time - startTime) / (endTime - startTime));
                progressBar.SetProgress(progress);
                yield return null;
            }
        }
    }
}