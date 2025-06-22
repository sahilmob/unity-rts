

using RTS.EventBus;
using RTS.Events;
using UnityEngine;
using UnityEngine.AI;

namespace RTS.Units
{
    [RequireComponent(typeof(NavMeshAgent))]
    public abstract class AbstractUnit : AbstractCommandable, IMovable
    {
        public float AgentRadius => agent.radius;
        private NavMeshAgent agent;

        public void MoveTo(Vector3 position)
        {
            agent.SetDestination(position);
        }

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
        }

        private void Start()
        {
            Bus<UnitSpawnEvent>.Raise(new UnitSpawnEvent(this));
        }
    }
}