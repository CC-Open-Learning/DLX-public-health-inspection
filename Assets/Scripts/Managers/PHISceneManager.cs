using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VARLab.PublicHealth
{
    public class PHISceneManager : MonoBehaviour
    {
        public static PHISceneManager Instance;

        [SerializeField, Tooltip("Reference to the Camera Controller")] private CameraController _cameraController;
        [SerializeField, Tooltip("Reference to the POI Manager")] public POIManager PoiManager;
        public static bool LoadCompleted = false;

        private void Start()
        {
            if (Instance == null)
            {
                if (Instance == null)
                {
                    Instance = this;
                }
            }
        }

        public void RestartScene()
        {
            StartCoroutine(RestartSceneCoroutine());
        }

        /// <summary>
        /// This is where we should add any static variable that needs to be reset when the scene is restarted.
        /// </summary>
        private IEnumerator RestartSceneCoroutine()
        {
            PlayerController.IntroCompleted = false;
            PlayerController.HasWashedHands = false;
            PlayerController.VisitedOffice = false;
            PauseBehaviour.IsPaused = false;
            SaveDataSupport.Restarted = true;
            yield return DeleteCloudSave();
            LoadCompleted = false;
            var loading = SceneManager.LoadSceneAsync("Main Scene");
            while (!loading.isDone) yield return null;
            LoadCompleted = true;
        }

        private IEnumerator DeleteCloudSave()
        {
            yield return SaveDataSupport.Singleton.CloudSave._Delete();
        }
    }
}
