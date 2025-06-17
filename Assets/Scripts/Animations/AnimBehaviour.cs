using UnityEngine;

namespace VARLab.PublicHealth
{
    /// <summary>
    ///     This class holds the animator and the hash's for the trigger and bool parameters
    /// </summary>
    [System.Serializable]
    public class AnimBehaviour : ISerializationCallbackReceiver
    {
        [Tooltip("Animator controller for the behaviour")] public Animator Animator;

        [SerializeField] private string _triggerParamString;

        [SerializeField] private string _boolParamString;

        private int _triggerParamHash;

        private int _boolParamHash;

        public string TriggerParamString
        {
            get => _triggerParamString;
            set => _triggerParamString = value;
        }

        public int TriggerParamHash => _triggerParamHash;
        public int BoolParamHash => _boolParamHash;

        /// <summary>
        ///     Empty but included because of the interface
        /// </summary>
        public void OnBeforeSerialize() { }


        /// <summary>
        ///     Called when Action and State Parameter changes in the inspector. 
        ///     Used only for Editor changes.
        /// </summary>
        public void OnAfterDeserialize()
        {
            _triggerParamHash = Animator.StringToHash(_triggerParamString);
            _boolParamHash = Animator.StringToHash(_boolParamString);
        }
    }
}
