using UnityEngine;

namespace VARLab.PublicHealth
{
    public class LayerManager : MonoBehaviour
    {
        [SerializeField] private LayerMask _defaultLayers;
        [SerializeField] private LayerMask _kitchenLayers;
        [SerializeField] private LayerMask _diningLayers;

        private LayerMask _selectedMask;

        /// <summary>
        /// Selects which layers are visible from a certain poi.
        /// </summary>
        /// <param name="poi">point of interest selected</param>
        public void SetLayers(POI poi)
        {
            if (TargetDoorAnimBehaviour.IsDoorOpen)
            {
                _selectedMask = _defaultLayers;
            }
            else
            {
                switch (poi.POIName)
                {
                    case "Kitchen":
                        _selectedMask = _kitchenLayers;
                        break;
                    case "Dining":
                        _selectedMask = _diningLayers;
                        break;
                    default:
                        _selectedMask = _defaultLayers;
                        break;
                }
            }

            CinemachineExtensionLayerMask.Instance.DefaultLayers = _selectedMask;
        }
    }
}
