using RTS.Units;
using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

namespace RTS.Behavior
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Build Building", story: "[Self] builds [BuildingSO] at [TargetLocation]", category: "Action", id: "990fcff5d3da9c632de2f49daa9581e2")]
    public partial class BuildBuildingAction : Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> Self;
        [SerializeReference] public BlackboardVariable<BuildingSO> BuildingSO;
        [SerializeReference] public BlackboardVariable<Vector3> TargetLocation;
        [SerializeReference] public BlackboardVariable<BaseBuilding> BuildingUnderConstruction;
        private float startTime;
        private BaseBuilding completeBuilding;
        private Vector3 startPosition;
        private Vector3 endPosition;
        private Renderer buildingRenderer;
        private float targetHealth;

        protected override Status OnStart()
        {
            if (!HasValidInputs())
            {
                return Status.Failure;
            }
            startTime = Time.time;
            GameObject building = GameObject.Instantiate(BuildingSO.Value.Prefab, TargetLocation, Quaternion.identity);

            if (!building.TryGetComponent(out completeBuilding) || completeBuilding.MainRenderer == null) return Status.Failure;

            buildingRenderer = completeBuilding.MainRenderer;

            BuildingUnderConstruction.Value = completeBuilding;

            startPosition = TargetLocation.Value - Vector3.up * buildingRenderer.bounds.size.y;
            endPosition = TargetLocation.Value;
            buildingRenderer.transform.position = startPosition;
            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            float normalizedTime = (Time.time - startTime) / BuildingSO.Value.BuildTime;

            buildingRenderer.transform.position = Vector3.Lerp(startPosition, endPosition, normalizedTime);

            targetHealth += Time.deltaTime * (BuildingSO.Value.Health / BuildingSO.Value.BuildTime);

            if (targetHealth >= 1)
            {
                int healAmount = Mathf.FloorToInt(targetHealth);
                completeBuilding.Heal(healAmount);
                targetHealth -= healAmount;
            }

            return normalizedTime >= 1 ? Status.Success : Status.Running;
        }

        protected override void OnEnd()
        {
            if (CurrentStatus == Status.Success)
                completeBuilding.enabled = true;
        }

        private bool HasValidInputs()
        {
            if (BuildingSO.Value == null || TargetLocation.Value == null || Self.Value == null)
            {
                return false;
            }
            return true;
        }
    }
}