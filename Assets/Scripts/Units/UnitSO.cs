
using UnityEngine;

namespace RTS.Units
{

    [CreateAssetMenu(fileName = "Unit", menuName = "Units/Unit")]
    public class UnitSO : ScriptableObject
    {
        [field: SerializeField] public int health { get; private set; } = 100;
    }
}