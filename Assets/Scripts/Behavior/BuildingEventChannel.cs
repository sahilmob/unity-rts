using RTS.Units;
using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Behavior/Event Channels/Building Event Channel")]
#endif
[Serializable, GeneratePropertyBag]
[EventChannelDescription(name: "Building Event Channel", message: "[Self] [BuildingEventType] on [BaseBuilding]", category: "Events", id: "f22ba00b4381ae7ce2c1443faccc8678")]
public sealed partial class BuildingEventChannel : EventChannel<GameObject, BuildingEventType, BaseBuilding> { }

