using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;


namespace VARLab.PublicHealth
{
    /// <summary>
    ///     This class is used to manage the inspection window. It will display the inspection window when an inspectable object is clicked on.
    ///     It also handles all the cases of re-inspection and the user selecting compliant or non-compliant. 
    ///     The user can take a photo of the object and the sound / visual effects are handled here.
    /// </summary>
    public class InspectionWindow : MonoBehaviour
    {
        /// <summary><see cref="ImageManager.AddPhotoToGallery(InspectablePhoto)"/></summary>
        public UnityEvent<InspectablePhoto> PhotoConfirmed;

        /// <summary><see cref="ImageManager.UpdatePhotoTimeStamp(InspectablePhoto)"/></summary>
        public UnityEvent<InspectablePhoto> UpdatePhotoTimeStamp;

        /// <summary><see cref="ActivityLogManager.LogInspection(Tools, InspectableObject)"/></summary>
        public UnityEvent<Tools, InspectableObject> LogInspectionEvent;

        /// <summary> <see cref="POIManager.OnInspectableInteracted(POI)"/> </summary>
        public UnityEvent<POI> POIInteractedEvent;

        /// <summary><see cref="ToastManager.DisplayInspectionToast(string, Color, bool)"/></summary>
        public UnityEvent<string, Color, bool> CreateToast;

        /// <summary><see cref="ToastManager.HideToast()"/></summary>
        public UnityEvent HideToast;

        /// <summary><see cref="InspectableManager.OnInspectionCompleted(InspectableObject, Tools)"/></summary>
        public UnityEvent<InspectableObject, Tools> InspectionCompleted;

        /// <summary><see cref="ModalPopupBuilder.HandleDisplayModal(ModalPopupSO)"/></summary>
        public UnityEvent<ModalPopupSO> CreateModal;

        //The selected tool value
        public Tools SelectedTool;

        [SerializeField] private UIDocument _inspectionWindow;
        [SerializeField] private InspectableManager _inspectableManager;

        [SerializeField] private ModalPopupSO _replaceCompliantModal;
        [SerializeField] private ModalPopupSO _replaceNonCompliantModal;
        [SerializeField] private ModalPopupSO _discardPhotoModal;

        [SerializeField] private InspectionWindowElementNamesSO _elementNames;
        [SerializeField] private InspectionPopupStringsSO _popupStrings;

        [SerializeField] private AudioSource _cameraSound;

        //Buttons used for the tools
        private Button _visualBtn;
        private Button _iRTempBtn;
        private Button _probeTempBtn;
        private List<Button> _toolBtns;

        //Buttons used for the user to select compliant or non compliant
        private Button _compliantBtn;
        private Button _nonCompliantBtn;

        //Close button
        private Button _closeBtn;

        //Take photo button
        private Button _takePhotoBtn;

        //Labels for the inspectable object and location
        private Label _inspectableLbl;
        private Label _locationLbl;

        //The display area for the visual inspection
        private VisualElement _displayArea;

        //Texture for display area
        private Texture2D tex;

        //The inspectable object that was clicked on
        private InspectableObject _inspectable;

        //The pop up for the probe temperature and infrared temperature
        private VisualElement _probePopUp;
        private VisualElement _infraPopUp;

        //The labels for the probe temperature and infrared temperature
        private Label _probeTemp;
        private Label _infraTemp;

        //The flash effect for the camera and the border for the photo
        private VisualElement _flash;
        private VisualElement _border;

        //The photo that was taken
        private InspectablePhoto _photo;

        //The info label
        private Label _infoLbl;

        //Flags for the inspection window
        public bool ReinspectDialogOpen = false;
        private bool? _previousInspection;
        private bool _isPhoto = false;

        //const string for empty string
        private const string empty = "";

        // USS Classes
        private const string _yellowHighlight = "YellowHighlight";
        private const string _leftBorder = "LeftBorder";

