using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;
using RTS.Utilities;

namespace RTS.Behavior
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "TranslatePosition", story: "[Self] moves to [TargetLocation] at [Speed] speed", category: "Action", id: "fa3794fd517fdbad8edd19fa0b3b0547")]
    public partial class TranslatePositionAction : Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> Self;
        [SerializeReference] public BlackboardVariable<Vector3> TargetLocation;
        [SerializeReference] public BlackboardVariable<float> Speed;
        private Animator animator;
        private NavMeshAgent navMeshAgent;
        private float endTime;
        private Vector3 direction;
        private Transform selfTransform;

        protected override Status OnStart()
        {
            if (Self.Value == null) return Status.Failure;
            animator = Self.Value.GetComponent<Animator>();

            if (Self.Value.TryGetComponent(out navMeshAgent))
            {
                navMeshAgent.enabled = false;
            }

            selfTransform = Self.Value.transform;
            float distance = Vector3.Distance(selfTransform.position, TargetLocation.Value);
            endTime = Time.time + (distance / Speed);
            direction = (TargetLocation.Value - selfTransform.position).normalized;

            selfTransform.forward = direction;
            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            if (Time.time > endTime)
                return Status.Success;

            animator?.SetFloat(AnimationConstants.SPEED, Speed);


            selfTransform.position += Speed * Time.deltaTime * direction;

            return Status.Running;
        }

        protected override void OnEnd()
        {
            animator?.SetFloat(AnimationConstants.SPEED, 0);
            if (navMeshAgent != null)
            {
                navMeshAgent.enabled = true;
            }
        }
    }
}