using Cinemachine;
using UnityEngine;

namespace VARLab.PublicHealth
{
    public class CinemachineExtensionLayerMask : CinemachineExtension
    {
        [Tooltip("Reference to the layers")] public LayerMask DefaultLayers;

        public static CinemachineExtensionLayerMask Instance;

        private void Start()
        {
            Instance = this;
        }
        protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
        {
            Camera.main.cullingMask = DefaultLayers;
        }
    }
}
