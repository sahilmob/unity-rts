

using RTS.EventBus;
using RTS.Events;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;

namespace RTS.Units
{
    [RequireComponent(typeof(NavMeshAgent), typeof(BehaviorGraphAgent))]
    public abstract class AbstractUnit : AbstractCommandable, IMovable
    {
        public float AgentRadius => agent.radius;
        private NavMeshAgent agent;
        protected BehaviorGraphAgent graphAgent;

        public void MoveTo(Vector3 position)
        {
            SetCommandOverrides(null);
            graphAgent.SetVariableValue("TargetLocation", position);
            graphAgent.SetVariableValue("Command", UnitCommand.Move);
        }

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            graphAgent = GetComponent<BehaviorGraphAgent>();
            graphAgent.SetVariableValue("Command", UnitCommand.Stop);
        }

        protected override void Start()
        {
            base.Start();
            CurrentHealth = UnitSO.Health;
            MaxHealth = UnitSO.Health;
            Bus<UnitSpawnEvent>.Raise(new UnitSpawnEvent(this));
        }

        public void Stop()
        {
            SetCommandOverrides(null);
            graphAgent.SetVariableValue("Command", UnitCommand.Stop);
        }

        private void OnDestroy()
        {
            Bus<UnitDeathEvent>.Raise(new(this));
        }
    }
}