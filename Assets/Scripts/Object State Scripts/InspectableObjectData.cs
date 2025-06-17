using UnityEngine;

namespace VARLab.PublicHealth
{
    /// <summary> This class will contain all of the state data in an inspectable object </summary>
    [CreateAssetMenu(fileName = "InspectableObjectData", menuName = "ScriptableObjects/InspectableObject")]
    public class InspectableObjectData : ScriptableObject
    {
        [Tooltip("Internal object temperature")] public float ProbeTemp;

        [Tooltip("Infrared object temperature")] public float IRTemp;

        [Tooltip("Is this state compliant")] public bool IsCompliant;

        [Tooltip("The correct tool to be used on the object")] public Tools Tools;
    }
}
