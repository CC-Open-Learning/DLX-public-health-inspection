using System;

namespace VARLab.PublicHealth
{
    /// <summary>
    ///     This class contains the progress bar data and methods to update the progress bar that is displayed in the inspection review screen
    /// </summary>
    public class ProgressBar
    {
        /// <summary>
        ///     String to be displayed in the progress bar hold the current progress of the player vs the total locations 
        /// </summary>
        public string LocationProgress = "";

        /// <summary>
        ///     String to be displayed in the progress bar holding the current amount of non-compliance logs made by the player
        /// </summary>
        public string NonComplianceProgress = "";

        /// <summary>
        ///     A string to hold the time span in a correct format 
        /// </summary>
        public string TimeProgressString;


        /// <summary>
        ///     This method is used to update the progress bar
        /// </summary>
        /// <param name="poiCount">total number of POIs</param>
        /// <param name="poiVisitedCount">total number of Visited POIs</param>
        /// <param name="nonCompliancesMadeCount">Total number of nonCompliances made</param>
        /// <param name="timeSpan">total time the player has had, in the form of a timespan</param>
        public void UpdateProgress(int poiCount, int poiVisitedCount, int nonCompliancesMadeCount, TimeSpan timeSpan)
        {
            //get the total number of locations and the number of locations visited by the player
            LocationProgress = poiVisitedCount.ToString() + "/" + poiCount.ToString();

            //get the total number of non-compliance logs and the number of non-compliance logs created by the player
            NonComplianceProgress = nonCompliancesMadeCount.ToString();

            //convert the time span to a string
            TimeProgressString = ConvertTimeSpanToString(timeSpan);
        }


        /// <summary>
        ///     This method is used to convert a time span to a string
        ///     Note: If the time reaches 1hr, seconds will not be converted. Ex. 1hr 2mins 3secs will be "1hr 2mins"
        /// </summary>
        /// <param name="timeSpan">The timespan that will be converted.</param>
        /// <returns>A formatted string of the time with the wording of hr(s)/min(s)/sec(s)</returns>
        public string ConvertTimeSpanToString(TimeSpan timeSpan)
        {
            string returnString = "";

            //Formatting the time span to be displayed in the progress bar
            if (timeSpan.Hours > 0)
                returnString += (returnString != "" ? " " : "") + timeSpan.Hours.ToString() + " hr" + (timeSpan.Hours > 1 ? "s" : "");

            if (timeSpan.Minutes > 0)
                returnString += (returnString != "" ? " " : "") + timeSpan.Minutes.ToString() + " min" + (timeSpan.Minutes > 1 ? "s" : "");

            if (timeSpan.Seconds > 0 && timeSpan.Hours <= 0)
                returnString += (returnString != "" ? " " : "") + timeSpan.Seconds.ToString() + " sec" + (timeSpan.Seconds > 1 ? "s" : "");

            return returnString;
        }
    }
}
