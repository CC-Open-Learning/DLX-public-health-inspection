using UnityEngine;

namespace VARLab.PublicHealth
{
    /// <summary>
    ///    This class is used to swap objects for loading that have a different inspection state than the default in scene.
    ///    It turns off all the inspectable game objects and turns on the one that matches the current state of the original object.
    ///    This allows for the picture of the walk-in door to be taken in an open state and then set back to the closed state when the inspection is loaded.
    /// </summary>

    public class SwapObjectForLoad : MonoBehaviour
    {
        public GameObject NewObject;
        public GameObject OriginalObject;
        public Camera CameraForLoad;

        public void SwapObjects()
        {
            OriginalObject.SetActive(!OriginalObject.activeSelf);
            NewObject.SetActive(!NewObject.activeSelf);

            var obj = NewObject.GetComponent<InspectableObject>();

            // It will only set the state if the oblect being swapped has an inspectable object attached to it.
            if (obj != null)
            {
                //make new "copy" object state match the actual objects state
                //Set all the children game objects to inactive
                foreach (var go in NewObject.GetComponent<InspectableObject>().AllScenarioStates)
                {
                    go.InspectableGameObject.SetActive(false);
                }

                // Find the game object that matches the current state and make it active.
                foreach (var go in NewObject.GetComponent<InspectableObject>().AllScenarioStates)
                {
                    if (go.InspectableObjectState == OriginalObject.GetComponent<InspectableObject>().CurrentObjState)
                    {
                        go.InspectableGameObject.SetActive(true);
                    }
                }
            }
        }
    }
}
