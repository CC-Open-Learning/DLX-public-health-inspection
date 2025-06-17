using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace VARLab.PublicHealth
{
    /// <summary>
    ///     This class is used to build the gallery portion of the inspection review window. It will display all photos taken during the inspection.
    /// </summary>
    public class GalleryBuilder : MonoBehaviour
    {

        [SerializeField, Tooltip("Reference to the inspectable manager in the scene")] private InspectableManager _inspectableManager;

        [SerializeField, Tooltip("Reference to the image manager in the scene")] private ImageManager _imageManager;

        //Gallery UI Document and Visual Tree References
        [SerializeField] private UIDocument _inspectionReviewWindow;
        [SerializeField] private VisualTreeAsset _mainGallery;
        [SerializeField] private VisualTreeAsset _galleryRow;
        [SerializeField] private VisualTreeAsset _galleryPicture;

        //Modal creation SerializedObjects
        [SerializeField] private ModalPopupSO _compliantPhotoDelete;
        [SerializeField] private ModalPopupSO _noncompliantPhotoDelete;

        //Element Names SerializedObjects
        [SerializeField] private GalleryTabElementNamesSO _elementNames;

        //Dictionary to hold location to photos
        public TemplateContainer _galleryContainer;
        private ScrollView _mainGalleryList;

        //VisualElement references
        private VisualElement _emptyGalleryContainer;
        private VisualElement _galleryViewContainer;

        //Sort Buttons + associated flags
        private Button _allBtn;
        private Button _sortCompliant;
        private bool _compliantFlag;
        private Button _sortNonCompliant;
        private bool _noncompliantFlag;

        //List that will hold the textures created for the gallery
        private List<Texture2D> textures = new List<Texture2D>();

        //ToastColor
        private Color _toastColor = new(9f / 255, 117f / 255, 56 / 255f);

        //Dictionary to hold location to photos
        private Dictionary<string, List<InspectablePhoto>> _locationToPhotos;

        //Unity Events
        /// <summary><see cref="InspectableManager.OnPhotoDeleted(string)"/></summary>
        public UnityEvent<string> DeletePhoto;

        /// <summary><see cref="ImageManager.TriggerPhotoSave"/></summary>
        public UnityEvent SaveGallery;

        /// <summary><see cref="ToastManager.DisplayToast(string, Color)"/></summary>
        public UnityEvent<string, Color> CreateToast;

        /// <summary><see cref="ProgressBarBuilder.UpdateNonComplianceText"/></summary>
        public UnityEvent UpdateProgress;

        /// <summary><see cref="InspectableManager.RemoveInspection(string, string)"/> </summary>
        public UnityEvent<string, string> DeleteInspection;

        /// <summary><see cref="ModalPopupBuilder.HandleDisplayModal(ModalPopupSO)"/>/// </summary>
        public UnityEvent<ModalPopupSO> CreateModal;

        /// <summary>This bool is set to indicate if the user would like to delete the non-compliance tied with the photo</summary>
        private bool _deleteNonComplianceFlag;

        public ImagePopUp ImagePopUp;

        /// <summary>
        /// This struct is used to store the data for the delete button, it is used to store the name of the image, the location of the image, and the non-compliance log if it exists.
        /// Null if it does not exist.
        /// </summary>
        private struct BtnData
        {
            public string Name;
            public string Location;
        }

        /// <summary>This method is called when the script instance is being loaded.</summary>
        public void Awake()
        {
            _locationToPhotos = new();
            _deleteNonComplianceFlag = false;

            ResetFlags();
        }

        public void ResetFlags()
        {
            _compliantFlag = false;
            _noncompliantFlag = false;
        }

        private void SetupReferences(TemplateContainer container)
        {
            //once instantiated get references here
            _sortCompliant = container.Q<Button>("CompliantBtn");
            _sortNonCompliant = container.Q<Button>("NonCompliantBtn");
            _allBtn = container.Q<Button>("AllBtn");

            List<Button> buttons = new() { _allBtn, _sortCompliant, _sortNonCompliant };

            _allBtn.clicked += () =>
            {
                _mainGalleryList.Clear();
                DestroyTextures();
                _noncompliantFlag = false;
                _compliantFlag = false;
                BuildAllRows();
                gameObject.GetComponent<InspectionLogBuilder>().HandleButtonStyling(buttons, 0);
            };

            _sortCompliant.clicked += () =>
            {
                _mainGalleryList.Clear();
                DestroyTextures();
                _noncompliantFlag = false;
                _compliantFlag = true;
                BuildAllRows();
                gameObject.GetComponent<InspectionLogBuilder>().HandleButtonStyling(buttons, 1);
            };

            _sortNonCompliant.clicked += () =>
            {
                _mainGalleryList.Clear();
                DestroyTextures();
                _compliantFlag = false;
                _noncompliantFlag = true;
                BuildAllRows();
                gameObject.GetComponent<InspectionLogBuilder>().HandleButtonStyling(buttons, 2);
            };
        }


        /// <summary>
        /// This is the "main" method for building the gallery, it acts as the entry point for building the gallery portion of the window.
        /// </summary>
        public void BuildGallery()
        {
            //get reference to the tab window element
            VisualElement tabWindowElement = _inspectionReviewWindow.rootVisualElement.Q(_elementNames.TabContent);

            //build main gallery and get reference to ListView
            try
            {
                _galleryContainer = _mainGallery.Instantiate();
                SetupReferences(_galleryContainer);
            }
            catch (Exception ex)
            {
                Debug.Log("Instantiate exception: " + ex.Message + " trying again");
                _galleryContainer = _mainGallery.Instantiate();
                SetupReferences(_galleryContainer);
            }

            _mainGalleryList = _galleryContainer.Q(_elementNames.MainGalleryListName) as ScrollView;
            _mainGalleryList.mouseWheelScrollSize = 1500;
            _emptyGalleryContainer = _galleryContainer.Q(_elementNames.EmptyGalleryContainer);
            _galleryViewContainer = _galleryContainer.Q(_elementNames.GalleryContainer);

            _galleryContainer.AddToClassList(_elementNames.TemplateContainerClass);

            //if there are no photos, display the empty gallery message
            if (_imageManager.Photos.Count == 0)
            {
                DisplayEmptyGalleryMessage();
            }
            else
            {
                HideEmptyGalleryMessage();
                BuildAllRows();
            }

            //add container to window element
            tabWindowElement.Add(_galleryContainer);
        }


        /// <summary>
        /// Display the empty log message
        /// </summary>
        private void HideEmptyGalleryMessage()
        {
            //If there are logs, hide the empty log container
            _emptyGalleryContainer.style.display = DisplayStyle.None;

            //Display the inspection log container
            _galleryViewContainer.style.display = DisplayStyle.Flex;
        }

        /// <summary>
        /// Display the empty log message
        /// </summary>
        private void DisplayEmptyGalleryMessage()
        {
            // If there are no logs, display the empty log container
            _emptyGalleryContainer.style.display = DisplayStyle.Flex;

            //Hide the inspection log container
            _galleryViewContainer.style.display = DisplayStyle.None;
        }

        /// <summary>
        /// This method (re)sets up the location to photos dictionary and populates the rows
        /// </summary>
        private void BuildAllRows()
        {
            PopulateLocationPhotoDict();

            foreach (string location in _locationToPhotos.Keys) //for build
            {
                TemplateContainer row = PopulateRow(location);
                if (row != null) { _mainGalleryList.Add(row); }
            }
        }


        /// <summary>
        /// This function is used to create a "Row" for a location and populate it with all images associated with that location
        /// </summary>
        /// <param name="location"> This is the Enum of the location used to index the location->photos dictionary </param>
        /// <returns> The "row container" that will have the label and a collection of photos, which can be added to the Main gallery element, will return null if there are no photos to display </returns>
        private TemplateContainer PopulateRow(string location)
        {
            TemplateContainer rowContainer = _galleryRow.Instantiate();
            VisualElement imageRow = rowContainer.Q(_elementNames.RowScroll);

            Label locationName = rowContainer.Q(_elementNames.RowLabel) as Label;
            locationName.text = _locationToPhotos[location][0].Location.ToString();

            foreach (InspectablePhoto photo in _locationToPhotos[location])
            {
                //here we check sort flags and skip if needed
                if (_compliantFlag || _noncompliantFlag)
                {
                    Inspection found = _inspectableManager.Inspections[photo.Id];
                    Evidence evidence = null;
                    if (found.InspectionEvidences.ContainsKey(Tools.Visual.ToString()) && found.InspectionEvidences.TryGetValue(Tools.Visual.ToString(), out evidence))
                    {
                        if ((_compliantFlag && !evidence.IsCompliant) || (_noncompliantFlag && evidence.IsCompliant)) continue;
                    }
                }


                //instantiate img container
                TemplateContainer img = _galleryPicture.Instantiate();

                //take photo and load texture top background of element
                Texture2D tex = new(ImageManager.ResWidth, ImageManager.ResHeight);
                tex.LoadImage(photo.Data);
                textures.Add(tex);
                img.Q(_elementNames.PhotoPicture).style.backgroundImage = tex;

                img.Q<Button>("ImageBtn").clicked += () => ImagePopUp.OpenImage(photo.Id);

                //set up delete button
                Button deleteBtn = img.Q(_elementNames.PhotoTrashBtn) as Button;
                deleteBtn.visible = true;

                //setup name/timestamp
                Label name = img.Q(_elementNames.ImgLabel) as Label;

                //Split the string and store the value after the underscore
                name.text = _imageManager.ParseNameFromID(photo.Id);

                Label time = img.Q<Label>(_elementNames.PhotoTimestamp);
                time.text = photo.TimeStamp;

                deleteBtn.userData = new BtnData
                {
                    Name = photo.Id,
                    Location = photo.Location,
                };
                //print the properties of the button
                BtnData btnData = (BtnData)deleteBtn.userData;

                //add listener to delete button
                deleteBtn.clicked += () =>
                {
                    _noncompliantPhotoDelete.SetPrimaryAction(() => DeleteItems(btnData.Name, deleteBtn));
                    _noncompliantPhotoDelete.SetToggleAction((val) => _deleteNonComplianceFlag = val);
                    CreateModal?.Invoke(_noncompliantPhotoDelete);
                };

                imageRow.Add(img);
            }
            // check if there are any photos in imagerow to display
            if (imageRow.childCount <= 0) rowContainer = null;
            return rowContainer;
        }

        /// <summary>
        ///     This iterates over the ImageManagers Photo list and creates a dictionary of locations with the associated list of photos for that location.
        /// </summary>
        private void PopulateLocationPhotoDict()
        {
            _locationToPhotos.Clear();
            //get a dictionary of the locations -> list of photos
            foreach (KeyValuePair<string, InspectablePhoto> kvp in _imageManager.Photos)
            {
                if (_locationToPhotos.ContainsKey(kvp.Value.Location))
                {
                    _locationToPhotos[kvp.Value.Location].Add(kvp.Value);
                }
                else
                {
                    _locationToPhotos.Add(kvp.Value.Location, new List<InspectablePhoto>() { kvp.Value });
                }
            }
        }

        /// <summary>
        ///     This method will delete the photo selected from the gallery and update the gallery.
        /// </summary>
        private void DeletePhotoSelected(string name)
        {
            UpdateGallerySave(name);

            _mainGalleryList.Clear();
            DestroyTextures();
            BuildAllRows();

            if (_imageManager.Photos.Count == 0)
            {
                DisplayEmptyGalleryMessage();
            }
        }

        /// <summary>
        ///     This function will be called when the user presses confirm on the confirm dialog, it will delete the photo and the non-compliance if the user has selected to do so.
        /// </summary>
        /// <param name="objName">the object name</param>
        /// <param name="btn">the button that has all of the data stored within it</param>
        private void DeleteItems(string objName, Button btn)
        {
            BtnData btnData = (BtnData)btn.userData;

            DeletePhotoSelected(objName);

            if (_deleteNonComplianceFlag)
            {
                DeleteInspection?.Invoke(btnData.Name, Tools.Visual.ToString());
                UpdateProgress?.Invoke();
                CreateToast?.Invoke("Photo and Inspection Deleted", _toastColor);
            }
            else
            {
                CreateToast?.Invoke("Photo Deleted", _toastColor);
            }


            //reset flag after deleting. 
            _deleteNonComplianceFlag = false;
        }


        /// <summary>
        ///     Listens for the event to update the gallery called from <see cref="InspectionLogBuilder.DeleteLog""/>
        /// </summary>
        public void UpdateGallerySave(string name)
        {
            _imageManager.DeletePhoto(name);
            DeletePhoto?.Invoke(name); //used by inspectable manager 
        }

        public void DestroyTextures()
        {
            if (textures.Count > 0)
            {
                foreach (var texture in textures)
                {
                    Destroy(texture);
                }
            }
        }
    }
}