        //Actions for the buttons
        ActionWrapper onClose;
        ActionWrapper onProbe;
        ActionWrapper onIr;
        ActionWrapper onCompliant;
        ActionWrapper onNonCompliant;
        ActionWrapper onVisual;
        ActionWrapper onPhoto;


        // Start is called before the first frame update
        void Start()
        {
            if (_inspectionWindow == null)
            {
                return; //this is for avoiding null refs in tests on inspectable objects
            }
            SelectedTool = Tools.None;
            //we get all the references to the various buttons and labels on start
            GetAllReferences();

            _inspectionWindow.rootVisualElement.style.display = DisplayStyle.None;
        }

        /// <summary>
        /// Cleaned up start method isolating actions into methods
        /// </summary>
        private void GetAllReferences()
        {
            _visualBtn = _inspectionWindow.rootVisualElement.Q<Button>(_elementNames.VisualBtn);
            _iRTempBtn = _inspectionWindow.rootVisualElement.Q<Button>(_elementNames.IRTempBtn);
            _probeTempBtn = _inspectionWindow.rootVisualElement.Q<Button>(_elementNames.ProbeTempBtn);
            _toolBtns = new List<Button>() { _visualBtn, _iRTempBtn, _probeTempBtn };

            _infoLbl = _inspectionWindow.rootVisualElement.Q<Label>(_elementNames.InfoLabel);

            _compliantBtn = _inspectionWindow.rootVisualElement.Q<Button>(_elementNames.CompliantBtn);
            _nonCompliantBtn = _inspectionWindow.rootVisualElement.Q<Button>(_elementNames.NonCompliantBtn);
            _closeBtn = _inspectionWindow.rootVisualElement.Q<Button>(_elementNames.CloseBtn);

            _inspectableLbl = _inspectionWindow.rootVisualElement.Q<Label>(_elementNames.ItemLabel);
            _locationLbl = _inspectionWindow.rootVisualElement.Q<Label>(_elementNames.LocationLabel);
            _displayArea = _inspectionWindow.rootVisualElement.Q(_elementNames.DisplayArea);

            _probePopUp = _inspectionWindow.rootVisualElement.Q(_elementNames.ProbePopup);
            _probeTemp = _probePopUp.Q<Label>(_elementNames.ProbeTemp);

            _infraPopUp = _inspectionWindow.rootVisualElement.Q(_elementNames.InfraPopup);
            _infraTemp = _infraPopUp.Q<Label>(_elementNames.InfraTemp);

            _flash = _inspectionWindow.rootVisualElement.Q(_elementNames.FlashEffect);
            _border = _inspectionWindow.rootVisualElement.Q(_elementNames.PhotoBorder);
            _takePhotoBtn = _inspectionWindow.rootVisualElement.Q<Button>(_elementNames.TakePhotoBtn);

            onClose = new(_closeBtn);
            onProbe = new(_probeTempBtn);
            onIr = new(_iRTempBtn);
            onCompliant = new(_compliantBtn);
            onNonCompliant = new(_nonCompliantBtn);
            onVisual = new(_visualBtn);
            onPhoto = new(_takePhotoBtn);
        }

        /// <summary>
        /// This is the listener to the event <see cref="InspectableObject.InspectionMade"/> and triggers on click of an inspectable
        /// </summary>
        /// <param name="obj"> The inspectable object that was clicked </param>
        public void OpenWindow(InspectableObject obj)
        {
            if (_inspectionWindow == null)
            {
                return; //this is for avoiding null refs in tests on objects
            }
            _inspectable = obj;
            _inspectableLbl.text = obj.InspectableObjectName;

            _inspectionWindow.rootVisualElement.style.display = DisplayStyle.Flex;

            Interactions.Instance.SetInteract(false);
            Interactions.Instance.TurnOffUI();

            //get the poi from the object
            POI poi = obj.GetComponentInParent<POI>();

            if (!poi.Interacted)
            {
                POIInteractedEvent?.Invoke(poi);
            }

            _locationLbl.text = obj.Location;

            ResetListeners();
            //here setup listeners
            SetupListeners();

            // Set selected tool to none, allows DisplayVisual to run
            SelectedTool = Tools.None;
            DisplayVisual();
        }

