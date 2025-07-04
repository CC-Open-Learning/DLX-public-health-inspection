using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VARLab.CloudSave;
using VARLab.PublicHealth;

namespace VARLab.PublicHealth
{
    /// <summary>
    /// This is the CloudSave class, its just is to serialize send data to the API, This class should not be called directly but rather
    /// through SaveDataSupport which acts as the communication manager between the SaveData Object, PHI Scene, and the CloudSave system.
    /// </summary>
    public class CloudSaving : MonoBehaviour, ICloudSaving
    {

        [SerializeField] public ICloudSaveSystem _saveSystem;
        [SerializeField] private AzureSaveSystem _azureSaveSystem;
        [SerializeField] private LocalSaveSystem _localSaveSystem;

        private string _fileName;

        private string _filePath;

        public bool HasLoaded { get; set; }
        public bool? LoadSuccess { get; set; }
        public string LoadData { get; set; }

        private ICloudSerializer _serializer;

        private CoroutineWithData _load;

#pragma warning disable IDE0044 // Add readonly modifier
        private Queue<Action> _saveQueue = new();
#pragma warning restore IDE0044 // Add readonly modifier

        private bool _saveFlag = false;

        private bool _requestDone = false;

        public bool IsInitialized { get; private set; } = false;

        public void Start()
        {
            //start save background loop in start
            StartCoroutine(SaveLoop());
        }

        /// <summary>
        /// This is the event listener matching the delegate in both Azure and Local systems
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void RequestCompletedEventHandler(object sender, object args)
        {
            //this is the listener currently has no need of the sent information just sets a flag saying action is done
            _requestDone = true;
        }

        /// <summary>
        /// save now adds to queue instead of triggering save directly
        /// </summary>
        public void Save()
        {
            _saveQueue.Enqueue(SaveAction);
            _saveFlag = true;
        }

        /// <summary>
        /// this is the actual save action that is queued and calls the azure system
        /// </summary>
        public void SaveAction()
        {
            var data = _serializer.Serialize();
            _saveSystem.Save(_filePath, data);
        }

        /// <summary>
        /// This is the coroutine that runs in the background looping handling actions if they are queued otherwise just does nothing.
        /// </summary>
        /// <returns> IEnumerator to yield return and await </returns>
        public IEnumerator SaveLoop()
        {
            while (true)
            {
                if (_saveFlag)
                {
                    Action currentAction = _saveQueue.Dequeue();
                    currentAction?.Invoke();
                    yield return new WaitUntil(() => _requestDone == true);

                    _requestDone = false;
                }

                if (_saveQueue.Count == 0)
                {
                    _saveFlag = false;
                }

                yield return null;
            }
        }

        /// <summary>
        ///     Load the data from the cloud
        /// </summary>
        public void Load()
        {
            //idk if this is needed ,might be worth to remove later on if it causes issues, this is to ensure
            //that the system is initialized before trying to load
            if (!IsInitialized)
            {
                Debug.LogWarning("[CloudSaving] Tried to Load before initialization was complete.");
                return;
            }
            StartCoroutine(_Load());
        }

        private IEnumerator _Load()
        {
            if (_serializer == null)
            {
                Debug.LogError("[CloudSaving] Serializer is not initialized. Did you call Initialize() and wait for it to complete before calling Load()?");
                yield break;
            }
            _load = _saveSystem.Load(_filePath);

            yield return _load.Routine;

            var loadedData = (string)_load.Result;

            if (loadedData != null)
            {
                LoadSuccess = true;
                LoadData = loadedData;
                _serializer.Deserialize(loadedData);
            }
            else
            {
                LoadSuccess = false;
            }
            HasLoaded = true;
        }

        public void Delete()
        {
            StartCoroutine(_Delete());
        }

        public IEnumerator _Delete()
        {
            var delete = _saveSystem.Delete(_filePath);

            yield return delete;
        }

        /// <summary>
        /// Need to be called after start to avoid Race condition
        /// </summary>
        public void Initialize()
        {
            StartCoroutine(SelectSaveSystem());
        }

        private IEnumerator SelectSaveSystem()
        {
            Debug.Log("[CloudSaving] Starting SelectSaveSystem...");
            
#if UNITY_WEBGL && !UNITY_EDITOR
            _fileName = ScormIntegrator.LearnerId;
#else
            _fileName = SystemInfo.deviceUniqueIdentifier;
#endif
            _filePath = "publichealthinspection/" + _fileName + ".txt";
            
            Debug.Log($"[CloudSaving] File path: {_filePath}");

            bool useAzure = false;

            if (_azureSaveSystem != null)
            {
                Debug.Log("[CloudSaving] Attempting Azure authorization...");
                yield return StartCoroutine(_azureSaveSystem.AuthorizeRequest());
                useAzure = _azureSaveSystem.IsAuthorized;
            }

            if (useAzure)
            {
                SetupAzureSystem();
            }
            else
            {
                SetupLocalSystem(_azureSaveSystem == null ? "Azure not assigned" : "Azure authorization failed");
            }

            // Complete initialization
            HasLoaded = false;
            _serializer = new JsonCloudSerializer();
            LoadSuccess = null;
            IsInitialized = true;
            Debug.Log("[CloudSaving] Initialization complete!");
        }

        private void SetupAzureSystem()
        {
            _saveSystem = _azureSaveSystem;
            Debug.Log("[CloudSaving] Using Azure Save System");
            _azureSaveSystem.RequestCompleted += (sender, args) => RequestCompletedEventHandler(sender, args);
        }

        private void SetupLocalSystem(string reason)
        {
            _saveSystem = _localSaveSystem;
            Debug.LogWarning($"[CloudSaving] Using Local Save System ({reason})");
            _localSaveSystem.RequestCompleted += (sender, args) => RequestCompletedEventHandler(sender, args);
        }
    }
}
