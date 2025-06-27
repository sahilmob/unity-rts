using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

namespace RTS.Behavior
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Set Agent Obstacle Avoidance", story: "Sets [Agent] ObstacleAvoidanceQuality", category: "Action/Navigation", id: "240073ab5fff606e7572f7df669d977e")]
    public partial class SetAgentObstacleAvoidanceAction : Action
    {

        [SerializeReference] public BlackboardVariable<GameObject> Agent;
        [SerializeReference] public BlackboardVariable<int> AvoidanceQuality;
        protected override Status OnStart()
        {
            if (!Agent.Value.TryGetComponent(out NavMeshAgent agent) || AvoidanceQuality > 4 || AvoidanceQuality < 0)
            {
                return Status.Failure;
            }

            agent.obstacleAvoidanceType = (ObstacleAvoidanceType)AvoidanceQuality.Value;
            return Status.Success;
        }
    }
}