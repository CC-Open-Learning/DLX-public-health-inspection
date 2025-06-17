using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace VARLab.PublicHealth
{
    /// <summary>
    /// This class is responsible for handling the introduction event. The script is attached to the movement tile in front of the receptionist. 
    /// When the player interacts with the tile, the introduction event is triggered.
    /// </summary>
    public class IntroductionHandler : MonoBehaviour
    {
        /// <summary><see cref="Conversation.HandleDisplayConversation(ConversationSO)"/></summary>
        public UnityEvent OnIntroStart;

        /// <summary>
        /// Listener <see cref="InformationModalBuilder.HandleDisplayModal(ModalPopupSO)"/>
        /// </summary>
        public UnityEvent<InformationModalSO> ShowModalPopup;

        [SerializeField, Tooltip("List of allowed POI before introduction")] private List<POI> _allowedPOIs;

        [SerializeField, Tooltip("The scriptable object for the restart window")] private InformationModalSO _restartSO;

        [SerializeField, Tooltip("Reference to to manager game object")] private GameObject _manager;

        private const string IntroductionToast = "You introduced yourself and notified the facility about the inspection.";

        private Color _toastColour = new(9f / 255, 117f / 255, 56f / 255);

        /// <summary>
        /// This method is for handling the intro and just returns immediately if the intro has been completed
        /// </summary>
        /// <param name="MoveTile"> The movement tile the player has moved to </param>
        public IEnumerator HandleIntroduction(GameObject movementTile)
        {
            // If the player has not completed the introductiona nd they are not in an allowed POI they need to restart the game.
            if (!PlayerController.IntroCompleted && !_allowedPOIs.Contains(PHISceneManager.Instance.PoiManager.CurrentPOI))
            {
                // wait for walk completed to finish
                yield return new WaitForFixedUpdate();

                // set the primary action for the information modal button
                _restartSO.SetPrimaryAction(Fade.Singleton.SetFadeAction(() => PHISceneManager.Instance.RestartScene(), true));
                Interactions.Instance.SetInteract(false);
                ShowModalPopup?.Invoke(_restartSO);
            }

            // If the player has moved to a tile with an introduction handler invoke the intro event. This is used to open the introduction dialogue window.
            if (movementTile.TryGetComponent(out IntroductionHandler introductionHandler))
            {
                // disables the manager collider and removes tag since the manager is no longer interactable
                _manager.GetComponent<CapsuleCollider>().enabled = false;
                _manager.tag = "Untagged";

                // wait for player to start walking
                yield return new WaitForFixedUpdate();

                // wait for player to reach the target
                yield return new WaitUntil(() => PlayerController.IsPlayerWalking == false);

                // wait to display dialogue
                yield return new WaitUntil(() => CameraController.LookAtComplete == true);
                yield return new WaitForSeconds(0.5f);

                OnIntroStart?.Invoke();
            }
        }

        /// <summary>
        /// <see cref="Conversation.OnConversationEnded"/>
        /// </summary>
        public void IntroToast()
        {
            if (!PlayerController.VisitedOffice)
            {
                ToastManager.Instance.DisplayToast(IntroductionToast, _toastColour, false, ToastIcons.Check);
            }
        }
    }
}
