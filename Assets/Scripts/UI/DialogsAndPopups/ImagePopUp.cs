using UnityEngine;
using UnityEngine.UIElements;

namespace VARLab.PublicHealth
{
    /// <summary>
    /// This class is used to manage the pop up that displays an image when a photo is clicked on or the view photo button is clicked on.
    /// </summary>
    public class ImagePopUp : MonoBehaviour
    {
        public UIDocument RootDoc;
        private Button _closeBtn;
        private VisualElement _image;
        private Label _location;
        private Label _name;
        private Label _time;
        private VisualElement _root;
        private Texture2D tex;

        public ImageManager ImageManager;
        public InspectablePhoto CurrentPhoto;

        public void Start()
        {
            if (RootDoc == null)
            {
                RootDoc = GetComponent<UIDocument>();
            }

            _root = RootDoc.rootVisualElement; //this is to type less

            //get all references
            _closeBtn = _root.Q<Button>();
            _image = _root.Q<VisualElement>("Image");
            _location = _root.Q<Label>("LocationLbl");
            _name = _root.Q<Label>("NameLbl");
            _time = _root.Q<Label>("TimeLbl");

            _closeBtn.clicked += CloseWindow;

            _root.style.display = DisplayStyle.None; //this is not a CloseWindow call incase we want to add other functionality to close window
        }

        /// <summary>
        /// This is called to open the pop up to display an image
        /// </summary>
        /// <param name="name"> The name of the object that is associated with it's photo </param>
        public void OpenImage(string name)
        {
            ImageManager.Photos.TryGetValue(name, out CurrentPhoto);
            if (CurrentPhoto == null)
            {
                Debug.Log("Image pop up could not find photo matching name!");
                return;
            }
            SetImage();
            SetDetails();
            _root.style.display = DisplayStyle.Flex;
        }

        /// <summary>
        /// Sets the background image of the pop up
        /// </summary>
        private void SetImage()
        {
            tex = new(ImageManager.ResWidth, ImageManager.ResHeight);
            tex.LoadImage(CurrentPhoto.Data);
            _image.style.backgroundImage = tex;
        }

        /// <summary>
        /// sets the labels of the photos information
        /// </summary>
        private void SetDetails()
        {
            _location.text = CurrentPhoto.Location;
            _time.text = CurrentPhoto.TimeStamp;

            //parse the name to remove the location from the id.
            _name.text = ImageManager.ParseNameFromID(CurrentPhoto.Id);
        }

        /// <summary>
        /// This is called to close the pop up
        /// </summary>
        public void CloseWindow()
        {
            _root.style.display = DisplayStyle.None;
            Destroy(tex);
        }
    }
}
