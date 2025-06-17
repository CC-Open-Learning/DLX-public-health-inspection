using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace VARLab.PublicHealth
{
    public class OfficeHandler : MonoBehaviour
    {
        /// <summary><see cref="Conversation.HandleDisplayConversation(ConversationSO)"/></summary>
        public UnityEvent OnOfficeArrive;

        public IEnumerator EnterOffice()
        {
            yield return new WaitUntil(() => CameraController.LookAtComplete == true);
            yield return new WaitForSeconds(0.5f);
            OnOfficeArrive?.Invoke();
        }
    }
}
