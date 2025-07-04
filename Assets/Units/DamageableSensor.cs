using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DamageableSensor : MonoBehaviour
{
    private HashSet<IDamageable> damageables = new();
    public List<IDamageable> Damageables => damageables.ToList();
    public delegate void UnitDetectionEvent(IDamageable damageable);
    public event UnitDetectionEvent OnUnitEnter;
    public event UnitDetectionEvent OnUnitExit;
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out IDamageable damageable))
        {
            damageables.Add(damageable);
            OnUnitEnter.Invoke(damageable);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out IDamageable damageable))
        {
            damageables.Remove(damageable);
            OnUnitExit.Invoke(damageable);
        }
    }
}
