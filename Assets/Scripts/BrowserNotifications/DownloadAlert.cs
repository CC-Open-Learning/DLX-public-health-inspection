using System.Collections;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

namespace VARLab.PublicHealth
{
    public class DownloadAlert : MonoBehaviour
    {
        [DllImport("__Internal")]
        private static extern void Alert(string str);
        private bool _downloadReady = false;
        private bool _mouseCheckRunning = false;
        public void OnLeaveUnityScreen()
        {
            if (!_mouseCheckRunning)
            {
                _mouseCheckRunning = true;
                StartCoroutine(MouseContained());
            }
        }
        private IEnumerator MouseContained()
        {
#if UNITY_EDITOR
            yield return new WaitUntil(() => (Input.mousePosition.x <= 0 || Input.mousePosition.y <= 0 || Input.mousePosition.x >= Handles.GetMainGameViewSize().x - 1 || Input.mousePosition.y >= Handles.GetMainGameViewSize().y - 1) == true);
            if (!_downloadReady)
            {
                _mouseCheckRunning = false;
                yield break;
            }
            Debug.Log("Mouse Left Screen Area!");
            DownloadTriggered();
            _mouseCheckRunning = false;
#else
            yield return new WaitUntil(() => (Input.mousePosition.x <= 0 || Input.mousePosition.y <= 0 || Input.mousePosition.x >= Screen.width - 1 || Input.mousePosition.y >= Screen.height - 1) == true);
            if (!_downloadReady)
            {
                _mouseCheckRunning = false;
                yield break;
            }
            Alert("You have yet to download your files. Please ensure you have downloaded them before closing or refreshing the window.");
            _downloadReady = false;
            _mouseCheckRunning = false;
#endif
        }
        public void DownloadReady()
        {
            if (!_downloadReady)
            {
                _downloadReady = true;
                OnLeaveUnityScreen();
            }
        }
        public void DownloadTriggered()
        {
            if (_downloadReady)
            {
                _downloadReady = false;
            }
        }
    }
}