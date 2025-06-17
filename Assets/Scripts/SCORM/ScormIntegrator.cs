using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using VARLab.Analytics;
using VARLab.LTIPackage;
using VARLab.SCORM;

namespace VARLab.PublicHealth
{
    public delegate void OnInitializeEvent();
    public class ScormIntegrator : MonoBehaviour
    {
        public static ScormIntegrator Instance;
        public ScormManager ScormMnger;

        public UnityEvent OnInitialize;
        public static string LearnerId { get; private set; }
        public static string LearnerName { get; private set; }
        public static bool Initialized { get; private set; } = false;

        private void Start()
        {

            if (ScormManager.IsDeployedWebGL && LearnerInfo.UserID == null)
            {
                ScormManager.Initialize();
                StartCoroutine(Startup());
            }
            else if (ScormManager.IsDeployedWebGL)
            {
                LearnerId = LearnerInfo.UserID;
                OnInitialize?.Invoke();
                CoreAnalytics.LoginUser(LearnerId);
                CoreAnalytics.VersionCheck(ProcessDlxVersionResult);
            }
#if UNITY_EDITOR
            CoreAnalytics.LoginUser(LearnerId);
            CoreAnalytics.VersionCheck(ProcessDlxVersionResult);
#endif
        }

        private void Awake()
        {
            //Since this sim is using LTI disable auto initialization of scorm
            ScormMnger.InitializeOnStart = false;
            Instance = this;
            LearnerId = SystemInfo.deviceUniqueIdentifier;
            CoreAnalytics.Initialize();
        }

        public IEnumerator Startup()
        {
            yield return new WaitUntil(() => ScormManager.Initialized);
            yield return new WaitForSeconds(2); // Race condition, should be a callback or a timeout-based wait until.
            InitializeData();
        }

        private void InitializeData()
        {
            // Must be set to incomplete at the start so that the LMS does not set to complete as soom as the student opens the Module
            ScormManager.SetCompletionStatus(StudentRecord.CompletionStatusType.incomplete);
            LearnerId = ScormManager.GetLearnerId();
            LearnerName = ScormManager.GetLearnerName(); // This informations is not saved and will only be used to generate the PDF
            CoreAnalytics.LoginUser(LearnerId);
            Initialized = true;
            OnInitialize?.Invoke();
        }

        public static void Completed()
        {
            if (Instance != null)
                Instance.SetCompletion();
        }

        private void SetCompletion()
        {
            ScormManager.SetCompletionStatus(StudentRecord.CompletionStatusType.completed);
        }

        public static bool IsCompleted()
        {
            if (Instance == null)
                return false;

            return ScormManager.GetCompletionStatus() == StudentRecord.CompletionStatusType.completed;
        }

        private void ProcessDlxVersionResult(VersionResult versionResult)
        {
            //Debug.Log("Version Check Callback");
            //Debug.Log("Is a Correct Version " + versionResult.IsVersionCorrect);
            //Debug.Log("Current Version " + versionResult.CurentVersionNumber);
            //Debug.Log("New Version " + versionResult.NewVersionNumber);
        }
    }
}
