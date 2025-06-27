using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using RTS.Units;
using System.Collections.Generic;

namespace RTS.Behavior
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Find Closest Command Post", story: "[Unit] finds nearest [CommandPost]", category: "Action/Units", id: "b2f119869945a9321b4c2a2b964d0d3c")]
    public partial class FindClosestCommandPostAction : Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> Unit;
        [SerializeReference] public BlackboardVariable<GameObject> CommandPost;
        [SerializeReference] public BlackboardVariable<float> SearchRadius = new(10);
        [SerializeReference] public BlackboardVariable<UnitSO> CommandPostBuilding;


        protected override Status OnStart()
        {
            Collider[] colliders = Physics.OverlapSphere(Unit.Value.transform.position, SearchRadius, LayerMask.GetMask("Buildings"));

            List<BaseBuilding> nearbyCommandPosts = new();

            foreach (Collider c in colliders)
            {
                if (c.TryGetComponent(out BaseBuilding b) && b.UnitSO.Equals(CommandPostBuilding.Value))
                {
                    nearbyCommandPosts.Add(b);
                }
            }

            if (nearbyCommandPosts.Count == 0)
            {
                return Status.Failure;
            }

            CommandPost.Value = nearbyCommandPosts[0].gameObject;

            return Status.Success;
        }

    }
}