        /// <summary>
        /// this changes the visibility of the window to none to hide it
        /// </summary>
        public void CloseWindow()
        {
            _inspectionWindow.rootVisualElement.style.display = DisplayStyle.None;

            ResetWindow();

            if (_inspectable != null)
            {
                _inspectable.ToggleObjects();
            }

            Interactions.Instance.ReEnableActions();
            Interactions.Instance.TurnOnUI();

            Destroy(tex);
        }

        /// <summary>
        /// Clears all buttons listeners
        /// </summary>
        private void ResetListeners()
        {
            onClose.ClearBtn();
            onCompliant.ClearBtn();
            onNonCompliant.ClearBtn();
            onVisual.ClearBtn();
            onIr.ClearBtn();
            onProbe.ClearBtn();
            onPhoto.ClearBtn();
        }

        /// <summary>
        /// Cleaned up start method isolating actions into methods
        /// </summary>
        private void SetupListeners()
        {
            onClose.AddAction(CloseWindow);
            onProbe.AddAction(DisplayProbeTemp);
            onIr.AddAction(DisplayIRTemp);
            onCompliant.AddAction(Compliant);
            onNonCompliant.AddAction(NonCompliant);
            onVisual.AddAction(DisplayVisual);
            onPhoto.AddAction(TakePhoto);
        }

        /// <summary>
        ///     Reset the window to its default state
        /// </summary>
        private void ResetWindow()
        {
            HideTools();
            RemoveBtnSelection();
        }

        /// <summary>
        /// hides the temperature reading pop up
        /// </summary>
        private void HideTools()
        {
            _probePopUp.style.display = DisplayStyle.None;
            _infraPopUp.style.display = DisplayStyle.None;
            _takePhotoBtn.style.display = DisplayStyle.None;
            _border.RemoveFromClassList("PhotoFrameVisible");
            _isPhoto = false;
        }

        /// <summary>
        /// remove border from all buttons
        /// </summary>
        private void RemoveBtnSelection()
        {
            foreach (VisualElement btn in _toolBtns)
            {
                btn.RemoveFromClassList(_leftBorder);
                btn.RemoveFromClassList(_yellowHighlight);
            }
            _infoLbl.text = empty;
        }

        /// <summary>
        /// this is added as a listener to <see cref="ImageManager.TempImageTaken"/> so when a temp image is taken it sets the background image
        /// of the visual display area to that photo.
        /// </summary>
        /// <param name="photo"> the photo to set as background </param>
        public void SetPhoto(InspectablePhoto photo)
        {
            _photo = photo;
            tex = new Texture2D(ImageManager.ResWidth, ImageManager.ResHeight);
            tex.LoadImage(photo.Data);
            _displayArea.style.backgroundImage = tex;
        }

        /// <summary>
        ///     Displays the objects visual inspection state.
        /// </summary>
        public void DisplayVisual()
        {
            if (SelectedTool != Tools.Visual)
            {
                ResetWindow();

                SelectedTool = Tools.Visual;

                _previousInspection = _inspectableManager.PreviousInspectionCompliancy(_inspectable.InspectableObjectID, SelectedTool.ToString());
                _popupStrings.SetCurrentInspectionMessage(SelectedTool, _previousInspection, _inspectable.HasPhoto);

                LogInspection(Tools.Visual);

                _visualBtn.AddToClassList(_leftBorder);
                _visualBtn.AddToClassList(_yellowHighlight);

                _takePhotoBtn.style.display = DisplayStyle.Flex;
            }
        }

