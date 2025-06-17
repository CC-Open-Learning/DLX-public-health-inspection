using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace VARLab.PublicHealth
{
    /// <summary>
    ///     This class is responsible for building the inspection log tab in the inspection review window. 
    ///     Using the Inspectable Manager, this class will read the information from the manager and build the UI
    ///     based on the values within the managers log dictionary. 
    /// </summary>
    public class InspectionLogBuilder : MonoBehaviour
    {
        //constants for class/element names
        [SerializeField] private InspectionLogElementNamesSO _elementNames;

        [SerializeField] private InspectableManager _inspectableManager;

        //doc references
        [SerializeField] private UIDocument _inspectionReviewWindow;

        [SerializeField] private VisualTreeAsset _mainInspectionLog;
        [SerializeField] private VisualTreeAsset _InspectionLogRow;
        [SerializeField] private VisualTreeAsset _foldout;

        [SerializeField] private ModalPopupSO _deletionWithPhoto;
        [SerializeField] private ModalPopupSO _deletionWithoutPhoto;

        //images for the background of the inspection log
        [SerializeField] private Sprite _photo;
        [SerializeField] private Sprite _visual;
        [SerializeField] private Sprite _IRThermometer;
        [SerializeField] private Sprite _probeThermometer;
        [SerializeField] private Sprite _deleteIcon;
        [SerializeField] private Sprite _missingPhoto;
        [SerializeField] private Sprite _compliantIcon;
        [SerializeField] private Sprite _nonCompliantIcon;

        //container settings
        private SortType _savedSort;

        //container references
        private TemplateContainer _inspectionContainer;
        private ScrollView _mainInspectionLogList;

        //VisualElement references
        private VisualElement _emptyLogContainer;
        private VisualElement _inspectionLogContainer;

        //untiy event 
        /// <summary><see cref="ToastManager.DisplayToast(string, Color)"/></summary>
        public UnityEvent<string, Color> CreateToast;

        /// <summary><see cref="ModalPopupBuilder.HandleDisplayModal(ModalPopupSO)"/></summary>
        public UnityEvent<ModalPopupSO> CreateModal;

        /// <summary><see cref="InspectableManager.RemoveInspection(string, string)"/></summary>
        public UnityEvent<string, string> DeleteInspection;

        //ToastColor
        private Color _toastColor = new(9f / 255, 117f / 255, 56 / 255f);

        public ImagePopUp ImagePopUp;

        /// <summary>
        ///     This struct is used to hold the data for each log in the inspection log
        /// </summary>
        private struct InspectableLogData
        {
            public string Name;
            public string Location;
            public string ID;
            public string ToolUsed;
        }

        /// <summary>
        ///    This method will sort the logs based on the button clicked
        /// </summary>
        private enum SortType
        {
            All,
            Complaint,
            NonCompliant
        }

        /// <summary> 
        ///     Builds the InspectionLog tab in the inspection review window
        /// </summary>
        public void BuildInspectionLog()
        {
            //Get the tab window element from the inspection review window 
            VisualElement tabWindowElement = _inspectionReviewWindow.rootVisualElement.Q(_elementNames.TabContent);

            //Instantiate the main inspection log container from the visual tree asset
            try
            {
                _inspectionContainer = _mainInspectionLog.Instantiate();
            }
            catch (Exception ex)
            {
                Debug.Log("Instantiate exception: " + ex.Message + " trying again");
                _inspectionContainer = _mainInspectionLog.Instantiate();
            }

            _inspectionContainer.AddToClassList(_elementNames.TemplateContainerClass);

            //get the main inspection list from the container
            _mainInspectionLogList = _inspectionContainer.Q(_elementNames.MainContainer) as ScrollView;
            _mainInspectionLogList.mouseWheelScrollSize = 1500;

            //Get the empty log container and the inspection log container
            _emptyLogContainer = _inspectionContainer.Q(_elementNames.EmptyLogContainer);
            _inspectionLogContainer = _inspectionContainer.Q(_elementNames.InspectionLogContainer);

            //Reseting sort filter
            _savedSort = SortType.All;

            if (_inspectableManager.Inspections.Count > 0)
            {
                HideEmptyLogMessage();
                //Build the sorting buttons
                BuildSorting();

                //Build all the logs in rows for the UI
                BuildAllRows();
            }
            else
            {
                DisplayEmptyLogMessage();
            }

            //Add the main activity log to the tab window
            tabWindowElement.Add(_inspectionContainer);
        }

        /// <summary>
        /// Display the empty log message
        /// </summary>
        private void HideEmptyLogMessage()
        {
            //If there are logs, hide the empty log container
            _emptyLogContainer.style.display = DisplayStyle.None;

            //Display the inspection log container
            _inspectionLogContainer.style.display = DisplayStyle.Flex;
        }

        /// <summary>
        /// Display the empty log message
        /// </summary>
        private void DisplayEmptyLogMessage()
        {
            // If there are no logs, display the empty log container
            _emptyLogContainer.style.display = DisplayStyle.Flex;

            //Hide the inspection log container
            _inspectionLogContainer.style.display = DisplayStyle.None;
        }

        /// <summary>
        ///    This method will build all the rows in the inspectable manager.
        /// </summary>
        private void BuildAllRows(SortType sort = SortType.All)
        {
            _mainInspectionLogList.Clear();
            //Create a list of foldouts to hold the logs
            List<Foldout> foldouts = new();
            //Iterate through the inspections in the inspection manager
            foreach (var inspection in _inspectableManager.Inspections)
            {
                //Get the location of the inspection
                string location = _inspectableManager.GetInspectableObjectLocation(inspection.Key);

                //if inspection has no evidences that match the sort type, skip it
                //This is to prevent empty foldouts from being created
                if (sort == SortType.Complaint && inspection.Value.InspectionEvidences.All(e => !e.Value.IsCompliant))
                {
                    continue;
                }

                if (sort == SortType.NonCompliant && inspection.Value.InspectionEvidences.All(e => e.Value.IsCompliant))
                {
                    continue;
                }


                //Create a foldout for the location if it doesn't exist
                if (!foldouts.Any(f => f.text == location))
                {
                    //Instantiate a new foldout from the visual tree asset
                    TemplateContainer foldoutContainer = _foldout.Instantiate();

                    //Get the foldout element in the container 
                    Foldout foldout = foldoutContainer.Q<Foldout>(_elementNames.Foldout);

                    //Set the foldout text to the location
                    foldout.text = location;

                    foldout.RegisterValueChangedCallback((evt) => SetAlternatingBackgrounds());

                    //Add the foldout to the list of foldouts
                    foldouts.Add(foldout);

                    //Add the foldout to the main inspection list
                    _mainInspectionLogList.Add(foldoutContainer);
                }

                //Get the foldout for the location
                Foldout locationFoldout = foldouts.First(f => f.text == location);

                //Create a row for each log in the inspection
                foreach (var log in inspection.Value.InspectionEvidences)
                {
                    //Check the sort type and skip logs that don't match
                    if (sort == SortType.Complaint && !log.Value.IsCompliant)
                    {
                        continue;
                    }
                    if (sort == SortType.NonCompliant && log.Value.IsCompliant)
                    {
                        continue;
                    }

                    //Get the inspectable object name
                    string name = inspection.Value.InspectableObjectName;

                    //Populate the row with the noncompliance log data
                    VisualElement row = PopulateRow(log.Value, location, name, inspection.Key);

                    //Add the row to the foldout
                    locationFoldout.Add(row);
                }
            }


            SetAlternatingBackgrounds();
        }


        /// <summary>
        ///     This method is used for populating the rows in the inspection log
        /// </summary>
        /// <param name="log">The evidence found to fill out the log</param>
        /// <param name="location">Where this inspection was taken place</param>
        /// <param name="name">The name of the object that was inspected</param>
        /// <returns>Returns a populated row in the form of a visual element</returns>
        private VisualElement PopulateRow(Evidence log, string location, string name, string id)
        {
            //Instantiate the row and get the row element
            TemplateContainer container = _InspectionLogRow.Instantiate();
            VisualElement row = container.Q(_elementNames.Row);

            // Populate the row with the location
            Label locationLabel = row.Q<Label>(_elementNames.Location);
            locationLabel.text = location;

            //populate the row with the inspection log data
            Label item = row.Q<Label>(_elementNames.Item);
            item.text = name;

            //get the tool used container and the photo element
            VisualElement toolUsedContainer = row.Q<VisualElement>(_elementNames.ToolContainer);
            VisualElement photo = toolUsedContainer.Q<VisualElement>(_elementNames.ToolPhoto);

            //set the image of the background based on the type of inspection
            if (log.ToolLabel == ToolStringsSO.LogVisualString)
                photo.style.backgroundImage = new StyleBackground(_visual);

            else if (log.ToolLabel == ToolStringsSO.LogInfraredString)
                photo.style.backgroundImage = new StyleBackground(_IRThermometer);

            else if (log.ToolLabel == ToolStringsSO.LogProbeString)
                photo.style.backgroundImage = new StyleBackground(_probeThermometer);

            //populate the row with the tool used
            Label toolUsed = toolUsedContainer.Q<Label>(_elementNames.ToolLabel);
            toolUsed.text = log.ToolLabel;

            VisualElement compliancyIcon = row.Q(_elementNames.CompliancyIcon);
            compliancyIcon.style.backgroundImage = log.IsCompliant ? new StyleBackground(_compliantIcon) : new StyleBackground(_nonCompliantIcon);

            //populate the row with the reading
            Button info = row.Q<Button>(_elementNames.InfoBtn);
            info.text = log.Reading;
            info.SetEnabled(false);

            if (log.ToolLabel == ToolStringsSO.LogVisualString)
            {
                if (info.text == ToolStringsSO.LogPhotoAvailableString)
                {
                    info.SetEnabled(true);
                    //add listener to image popup
                    info.clicked += () => ImagePopUp.OpenImage(id);
                }
            }

            //display the delete icon
            Button deleteButton = row.Q<Button>(_elementNames.DeleteButton);
            deleteButton.style.backgroundImage = new StyleBackground(_deleteIcon);

            if (log.ToolLabel == ToolStringsSO.LogPhotoString)
            {
                deleteButton.clicked += () =>
                {
                    _deletionWithPhoto.SetPrimaryAction(() => DeleteLog(row));
                    CreateModal?.Invoke(_deletionWithPhoto);
                };
            }
            else
            {
                deleteButton.clicked += () =>
                {
                    _deletionWithoutPhoto.SetPrimaryAction(() => DeleteLog(row));
                    CreateModal?.Invoke(_deletionWithoutPhoto);
                };
            }

            row.userData = new InspectableLogData { Name = name, Location = location, ID = id, ToolUsed = toolUsed.text };

            return container;
        }

        /// <summary>
        ///     This method will dynamically alternate background colors depending
        ///     on the number of visible logs at any given time. Defaults to a minimum of
        ///     8 rows so the compliance log is always styled
        /// </summary>
        private void SetAlternatingBackgrounds()
        {
            int i = 0;
            int tempRowIndex = 0;
            const int minRows = 8;      // Used for styling, ensures container always full

            // Style the existing rows 
            foreach (var element in _mainInspectionLogList.Children())
            {
                // Get access to each foldout
                TemplateContainer container = element as TemplateContainer;
                Foldout foldout = container.Q<Foldout>();

                // If this was a previously created empty foldout for padding, remove it
                if (foldout.style.visibility == Visibility.Hidden)
                {
                    tempRowIndex = _mainInspectionLogList.IndexOf(element);
                    break;
                }

                foldout.style.backgroundColor = DetermineBackgroundColor(++i);
                // If the foldout is open, apply the same pattern to its children
                if (foldout.value)
                {
                    foreach (var row in foldout.Children())
                    {
                        row.style.backgroundColor = DetermineBackgroundColor(++i);
                    }
                }

            }

            // Remove old temp rows
            if (tempRowIndex != 0)
            {
                TrimEmptyRows(tempRowIndex);
            }

            // If there aren't enough rows, style temporary empty rows
            for (; i <= minRows;)
            {
                TemplateContainer emptyRow = GenerateEmptyRow();
                emptyRow.style.backgroundColor = DetermineBackgroundColor(++i);
                _mainInspectionLogList.Add(emptyRow);
            }
        }

        /// <summary>
        ///     This method is used to alternate the color of a rows background, based on the
        ///     current number of rows 
        /// </summary>
        /// <param name="count">The current number of rows</param>
        /// <returns>The correct color to apply to that row</returns>
        private Color DetermineBackgroundColor(int count)
        {
            Color color;
            if (count % 2 == 0)
            {
                color = new Color(0.18f, 0.18f, 0.18f);
            }
            else
            {
                color = Color.black;
            }
            return color;
        }

        /// <summary>
        ///     This method generates & adds empty rows to add to the main container, to 
        ///     provide consistency to the visual style of the container
        /// </summary>
        /// <returns></returns>
        private TemplateContainer GenerateEmptyRow()
        {
            //Instantiate a new container from the visual tree asset
            TemplateContainer container = _foldout.Instantiate();

            //Get the foldout element in the container 
            Foldout foldout = container.Q<Foldout>(_elementNames.Foldout);
            foldout.style.visibility = Visibility.Hidden;

            return container;
        }

        /// <summary>
        ///     This method cleans up generated empty rows that may no longer be necessary.
        ///     It removes everything from start index to the end of the container
        /// </summary>
        /// <param name="startIndex">The index of the first empty row</param>
        private void TrimEmptyRows(int startIndex)
        {
            for (int i = _mainInspectionLogList.childCount - 1; i >= startIndex; i--)
            {
                _mainInspectionLogList.RemoveAt(i);
            }
        }

        /// <summary>
        ///     This method starts the process of deleting a inspection log. Extracts
        ///     the key & log data from the UI, then sends to the manager for deletion.
        /// </summary>
        /// <param name="toDelete"></param>
        private void DeleteLog(VisualElement toDelete)
        {
            //Read from the container
            InspectableLogData data = (InspectableLogData)toDelete.userData;
            CreateToast?.Invoke("Inspection Deleted!", _toastColor);
            try
            {
                DeleteInspection?.Invoke(data.ID, ToolsExtension.ConvertLogToolToEnum(data.ToolUsed));
            }
            catch (Exception e)
            {
                Debug.Log("Error deleting inspection: " + e.Message);
            }

            _mainInspectionLogList.Clear();
            BuildAllRows(_savedSort);

            if (_inspectableManager.Inspections.Count == 0)
            {
                DisplayEmptyLogMessage();
            }
        }

        /// <summary>
        ///     This method will build the sorting buttons for the inspection log
        /// </summary>
        private void BuildSorting()
        {
            //Get references to the buttons
            Button allButton = _inspectionContainer.Q<Button>(_elementNames.AllBtnName);
            Button compliantButton = _inspectionContainer.Q<Button>(_elementNames.CompliantBtnName);
            Button nonCompliantButton = _inspectionContainer.Q<Button>(_elementNames.NonCompliantBtnName);

            List<Button> buttons = new() { allButton, compliantButton, nonCompliantButton };

            allButton.clicked += () =>
            {
                _savedSort = SortType.All;
                BuildAllRows(_savedSort);
                HandleButtonStyling(buttons, buttons.IndexOf(allButton));
            };

            compliantButton.clicked += () =>
            {
                _savedSort = SortType.Complaint;
                BuildAllRows(_savedSort);
                HandleButtonStyling(buttons, buttons.IndexOf(compliantButton));
            };

            nonCompliantButton.clicked += () =>
            {
                _savedSort = SortType.NonCompliant;
                BuildAllRows(_savedSort);
                HandleButtonStyling(buttons, buttons.IndexOf(nonCompliantButton));
            };

            //Set the default button style
            HandleButtonStyling(buttons, buttons.IndexOf(allButton));
        }

        /// <summary>
        ///     This method will handle the styling of the sorting buttons and which button is active
        /// </summary>
        /// <param name="buttons">the list of buttons</param>
        /// <param name="indexOfActiveButton">the index of the active button</param>
        public void HandleButtonStyling(List<Button> buttons, int indexOfActiveButton)
        {
            for (int i = 0; i < buttons.Count; i++)
            {
                if (i == indexOfActiveButton)
                {
                    StyleSortingButtons(buttons[i], true);
                }
                else
                {
                    StyleSortingButtons(buttons[i], false);
                }
            }
        }


        /// <summary>
        ///     This method will style the sorting buttons based on the active button
        /// </summary>
        /// <param name="btn">The button to style</param>
        /// <param name="active">If this button is the active item</param>
        public void StyleSortingButtons(Button btn, bool active)
        {
            //Remove the inactive style from the button
            if (active)
            {
                btn.RemoveFromClassList(_elementNames.InactiveBtnStyleSelector);//string is "inactive-button"
                btn.AddToClassList(_elementNames.ActiveBtnStyleSelector);//string is "active-button"
                //check if the button is the all button and if so, don't try to style the icon
                if (btn.name == _elementNames.AllBtnName)
                {
                    return;
                }


                //get the icon within the button
                var icon = btn.Q(_elementNames.SortIcon); //string is "Icon"
                icon.RemoveFromClassList(_elementNames.InactiveIconColourSelector);//string is "inactive-icon-colour"
                icon.AddToClassList(_elementNames.ActiveIconColourSelector);//string is "active-icon-colour"
            }
            else
            {
                btn.RemoveFromClassList(_elementNames.ActiveBtnStyleSelector);//string is "active-button"
                btn.AddToClassList(_elementNames.InactiveBtnStyleSelector);//string is "inactive-button"

                //check if the button is the all button and if so, don't try to style the icon
                if (btn.name == _elementNames.AllBtnName)
                {
                    return;
                }


                //get the icon within the button
                var icon = btn.Q(_elementNames.SortIcon);//string is "Icon"
                icon.RemoveFromClassList(_elementNames.ActiveIconColourSelector);//string is "active-icon-colour"
                icon.AddToClassList(_elementNames.InactiveIconColourSelector);//string is "inactive-icon-colour"
            }
        }
    }
}
