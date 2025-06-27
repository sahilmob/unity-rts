using RTS.Environment;
using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

namespace RTS.Behavior
{
#if UNITY_EDITOR
    [CreateAssetMenu(menuName = "Behavior/Event Channels/GatherSuppliesEventChannel")]
#endif
    [Serializable, GeneratePropertyBag]
    [EventChannelDescription(name: "GatherSuppliesEventChannel", message: "[Self] gathers [amount] [supplies]", category: "Events", id: "e3e3cf14298938070553c93dfac027d4")]
    public sealed partial class GatherSuppliesEventChannel : EventChannel<GameObject, int, SupplySO> { }
}