        /// <summary>
        /// Displays the objects probe temp
        /// </summary>
        public void DisplayProbeTemp()
        {
            if (SelectedTool != Tools.ProbeThermometer)
            {
                ResetWindow();

                SelectedTool = Tools.ProbeThermometer;

                _previousInspection = _inspectableManager.PreviousInspectionCompliancy(_inspectable.InspectableObjectID, SelectedTool.ToString());
                _popupStrings.SetCurrentInspectionMessage(SelectedTool, _previousInspection);

                LogInspection(Tools.ProbeThermometer);

                _probeTempBtn.AddToClassList(_leftBorder);
                _probeTempBtn.AddToClassList(_yellowHighlight);

                _probeTemp.text = _inspectable.CurrentObjState.ProbeTemp.ToString("F1", CultureInfo.InvariantCulture);
                _probePopUp.style.display = DisplayStyle.Flex;

            }
        }

        /// <summary>
        /// Displayer the objects IR temp
        /// </summary>
        public void DisplayIRTemp()
        {
            if (SelectedTool != Tools.IRThermometer)
            {
                ResetWindow();

                SelectedTool = Tools.IRThermometer;

                _previousInspection = _inspectableManager.PreviousInspectionCompliancy(_inspectable.InspectableObjectID, SelectedTool.ToString());
                _popupStrings.SetCurrentInspectionMessage(SelectedTool, _previousInspection);

                LogInspection(Tools.IRThermometer);

                _iRTempBtn.AddToClassList(_leftBorder);
                _iRTempBtn.AddToClassList(_yellowHighlight);

                _infraTemp.text = _inspectable.CurrentObjState.IRTemp.ToString("F1", CultureInfo.InvariantCulture);
                _infraPopUp.style.display = DisplayStyle.Flex;
            }
        }

        /// <summary>
        /// the method added as the action to happen when the camera button is selected
        /// </summary>
        public void TakePhoto()
        {
            SelectedTool = Tools.Visual;

            _popupStrings.CurrentInspectionMesssage = _popupStrings.CameraInfo;

            _border.AddToClassList("PhotoFrameVisible");
            _isPhoto = true;
            _cameraSound.Play();
            StartCoroutine(FlashRoutine());

            EnableWarnings();

            //Create a toast to inform the user that a photo was taken
            ToastManager.Instance.DisplayToast(message: _popupStrings.ToastPhotoTaken, alignment: ToastAlignment.TopInspectionWindow, color: new Color(6 / 255f, 113 / 255f, 224 / 255f, 1),
                displayTime: 2f, size: ToastSize.Auto, borderStyle: ToastBorderStyle.BorderNone);

            if (_previousInspection != null && IsVisualInspectionChanging())
            {
                //Create a toast to inform the photo was added to the visual inspection
                ToastManager.Instance.DisplayToast(message: _popupStrings.ToastPhotoAddedToVisual, alignment: ToastAlignment.TopInspectionWindow, color: new Color(6 / 255f, 113 / 255f, 224 / 255f, 1),
               displayTime: 3f, size: ToastSize.Auto, borderStyle: ToastBorderStyle.BorderNone);
            }

            //Invoke the event to update the timestamp of the photo
            UpdatePhotoTimeStamp?.Invoke(_photo);

            //log
            LogInspection(Tools.None);
        }

        /// <summary>
        /// This is the method that communicates that an inspection happened that should be logged
        /// </summary>
        /// <param name="tool"> The tool used in the inspection </param>
        private void LogInspection(Tools tool)
        {
            LogInspectionEvent?.Invoke(tool, _inspectable);
        }

