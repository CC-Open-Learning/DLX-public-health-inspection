using System.Text.RegularExpressions;

namespace VARLab.PublicHealth
{
    /// <summary>
    ///     This static class is used to set the Points of Interest in the game. Inspectable objects will be placed in these points of interest and set to one of these values in the editor.
    /// </summary>
    public static class POIList
    {
        public enum POIName
        {
            None = 0,
            BackKitchen = 1,
            // WalkInCoolerExterior = 2,
            WalkInCooler = 3,
            Pantry = 4,
            Reception = 5,
            Bar = 6,
            Kitchen = 7,
            Dining = 8,
            Office = 9,
            ServiceHallway = 10,
            Dishwashing = 11,
            WalkInFreezer = 12,
            BroomCloset = 13,
            EmployeeArea = 14,
            // Bathroom3 = 15,
            Bathroom1 = 16,
            Bathroom2 = 17,
        }

        public static string GetPOIName(string name)
        {
            name = Regex.Replace(name, @"\d+", " #$0");
            name = Regex.Replace(name, @"((?<=\p{Ll})\p{Lu})|((?!\A)\p{Lu}(?>\p{Ll}))", " $0");
            return name.Replace("Walk In", "Walk-in");
        }
    }
}
