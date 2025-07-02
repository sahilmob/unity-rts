using System;
using System.Collections;
using RTS.UI;
using RTS.UI.Components;
using RTS.Units;
using TMPro;
using UnityEngine;

public class BuildingUnderConstructionUI : MonoBehaviour, IUIElement<BaseBuilding>
{
    [SerializeField] private TextMeshProUGUI unitName;
    [SerializeField] private ProgressBar progressBar;

    public void Disable()
    {
        gameObject.SetActive(false);
    }

    public void EnableFor(BaseBuilding building)
    {
        gameObject.SetActive(true);
        unitName.SetText(building.UnitSO.DisplayName);
        StartCoroutine(AnimateBuildingProgress(building));
    }



    private IEnumerator AnimateBuildingProgress(BaseBuilding building)
    {
        progressBar.SetProgress(0);
        while (enabled && building.Progress.Progress < 1)
        {
            if (building.Progress.State != BuildingProgress.BuildingState.Building)
            {
                yield return null;
                continue;
            }
            float startTime = building.Progress.StartTime;
            float endTime = startTime + building.BuildingSO.BuildTime;
            float progress = (Time.time - startTime) / (endTime - startTime);
            progressBar.SetProgress(Mathf.Clamp01(progress));
            yield return null;
        }
    }
}