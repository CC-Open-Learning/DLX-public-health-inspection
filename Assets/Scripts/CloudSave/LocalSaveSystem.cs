using System.Collections;
using System.IO;
using UnityEngine;
using VARLab.CloudSave;

namespace VARLab.PublicHealth
{
    public class LocalSaveSystem : MonoBehaviour, ICloudSaveSystem
    {
        // This Event system exists in the AzureSaveSystem in the packages, I needed to replicate it here
        // Note that the only difference is that it does not have the IsAuthorized property because it is 
        // not relevant for local saves, there is no authentication
        public class RequestCompletedEventArgs : System.EventArgs
        {
            public enum RequestAction { Save, Load, Delete }
            public RequestAction Action;
            public bool Success = false;
            public string Data = null;
        }

        public delegate void RequestCompletedEventHandler(object sender, RequestCompletedEventArgs args);
        public event RequestCompletedEventHandler RequestCompleted;

        public Coroutine Save(string path, string data)
        {
            return StartCoroutine(SaveRoutine(path, data));
        }

        private IEnumerator SaveRoutine(string path, string data)
        {
            yield return null; // Wait one frame to simulate async operation
            
            try
            {
                string fullPath = Path.Combine(Application.persistentDataPath, path);
                
                // Create directory if it doesn't exist
                string directory = Path.GetDirectoryName(fullPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                File.WriteAllText(fullPath, data);
                
                // Fire success event
                RequestCompleted?.Invoke(this, new RequestCompletedEventArgs 
                { 
                    Action = RequestCompletedEventArgs.RequestAction.Save, 
                    Success = true, 
                    Data = data 
                });
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[LocalSaveSystem] Save failed: {ex.Message}");
                
                // Fire failure event
                RequestCompleted?.Invoke(this, new RequestCompletedEventArgs 
                { 
                    Action = RequestCompletedEventArgs.RequestAction.Save, 
                    Success = false, 
                    Data = data 
                });
            }
        }

        public CoroutineWithData Load(string path)
        {
            return new CoroutineWithData(this, LocalLoadRoutine(path));
        }

        private IEnumerator LocalLoadRoutine(string path)
        {
            yield return null; // Wait one frame to simulate async operation
            
            string data = null;
            bool success = false;
            
            try
            {
                string fullPath = Path.Combine(Application.persistentDataPath, path);
                data = File.Exists(fullPath) ? File.ReadAllText(fullPath) : null;
                success = data != null;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[LocalSaveSystem] Load failed: {ex.Message}");
                success = false;
                data = null;
            }
            
            // Fire event
            RequestCompleted?.Invoke(this, new RequestCompletedEventArgs 
            { 
                Action = RequestCompletedEventArgs.RequestAction.Load, 
                Success = success, 
                Data = data 
            });
            
            yield return data;
        }

        public Coroutine Delete(string path)
        {
            return StartCoroutine(DeleteRoutine(path));
        }

        private IEnumerator DeleteRoutine(string path)
        {
            yield return null; // Wait one frame
            
            bool success = false;
            
            try
            {
                string fullPath = Path.Combine(Application.persistentDataPath, path);
                if (File.Exists(fullPath))
                    File.Delete(fullPath);
                
                success = true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[LocalSaveSystem] Delete failed: {ex.Message}");
                success = false;
            }
            
            // Fire event
            RequestCompleted?.Invoke(this, new RequestCompletedEventArgs 
            { 
                Action = RequestCompletedEventArgs.RequestAction.Delete, 
                Success = success 
            });
        }
    }
}
