using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace VARLab.CloudSave
{
    /// <summary>Handles saving and loading data from Azure Blob Storage.</summary>
    public class AzureSaveSystem : MonoBehaviour, ICloudSaveSystem
    {

        public enum RequestAction
        {
            Save,
            Load,
            Delete
        }

        [Serializable]
        private struct SaveData
        {
            public string Content;
        }

        private struct PasswordPacket
        {
            public string Password;
        }

        public class RequestCompletedEventArgs : System.EventArgs
        {
            public RequestAction Action;
            public bool Success = false;
            public string Data = null;
            public bool IsAuthorized { get; private set; }

            public RequestCompletedEventArgs(bool isAuthorized)
            {
                IsAuthorized = isAuthorized;
            }

            public RequestCompletedEventArgs()
            {
            }
        }


        /// <summary> Event delegate which captures data from a completed request. </summary>
        public delegate void RequestCompletedEventHandler(object sender, RequestCompletedEventArgs args);


        /// <summary>Password used for JWT authentication with the API.</summary>
        private const string AUTHENTICATION_PASSWORD = "icecreamb23i4b2kh";

        private const string DEFAULT_API_ENDPOINT = "https://varlabcloudsave.azurewebsites.net/core/";


        /// <summary> Event fired when a web request is complete. </summary>
        public event RequestCompletedEventHandler RequestCompleted;

        /// <summary>API endpoint URI for the Azure Blob Storage instance.</summary>
        public string BlobStorageURI => blobStorageURI;

        public bool IsAuthorized { get; private set; } = false;


        [Tooltip("API endpoint URI for the Azure Blob Storage instance.")]
        [SerializeField] private string blobStorageURI = DEFAULT_API_ENDPOINT;

        /// <summary>Token used to authorize requests to the saveload API.</summary>
        private string authorizeToken;


        /// <summary>Gets the complete path on Azure for the given relative <paramref name="path"/>.</summary>
        private string GetFullFileURL(string path) => $"{blobStorageURI}{path}"; 

        public Coroutine Save(string path, string data)
        {
            var url = $"{GetFullFileURL(path)}/uploadcontent";

            var wrapper = new SaveData
            {
                Content = data
            };

            return StartCoroutine(SaveRequest(url, JsonUtility.ToJson(wrapper)));
        }

        public CoroutineWithData Load(string path)
        {
            var url = GetFullFileURL(path);

            return new CoroutineWithData(this, LoadRequest(url));
        }

        public Coroutine Delete(string path)
        {
            var url = GetFullFileURL(path);

            return StartCoroutine(DeleteRequest(url));
        }

        private IEnumerator SaveRequest(string uri, string data)
        {
            yield return AuthorizeRequest();

            using UnityWebRequest request = UnityWebRequest.Put(uri, data);

            request.method = UnityWebRequest.kHttpVerbPOST;
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Accept", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + authorizeToken);

            yield return request.SendWebRequest();

            var success = request.result == UnityWebRequest.Result.Success;

            if (!success)
            {
                Debug.LogError(request.error);
            }

            RequestCompleted?.Invoke(this, 
                new RequestCompletedEventArgs { Action = RequestAction.Save, Success = success, Data = data });
        }

        private IEnumerator LoadRequest(string uri)
        {
            yield return AuthorizeRequest();

            using UnityWebRequest webRequest = UnityWebRequest.Get(uri);

            webRequest.SetRequestHeader("Authorization", "Bearer " + authorizeToken);

            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            var success = webRequest.result == UnityWebRequest.Result.Success;

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                    yield return null;
                    break;

                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                    yield return null;
                    break;

                case UnityWebRequest.Result.Success:
                    //Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text); //Left for future debugging purposes if needed.
                    yield return webRequest.downloadHandler.text;
                    break;
            }

            RequestCompleted?.Invoke(this, 
                new RequestCompletedEventArgs { Action = RequestAction.Load, Success = success, Data = webRequest.downloadHandler.text });
        }

        /// <summary> This IEnumerator sends a Delete call to the API </summary>
        private IEnumerator DeleteRequest(string url)
        {
            yield return AuthorizeRequest();

            using UnityWebRequest webRequest = UnityWebRequest.Delete(url);

            webRequest.method = UnityWebRequest.kHttpVerbDELETE;
            webRequest.SetRequestHeader("Authorization", "Bearer " + authorizeToken);

            yield return webRequest.SendWebRequest();

            var success = webRequest.result == UnityWebRequest.Result.Success;

            if (!success)
            {
                Debug.LogError(webRequest.error);
            }

            RequestCompleted?.Invoke(this, 
                new RequestCompletedEventArgs { Action = RequestAction.Delete, Success = success });
        }

        public IEnumerator AuthorizeRequest()
        {
            var passwordData = new PasswordPacket()
            {
                Password = AUTHENTICATION_PASSWORD
            };

            string jsonPassword = JsonUtility.ToJson(passwordData);

            using UnityWebRequest request = UnityWebRequest.Put(blobStorageURI + "authenticate", jsonPassword);
            request.method = UnityWebRequest.kHttpVerbPOST;
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Accept", "application/json");
            
            var returnreq = request.SendWebRequest();
            Debug.Log("Sending authorization request...: " + returnreq);
            yield return returnreq;

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(request.error);
                IsAuthorized = false;
            }
            else
            {
                authorizeToken = request.downloadHandler.text.Trim('"');
                IsAuthorized = true;
            }

            RequestCompleted?.Invoke(this, new RequestCompletedEventArgs(IsAuthorized));
        }
    }
}
