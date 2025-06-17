using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace VARLab.PublicHealth
{
    /// <summary>
    /// This will act as a manager for the photos that are taken, this will hold the list of all images and the method to take a photo.
    /// </summary>
    public class ImageManager : MonoBehaviour
    {
        //properties
        public Dictionary<string, InspectablePhoto> Photos;
        public InspectablePhoto TempPhoto;
        private RenderTexture _screenTexture;

        //image resolution
        public const int ResWidth = 640;
        public const int ResHeight = 480;
        private const int RenderDepth = 16;

        /// <summary><see cref="SaveDataSupport.SavePhotos"/></summary>
        public UnityEvent<Dictionary<string, InspectablePhoto>> SavePhotos;

        /// <summary><see cref="InspectionWindow.SetPhoto(InspectablePhoto)"/></summary>
        public UnityEvent<InspectablePhoto> TempImageTaken;

        /// <summary><see cref="ActivityLogManager.LogPhotoDeleted(string)"/></summary>
        public UnityEvent<string> LogPhotoDeleted;

        public void Awake()
        {
            _screenTexture = new RenderTexture(ResWidth, ResHeight, RenderDepth);
            Photos = new Dictionary<string, InspectablePhoto>();
        }

        public void TriggerPhotoSave()
        {
            SavePhotos?.Invoke(Photos);
        }

        /// <summary>
        /// This is added as a listener to the event <see cref="InspectionWindow.PhotoConfirmed"/>
        /// </summary>
        /// <param name="photo"> the photo to add </param>
        public void AddPhotoToGallery(InspectablePhoto photo)
        {
            Photos.Add(photo.Id, photo);
            SavePhotos?.Invoke(Photos);
        }

        /// <summary>
        /// This is the method that will be called if we need a temporary image to display
        /// </summary>
        /// <param name="inspectable"></param>
        public void TakeTempPhoto(InspectableObject inspectable)
        {
            //here should pull the information necessary from the inspectable (like the camera taking the shot, location, etc)
            Camera cam = inspectable.CameraForPhoto;

            if (cam == null)// to prevent null refs in tests
            {
                return;
            }

            //code for taking the screen capture from a given camera
            cam.targetTexture = _screenTexture;
            RenderTexture.active = _screenTexture;
            cam.Render();
            Texture2D renderedTexture = new(ResWidth, ResHeight);
            renderedTexture.ReadPixels(new Rect(0, 0, ResWidth, ResHeight), 0, 0);
            RenderTexture.active = null;
            byte[] byteArray = renderedTexture.EncodeToPNG();

            InspectablePhoto newCapture = new(byteArray, inspectable.InspectableObjectID, inspectable.Location, TimerManager.Instance.GetElapsedTime());
            TempPhoto = newCapture;

            TempImageTaken?.Invoke(TempPhoto);
            Destroy(renderedTexture);
        }

        //NOTE MAY BE OBSELETE AFTER REFACTOR BUT I'M KEEPING IT 
        /// <summary>
        /// This is called when an inspection is made, and checks for tool before taking a photo
        /// </summary>
        /// <param name="tool"> selected tool </param>
        /// <param name="inspectable"> hte object to take a photo of </param>
        public void TakePhotoForInspection(InspectableObject inspectable)
        {
            if (inspectable.HasPhoto) return;

            //here should pull the information necessary from the inspectable (like the camera taking the shot, location, etc)
            Camera cam = inspectable.CameraForPhoto;

            //code for taking the screen capture from a given camera
            cam.targetTexture = _screenTexture;
            RenderTexture.active = _screenTexture;
            cam.Render();
            Texture2D renderedTexture = new(ResWidth, ResHeight);
            renderedTexture.ReadPixels(new Rect(0, 0, ResWidth, ResHeight), 0, 0);
            RenderTexture.active = null;
            byte[] byteArray = renderedTexture.EncodeToPNG();


            //add image to list
            string timeStamp = TimerManager.Instance.GetElapsedTime();
            InspectablePhoto newCapture = new(byteArray, inspectable.InspectableObjectName, inspectable.Location, timeStamp);
            Photos.Add(newCapture.Id, newCapture);

            inspectable.HasPhoto = true;

            SavePhotos?.Invoke(Photos);

            Destroy(renderedTexture);
        }

        /// <summary>
        /// This is used on load, disregarding tool selection and just taking photos of objects added as a listener to <see cref="InspectableObject.TakePhotoOfObj"/>
        /// </summary>
        /// <param name="inspectable"></param>
        public void TakePhotoForLoad(InspectableObject inspectable, string time)
        {
            if (inspectable.HasPhoto) return;
            //toggle the objects on/off
            inspectable.ToggleObjects();
            //here should pull the information necessary from the inspectable (like the camera taking the shot, location, etc)
            Camera cam = inspectable.CameraForPhoto;

            SwapObjectForLoad TempObject = inspectable.GetComponent<SwapObjectForLoad>();
            if (TempObject != null)
            {
                TempObject.SwapObjects();
                cam = TempObject.CameraForLoad;
                TempObject.GetComponent<InspectableObject>().CurrentObjState = inspectable.CurrentObjState;
            }
            //code for taking the screen capture from a given camera
            cam.targetTexture = _screenTexture;
            RenderTexture.active = _screenTexture;
            cam.Render();
            Texture2D renderedTexture = new(ResWidth, ResHeight);
            renderedTexture.ReadPixels(new Rect(0, 0, ResWidth, ResHeight), 0, 0);
            RenderTexture.active = null;
            byte[] byteArray = renderedTexture.EncodeToPNG();
            Destroy(renderedTexture);

            //add image to list
            InspectablePhoto newCapture = new(byteArray, inspectable.InspectableObjectID, inspectable.Location, time);
            Photos.Add(newCapture.Id, newCapture);

            inspectable.HasPhoto = true;
            //return the toggled objects to original state
            inspectable.ToggleObjects();

            if (TempObject != null)
            {
                TempObject.SwapObjects();
            }
        }


        /// <summary>
        ///     This method will delete a photo from the dictionary
        /// </summary>
        public void DeletePhoto(string id)
        {
            LogPhotoDeleted?.Invoke(ParseNameFromID(id));
            Photos.Remove(id);
            SavePhotos?.Invoke(Photos);
        }

        /// <summary>
        ///     This method will parse the name from the id
        /// </summary>
        /// <param name="id">id of the inspectable</param>
        /// <returns>the name of the inspectable</returns>
        public string ParseNameFromID(string id)
        {
            string[] split = id.Split('_');
            if (split.Length == 1)
            {
                return split[0];
            }
            else
            {
                return split[1];
            }
        }

        /// <summary>
        ///     This method will be a listener for when the actual photo is taken and will update the timestamp on the photo object
        ///     Note: When this photo is taken, it is currently the temp photo. If the photo already exists, it will not be added to the dictionary and the timestamp will not be updated.
        ///     <see cref="InspectionWindow.UpdatePhotoTimeStamp"/>   
        /// </summary>
        public void UpdatePhotoTimeStamp(InspectablePhoto ip)
        {
            ip.TimeStamp = TimerManager.Instance.GetElapsedTime();
        }
    }
}
