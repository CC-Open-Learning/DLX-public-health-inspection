namespace VARLab.PublicHealth
{
    public interface IActivityLogManager
    {
        public void LogPrimaryEvent(POI poi);
        public void AddToPrimaryList(string stub);
        public void LogInspection(Tools tools, InspectableObject inspectableObject);
        public void LogCompliancy(InspectableObject inspectableObject, Evidence evidence);

    }
}
