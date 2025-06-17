using System.Collections;

namespace VARLab.PublicHealth
{
    /// <summary>
    /// This is an interface for mocking out the CloudSave class to prevent pinging of the API
    /// </summary>
    public interface ICloudSaving
    {
        public void Initialize();
        public void Save();
        public void Load();
        public void Delete();
        public IEnumerator _Delete();
        public bool? LoadSuccess { get; set; }

        public bool HasLoaded { get; set; }

        public string LoadData { get; set; }
    }
}
