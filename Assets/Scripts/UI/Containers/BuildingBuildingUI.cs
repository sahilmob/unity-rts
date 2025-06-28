using System;
using System.Collections;
using RTS.UI.Components;
using RTS.Units;
using UnityEngine;

namespace RTS.UI.Container
{
    public class BuildingBuildingUI : MonoBehaviour, IUIElement<BaseBuilding>
    {
        [SerializeField] private UIBuildQueueButton[] unitButtons;
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
            progressBar.SetProgress(0);
            gameObject.SetActive(true);
            this.building = building;
            building.OnQueueUpdated += HandleQueueUpdated;

            SetupUnitButtons();

            updateUnitProgressCo = StartCoroutine(UpdateUnitProgress());
        }

        private void SetupUnitButtons()
        {
            int i = 0;

            for (; i < building.QueueSize; i++)
            {
                int index = i;
                unitButtons[i].EnableFor(building.Queue[i], () => building.CancelBuildingUnit(index));
            }

            for (; i < unitButtons.Length; i++)
            {
                unitButtons[i].Disable();
            }
        }

        private void HandleQueueUpdated(AbstractUnitSO[] unitsInQueue)
        {
            if (updateUnitProgressCo == null && unitsInQueue.Length == 1)
            {
                updateUnitProgressCo = StartCoroutine(UpdateUnitProgress());
            }

            SetupUnitButtons();
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

            updateUnitProgressCo = null;
        }
    }
}