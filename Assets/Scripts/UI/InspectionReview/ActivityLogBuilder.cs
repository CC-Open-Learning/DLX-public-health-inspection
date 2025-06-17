using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace VARLab.PublicHealth
{
    /// <summary>
    /// This class is used to build the activity log UI.
    /// </summary>
    public class ActivityLogBuilder : MonoBehaviour
    {
        //Template container reference
        public TemplateContainer _activityLogContainer;

        //Doc references
        [SerializeField] private ActivityLogManager _activityLogManager;
        [SerializeField] private UIDocument _inspectionReviewWindow;
        [SerializeField] private VisualTreeAsset _mainActivityLog;

        //String resource references
        [SerializeField] private ActivityLogTabElementNamesSO _elementNames;

        /// <summary> This is the main activity log list scroll view. It is the container for the logs in the UI</summary>
        private ScrollView _mainActivityLogList;

        //Visual Element references
        private VisualElement _emptyLogContainer;
        private VisualElement _activityLogViewContainer;

        //Dictionary to store the activity logs
        private Dictionary<string, List<string>> _activityLogs;

        //Foldout reference
        private Foldout _lastFoldout;

        //Exit log reference
        private Label _exitLog;

        public void Awake()
        {
            _activityLogs = new();
            _exitLog = new();
        }

        /// <summary>
        /// This method is used to build the activity log UI
        /// </summary>
        public void BuildActivityLog()
        {
            //get the tab window element for the inspection review window
            VisualElement tabWindowElement = _inspectionReviewWindow.rootVisualElement.Q(_elementNames.TabContent);

            //build main activity log and get reference to ListView
            try
            {
                _activityLogContainer = _mainActivityLog.Instantiate();
            }
            catch (Exception ex)
            {
                Debug.Log("Instantiate exception: " + ex.Message + " trying again");
                _activityLogContainer = _mainActivityLog.Instantiate();
            }

            //Get the main activity log list
            _mainActivityLogList = _activityLogContainer.Q(_elementNames.ActivityLogList) as ScrollView;
            _mainActivityLogList.mouseWheelScrollSize = 1500;
            _activityLogViewContainer = _activityLogContainer.Q(_elementNames.ActivityLogContainer);
            _emptyLogContainer = _activityLogContainer.Q(_elementNames.EmptyLogContainer);

            //Add the template container class
            _activityLogContainer.AddToClassList(_elementNames.TemplateContainerClass);

            //Check if there are logs
            if (_activityLogManager.PrimaryLogs.Count == 0)
            {
                DisplayEmptyLogMessage();
            }
            else
            {
                HideEmptyLogMessage();
                _mainActivityLogList.Add(PopulateActivityLogUI());
            }

            //add container to window element
            tabWindowElement.Add(_activityLogContainer);

        }

        /// <summary>
        /// Display the empty log message
        /// </summary>
        private void HideEmptyLogMessage()
        {
            //If there are logs, hide the empty log container
            _emptyLogContainer.style.display = DisplayStyle.None;

            //Display the inspection log container
            _activityLogViewContainer.style.display = DisplayStyle.Flex;
        }

        /// <summary>
        /// Display the empty log message
        /// </summary>
        private void DisplayEmptyLogMessage()
        {
            // If there are no logs, display the empty log container
            _emptyLogContainer.style.display = DisplayStyle.Flex;

            //Hide the inspection log container
            _activityLogViewContainer.style.display = DisplayStyle.None;
        }

        /// <summary>
        /// This method is used to populate the activity log UI
        /// </summary>
        /// <returns></returns>
        private VisualElement PopulateActivityLogUI()
        {
            // Add a container
            VisualElement container = new();
            _activityLogs.Clear();

            // Iterate over the primary logs
            foreach (PrimaryLog primaryLog in _activityLogManager.PrimaryLogs)
            {
                Foldout foldout = new() { text = primaryLog.ParentLog.LogContent };
                SetupFoldout(foldout);

                _mainActivityLogList.Add(foldout);

                // Iterate over the logs in primary logs
                foreach (var log in primaryLog.logs)
                {
                    Label label = new() { text = log.LogContent };
                    foldout.Add(label);
                }

                container.Add(foldout);
            }

            return container;
        }



        /// <summary>
        ///     This method is used to style the foldout and setup the callbacks so that the foldout text highlights and checkmark is white 
        /// </summary>
        /// <param name="foldout">The foldout to be setup</param>
        /// <returns></returns>
        private void SetupFoldout(Foldout foldout)
        {
            foldout.style.fontSize = 24;
            foldout.style.color = Color.white;
            foldout.value = false;
            foldout.contentContainer.style.marginLeft = 80;

            //callback for changing the foldout text color  to yellow when selected
            foldout.RegisterCallback<ClickEvent>(evt =>
            {
                //This if / else block is used to change the color of the foldout text and checkmark when selected when its toggled on and off
                if (foldout.value)
                {
                    //make the color rgb 255,192,9 (#FFC009)
                    foldout.Q<Label>().style.color = new Color(255f / 255f, 192f / 255f, 9f / 255f);
                    foldout.Q<Label>().style.unityFontStyleAndWeight = FontStyle.Bold;
                }
                else
                {
                    foldout.Q<Label>().style.color = Color.white;
                    foldout.Q<Label>().style.unityFontStyleAndWeight = FontStyle.Normal;
                }

                //this if statement handles the case where the user clicks on a different foldout while one is already selected
                if (_lastFoldout != null && _lastFoldout != foldout)
                {
                    _lastFoldout.Q<Label>().style.color = Color.white;
                    _lastFoldout.Q<Label>().style.unityFontStyleAndWeight = FontStyle.Normal;
                    _lastFoldout.value = false;
                }

                //Get the last child of the foldout and check if it is an exit log
                int x = foldout.childCount;
                if (x > 0)
                {
                    var temp = foldout.ElementAt(x - 1).Q<Label>();
                    //check if temp is an exit log 
                    if (temp.text.Contains("Exit"))
                    {
                        _exitLog = temp;
                        _exitLog.style.color = new Color(255f / 255f, 192f / 255f, 9f / 255f);
                        _exitLog.style.unityFontStyleAndWeight = FontStyle.Bold;
                        _exitLog.style.marginLeft = -17;
                    }
                }
                _lastFoldout = foldout;
            });
        }
    }
}
