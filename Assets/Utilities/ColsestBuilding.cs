

using System.Collections.Generic;
using RTS.Units;
using UnityEngine;

public struct ClosestBuildingComparer : IComparer<BaseBuilding>
{
    private Vector3 target;
    public ClosestBuildingComparer(Vector3 target)
    {
        this.target = target;
    }
    public int Compare(BaseBuilding x, BaseBuilding y)
    {
        return (x.transform.position - target).sqrMagnitude.CompareTo((y.transform.position - target).sqrMagnitude);
    }
}