        /// <summary>
        /// marks object as compliant and logs
        /// </summary>
        public void Compliant()
        {
            _previousInspection = _inspectableManager.PreviousInspectionCompliancy(_inspectable.InspectableObjectID, SelectedTool.ToString());
            //If it's already compliant don't do anything and display a toast
            if (_previousInspection == true && !IsVisualInspectionChanging())
            {
                CreateToast?.Invoke(_popupStrings.ToastPreviousCompliant, ToastManager._defaultColor, true);
                _inspectable.UserSelectionCompliant = true;
                return;
            }

            if (_previousInspection == false && !ReinspectDialogOpen)
            {
                ReinspectDialogOpen = true;
                ReinspectCompliant();
                return;
            }

            ConfirmPhoto();//only confirms if camera is selected
            _inspectable.UserSelectionCompliant = true;
            InspectionCompleted?.Invoke(_inspectable, SelectedTool);

            string toastString = BuildReportMessage(true, SelectedTool, _inspectable.InspectableObjectName);

            CloseWindow();

            CreateToast?.Invoke(toastString, ToastManager._defaultColor, false);

            _previousInspection = null;
        }

        /// <summary>
        /// marks object as noncompliant and logs, additionally add to the non compliance log
        /// </summary>
        public void NonCompliant()
        {
            _previousInspection = _inspectableManager.PreviousInspectionCompliancy(_inspectable.InspectableObjectID, SelectedTool.ToString());
            //If it's already compliant don't do anything and display a toast
            if (_previousInspection == false && !IsVisualInspectionChanging())
            {
                CreateToast?.Invoke(_popupStrings.ToastPreviousNonCompliant, ToastManager._defaultColor, true);
                _inspectable.UserSelectionCompliant = false;
                return;
            }
            else if (_previousInspection == true && !ReinspectDialogOpen)
            {
                ReinspectDialogOpen = true;
                ReinspectNonCompliant();
                return;
            }

            ConfirmPhoto();//only confirms if camera is selected
            _inspectable.UserSelectionCompliant = false;
            InspectionCompleted?.Invoke(_inspectable, SelectedTool);

            string toastString = BuildReportMessage(false, SelectedTool, _inspectable.InspectableObjectName);

            CloseWindow();

            CreateToast?.Invoke(toastString, ToastManager._defaultColor, false);

            _previousInspection = null;
        }

        /// <summary>
        /// Wrapper for the confirm dialog to replace the inspection as compliant
        /// </summary>
        public void ReinspectCompliant()
        {
            _replaceNonCompliantModal.SetPrimaryAction(() =>
            {
                Compliant();
                ReinspectDialogOpen = false;
            });

            _replaceNonCompliantModal.SetSecondaryAction(() =>
            {
                ReinspectDialogOpen = false;
            });

            CreateModal?.Invoke(_replaceNonCompliantModal);
            HideToast?.Invoke();
        }

        /// <summary>
        /// Wrapper for the confirm dialog to replace the inspection as non compliant
        /// </summary>
        public void ReinspectNonCompliant()
        {
            _replaceCompliantModal.SetPrimaryAction(() =>
            {
                NonCompliant();
                ReinspectDialogOpen = false;
            });

            _replaceCompliantModal.SetSecondaryAction(() =>
            {
                ReinspectDialogOpen = false;
            });

            CreateModal?.Invoke(_replaceCompliantModal);
            HideToast?.Invoke();
        }

        /// <summary>
        /// Helper function to determine if a visual inspection is changing
        /// </summary>
        private bool IsVisualInspectionChanging()
        {
            // If this is a visual inspection without a photo on an inspectable that has a photo, 
            // Or if this is a visual inspection with a photo on an inspectable without a photo

            // If the user is trying to report a visual inspection without a photo on an inspectable that has a photo
            if (SelectedTool == Tools.Visual && !_isPhoto && _inspectable.HasPhoto)
            {
                return false;
            }
            else
            {
                return (SelectedTool == Tools.Visual && !_isPhoto && _inspectable.HasPhoto) || (_isPhoto && !_inspectable.HasPhoto);
            }
        }

        /// <summary>
        /// upon user compliance selection if camera is selected add photo to gallery
        /// </summary>
        public void ConfirmPhoto()
        {
            if (_isPhoto && !_inspectable.HasPhoto)
            {
                _inspectable.HasPhoto = true;
                PhotoConfirmed?.Invoke(_photo);
            }
        }

