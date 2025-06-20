using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.Universal;

namespace RTS.Units
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class Worker : MonoBehaviour, ISelectable
    {
        [SerializeField] private Transform target;
        [SerializeField] private DecalProjector decalProjector;
        private NavMeshAgent agent;
        public void Deselect()
        {
            decalProjector?.gameObject.SetActive(false);
        }

        public void Select()
        {
            decalProjector?.gameObject.SetActive(true);
        }

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
        }

        // Update is called once per frame
        private void Update()
        {
            if (target != null)
            {
                agent.SetDestination(target.position);
            }
        }
    }
}