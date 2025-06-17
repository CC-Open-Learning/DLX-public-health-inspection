namespace VARLab.PublicHealth
{
    /// <summary>
    /// This is a class to represent a taken image/screen capture when the learner wants to take a photo of an object
    /// it will contain any necessary information for the image itself and any methods tied to images.
    /// </summary>
    public class InspectablePhoto
    {
        //Properties
        public byte[] Data;
        public string Id; //the id will be associated with the inspectable object as a unique identifier
        public string Location;
        public string TimeStamp;


        public InspectablePhoto(byte[] data, string id, string location, string TimeStamp)
        {
            this.Data = data;
            this.Id = id;
            this.Location = location;
            this.TimeStamp = TimeStamp;
        }

    }
}
