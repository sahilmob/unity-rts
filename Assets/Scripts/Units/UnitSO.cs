
using UnityEngine;

namespace RTS.Units
{

    [CreateAssetMenu(fileName = "Unit", menuName = "Units/Unit")]
    public class UnitSO : ScriptableObject
    {
        [field: SerializeField] public int health { get; private set; } = 100;
        [field: SerializeField] public GameObject Prefab { get; private set; }
        [field: SerializeField] public float BuildTime { get; private set; } = 5;
        [field: SerializeField] public Sprite Icon { get; private set; }
    }
}