
using UnityEngine;

namespace RTS.Units
{
    public interface IBuildingBuilder
    {
        public bool IsBuilding { get; }
        public GameObject Build(BuildingSO buildingSO, Vector3 targetLocation);
        public void ResumeBuilding(BaseBuilding building);
        public void CancelBuilding();
    }
}