        /// <summary>
        /// replaces the buttons behaviours with warnings
        /// </summary>
        private void EnableWarnings()
        {
            // Removing listeners from all other buttons
            onClose.ClearBtn();
            onIr.ClearBtn();
            onProbe.ClearBtn();
            onVisual.ClearBtn();
            onPhoto.ClearBtn();

            // Replace all listeners with DisplayWarning
            onClose.AddAction(ClosedOnConfirm);
            onProbe.AddAction(ProbeTempOnConfirm);
            onIr.AddAction(IRTempOnConfirm);
        }

        /// <summary>
        /// clears the buttons that have warnings and re-enables their standard behaviour
        /// </summary>
        private void ReEnableButtons()
        {
            // Removing listeners from all other buttons
            onClose.ClearBtn();
            onIr.ClearBtn();
            onProbe.ClearBtn();

            // Replace all listeners with DisplayWarning
            onClose.AddAction(CloseWindow);
            onProbe.AddAction(DisplayProbeTemp);
            onIr.AddAction(DisplayIRTemp);
            onVisual.AddAction(DisplayVisual);
            onPhoto.AddAction(TakePhoto);
        }

        /// <summary>
        ///     Called when the user is going to discard the photo they took. Warns the user that the photo will be discarded. Then closes the window on confirm.   
        /// </summary>
        private void ClosedOnConfirm()
        {
            DisplayWarning(CloseWindow);
        }

        /// <summary>
        ///     Called when the user is going to discard the photo they took. Warns the user that the photo will be discarded. Then opens the IR reading.   
        /// </summary>
        private void IRTempOnConfirm()
        {
            DisplayWarning(DisplayIRTemp);
        }

        /// <summary>
        ///     Called when the user is going to discard the photo they took. Warns the user that the photo will be discarded. Then opens the probe reading.
        /// </summary>
        private void ProbeTempOnConfirm()
        {
            DisplayWarning(DisplayProbeTemp);
        }

        /// <summary>
        ///     Displays a warning modal to the user before proceeding with the action.
        /// </summary>
        /// <param name="confirmAction"></param>
        public void DisplayWarning(Action confirmAction = null)
        {
            _discardPhotoModal.SetPrimaryAction(() =>
            {
                ReEnableButtons();
                HideTools();
                confirmAction?.Invoke();
            });
            HideToast?.Invoke();

            CreateModal?.Invoke(_discardPhotoModal);
        }

        /// <summary>
        ///     This handles the camera flash effect
        /// </summary>
        /// <returns></returns>
        private IEnumerator FlashRoutine()
        {
            _flash.AddToClassList("FlashEffect");

            yield return new WaitForSeconds((float)0.2);

            _flash.RemoveFromClassList("FlashEffect");
        }



        /// <summary>
        ///     This method is for building the toast message for the inspection report after the inspection is completed
        /// </summary>
        /// <param name="compliancy">bool compliancy, true compliant/false non-compliant</param>
        /// <param name="tool">the tool used</param>
        /// <param name="name">The name of the inspectable object.</param>
        /// <returns>a string with the correct message</returns>
        private string BuildReportMessage(bool compliancy, Tools tool, string name)
        {
            string toolReadingStr;
            switch (tool)
            {
                case Tools.IRThermometer:
                    toolReadingStr = "IR Reading";
                    break;
                case Tools.ProbeThermometer:
                    toolReadingStr = "Probe Reading";
                    break;
                case Tools.Visual:
                    toolReadingStr = _isPhoto ? "Visual Inspection with Photo" : "Visual Inspection";
                    break;
                default:
                    toolReadingStr = "Inspection";
                    break;
            }

            return $"{toolReadingStr} of '{name}' reported as {(compliancy ? "compliant" : "non-compliant")}.";
        }
    }
}
