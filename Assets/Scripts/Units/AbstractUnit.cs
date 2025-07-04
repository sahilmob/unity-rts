

using System;
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
        [SerializeField] private DamageableSensor damageableSensor;
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
            if (damageableSensor != null)
            {
                damageableSensor.OnUnitEnter += HandleUnitEnter;
                damageableSensor.OnUnitExit += HandleUnitExit;
            }
        }

        private void HandleUnitExit(IDamageable damageable)
        {
            Debug.Log($"Detected unit enter{damageableSensor.Damageables.Count}");
        }

        private void HandleUnitEnter(IDamageable damageable)
        {
            Debug.Log($"Detected unit exit {damageableSensor.Damageables.Count}");
        }

        public void Stop()
        {
            SetCommandOverrides(null);
            graphAgent.SetVariableValue("Command", UnitCommand.Stop);
        }

        private void OnDestroy()
        {
            Bus<UnitDeathEvent>.Raise(new(this));
            if (damageableSensor != null)
            {
                damageableSensor.OnUnitEnter -= HandleUnitEnter;
                damageableSensor.OnUnitExit -= HandleUnitExit;
            }
        }
    }
}