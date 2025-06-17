using CORE_ExportTool;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;
using VARLab.Analytics;

namespace VARLab.PublicHealth
{
    public class DownloadManager : MonoBehaviour
    {
        [SerializeField, Tooltip("Reference to the Activity Log Manager")] private ActivityLogManager _activityLogManager;
        [SerializeField, Tooltip("Reference to the Inspectable Manager")] private InspectableManager _inspectableManager;
        [SerializeField, Tooltip("Reference to the Image Manager")] private ImageManager _imageManager;
        [SerializeField, Tooltip("Reference to the Scenario Manager")] private ScenarioManager _scenarioManager;
        [SerializeField, Tooltip("Reference to the strings SO")] private ExportSummaryStrings _exportSummaryStrings;

        //Consts
        private const string Facility = "Restaurant";
        private const string ActivityLogTitle = "ACTIVITY LOG";
        private const int ImageBatchSize = 150;

        private List<string> _compliantList = new();
        private List<string> _nonCompliantList = new();

        /// <see cref="DownloadAlert.DownloadTriggered"/>
        public UnityEvent DownloadTriggered;

        /// <summary>
        /// <see cref="InspectionSummaryBuilder.Download"/>
        /// </summary>
        public void DownloadFiles()
        {
            DownloadTriggered?.Invoke();
            AnalyticsHelper.UpdateSessionTotalTimePlayed();
#if UNITY_WEBGL && !UNITY_EDITOR
            StartCoroutine(DownloadCoroutine());
            CoreAnalytics.CustomEvent("Downloaded_Report", "", 0);
#endif
        }
        /// <summary>
        /// Coroutine that triggers all the downloads.
        /// </summary>
        /// <returns>Added a wait to space out the downloads and avoid security warnings.</returns>
        private IEnumerator DownloadCoroutine()
        {
            // Download the PDF first
            DownloadPDF();
            yield return new WaitForSeconds(2);
            // Download the zip file(s) with the photos after 2 seconds
            yield return StartCoroutine(DownloadAllPhotos());
        }

        /// <summary>
        /// Generates and downloads the PDF document.
        /// </summary>
        private void DownloadPDF()
        {
            // create the PDF document
            generatePdf.CreatePDFDocument();

            AddSummary();

            generatePdf.AddPage();

            AddCompliancesAndNonCompliances();

            AddActivityLog();
            // Get the logo string data
            string base64LogoData = GetLogoData("pdfLogo");
            // Add the header and footer to conclude pdf formatting
            generatePdf.AddHeader(DateTime.Now.ToString("ddd MMM d yyyy HH:mm:ss 'EST'"), base64LogoData, Fonts.TimesRoman, "black", 10, 72);
            generatePdf.AddFooter("Centre for Virtual Reality Innovation", Fonts.TimesRoman, 9, "black", Fonts.TimesRoman, 10, "black", 72);
            // Initiate download pdf
            generatePdf.DownloadPDF("Inspection Summary_" + GetLearnerName() + "_" +
            DateTime.Now.ToString("ddMMMyyyy") + " " + DateTime.Now.ToString("HH'H'mm'M'ss'S'") + ".pdf");
        }

        /// <summary>
        /// Function that dowloads all the photos of the gallery in a zip file in batches so that the memory is not overloaded
        /// when downloading a large number of photos. It assigns a zipId to each batch and adds the photo data to the zip file
        /// by sending a base64 string per photo data. It then names the zip file based on the total number of batches and downloads it.
        /// </summary>
        public IEnumerator DownloadAllPhotos()
        {
            // Exit if there are no photos to download
            if (!_imageManager.Photos.Any())
            {
                yield break;
            }
            // Stage the batching process
            int totalPhotos = _imageManager.Photos.Count;
            int totalBatches = Mathf.CeilToInt(totalPhotos / (float)ImageBatchSize);
            var photoKeys = _imageManager.Photos.Keys.ToArray();
            // Sort the photo keys Alphabetically
            Array.Sort(photoKeys, StringComparer.OrdinalIgnoreCase);
            // For each batch, create a zip file and add the photos to it
            for (int batchIndex = 0; batchIndex < totalBatches; batchIndex++)
            {
                string zipId = $"zip_{batchIndex}";
                GenerateZip.CreateZip(zipId);
                int start = batchIndex * ImageBatchSize;
                int end = Mathf.Min(start + ImageBatchSize, totalPhotos);
                // For every photo in the batch, add it to the zip file
                for (int i = start; i < end; i++)
                {
                    string key = photoKeys[i];
                    var photo = _imageManager.Photos[key];
                    string fileName = GetSafeFileName(photo.Id, photo.TimeStamp);

                    // Convert compressed data to Base64 string
                    string base64Data = Convert.ToBase64String(photo.Data);

                    // Add the photo to the zip file
                    GenerateZip.AddPhotoToZip(zipId, fileName, base64Data);
                }
                // Assign a file name for the zip file based on the total batches
                string zipFileName = totalBatches == 1 ? "Gallery.zip" : $"Gallery_{batchIndex + 1}.zip";
                GenerateZip.DownloadZipFile(zipId, zipFileName);
                // Wait for 2 seconds before downloading the next batch
                yield return new WaitForSeconds(2);
            }
        }

        /// <summary>
        /// Function that replaces any special characters in the file name with an underscore. This ensures that the
        /// image files have proper names and do not contain any invalid characters.
        /// </summary>
        /// <param name="id"> The id of the photo </param>
        /// <param name="timeStamp"> The timestamp for the photo </param>
        /// <returns> a safe file name string for the photo </returns>
        private string GetSafeFileName(string id, string timeStamp)
        {
            string format = ".png";
            string fileName = $"{id}_{timeStamp}{format}";
            Regex reg = new Regex("[*'\",&#^@:;+]");
            return reg.Replace(fileName, "_");
        }

        /// <summary>
        /// Function that build the summary that will be added to the PDF document
        /// </summary>
        private void AddSummary()
        {
            // Top line
            generatePdf.AddLine("round", 72, 72, 540, 72, "#44546a");

            // Public Health Inspection Title
            generatePdf.AddContentWithXAndYPositions(_exportSummaryStrings.SummaryStrings[0], 20, Fonts.TimesRoman, "center", "#44546a", 72, 87, 0.5f);

            // Bottom Line
            generatePdf.AddLine("round", 72, 140, 540, 140, "#44546a");

            // add scenario number
            generatePdf.AddContentWithXAndYPositions(_scenarioManager.GetScenarioName().Replace("0", "#"), 10.5f, Fonts.TimesRoman, "center", "black", 72, 170, 0);

            // add message
            generatePdf.AddContentWithXAndYPositions(_exportSummaryStrings.SummaryStrings[1], 12, Fonts.TimesItalic, "center", "black", 72, 195, 3.5f);

            // add inspection summary title
            generatePdf.AddTextWithUnderline(_exportSummaryStrings.SummaryStrings[2], Fonts.TimesBold, 16, 1.5f);

            // add inspection summary
            CreateSummaryList();
        }

        /// <summary>
        /// Function that builds the Activity log PDF and downloads it.
        /// This function still needs to be adjusted to match the document layout.
        /// </summary>
        public void AddActivityLog()
        {
            // Add the activity log title
            generatePdf.AddTextWithUnderline(ActivityLogTitle, Fonts.TimesBold, 16, 0.8f);
            // For every primary log, add the parent log and all the logs
            if (_activityLogManager != null && _activityLogManager.PrimaryLogs.Any())
            {
                foreach (PrimaryLog primary in _activityLogManager.PrimaryLogs)
                {
                    // Adds the primary log to the PDF with primary log formatting
                    generatePdf.AddContent(primary.ParentLog.LogContent, 12, Fonts.TimesBold, "left", "black");

                    foreach (var log in primary.logs)
                    {
                        // Adds each of the inspection logs with log formatting
                        generatePdf.AddContent(log.LogContent, 11, Fonts.TimesRoman, "left", "black");
                    }
                }
            }
        }

        /// <summary>
        /// Creates the summary lists with the details of the inspection.
        /// </summary>
        private void CreateSummaryList()
        {
            string tempString = "";
            int count = 0;
            string learnerName = GetLearnerName();

            for (int i = 3; i <= 9; i++)
            {
                //reset temp string
                tempString = "";

                tempString = _exportSummaryStrings.SummaryStrings[i] + " ";
                count = tempString.Length;

                switch (i)
                {
                    case 3:
                        tempString += DateAndTime.GetDateString();
                        break;
                    case 4:
                        tempString += learnerName;
                        break;
                    case 5:
                        tempString += Facility;
                        break;
                    case 6:
                        tempString += _inspectableManager.GetNonComplianceCount();
                        break;
                    case 7:
                        tempString += _inspectableManager.GetComplianceCount();
                        break;
                    case 8:
                        tempString += $"{PHISceneManager.Instance.PoiManager.VisitedPOIs.Count} / {PHISceneManager.Instance.PoiManager.InspectablePOIsCount}";
                        break;
                    case 9:
                        tempString += TimerManager.Instance.GetElapsedTime();
                        break;
                    default:
                        break;
                }

                generatePdf.AddSlicedStringFontAndColour(tempString, count, tempString.Length, Fonts.TimesBold, Fonts.TimesRoman, "black", "black", 0.8f);
            }
        }

        /// <summary>
        /// Function that gets the pdf logo by returning it as a base64 string.
        /// If the resource is not found or an error occurs, it returns an empty string and logs the error.
        /// </summary>
        /// <param name="resourceName">The name of the resource to load.</param>
        /// <returns>A base64 string representing the logo data, or an empty string if there is an error.</returns>
        public string GetLogoData(string resourceName)
        {
            // Check if the resourceName is valid
            if (string.IsNullOrEmpty(resourceName))
            {
                Debug.LogError("GetLogoData: Resource name is null or empty.");
                return string.Empty;
            }

            try
            {
                // Load the texture from Resources
                Texture2D logoTexture = Resources.Load<Texture2D>(resourceName);

                // Check if the texture was loaded successfully
                if (logoTexture == null)
                {
                    Debug.LogError($"GetLogoData: Unable to load resource '{resourceName}' from Resources.");
                    return string.Empty;
                }

                // Convert the texture to a byte array
                byte[] logoData = logoTexture.EncodeToPNG();

                // Check if the byte array is valid
                if (logoData == null || logoData.Length == 0)
                {
                    Debug.LogError("GetLogoData: Failed to encode texture to PNG.");
                    return string.Empty;
                }

                // Convert byte array to base64
                string base64LogoData = Convert.ToBase64String(logoData);
                return base64LogoData;
            }
            catch (Exception ex)
            {
                // Catch any unexpected exceptions and log the error
                Debug.LogError($"GetLogoData: An error occurred while getting logo data. Error: {ex.Message}");
                return string.Empty;
            }
        }

        private void AddCompliancesAndNonCompliances()
        {
            GetCompliantAndNonCompliantList();

            // Add the compliance header
            generatePdf.AddTextWithUnderline("INSPECTION LOG: COMPLIANCES", Fonts.TimesBold, 16, 1);

            // Set font fot the list
            generatePdf.SetFont(Fonts.TimesRoman, 12);

            // Add the compliance list
            generatePdf.AddNumberedListItem(_compliantList);

            // Page break
            generatePdf.AddPage();

            // Add the non-compliance header
            generatePdf.AddTextWithUnderline("INSPECTION LOG: NON-COMPLIANCES", Fonts.TimesBold, 16, 1);

            // Set font for the numbered list
            generatePdf.SetFont(Fonts.TimesRoman, 12);

            // Add the compliance list
            generatePdf.AddNumberedListItem(_nonCompliantList);

            generatePdf.AddPage();
        }

        /// <summary>
        /// Creates a list of compliant logs and a list of non-compliant logs.
        /// </summary>
        private void GetCompliantAndNonCompliantList()
        {
            foreach (var inspection in _inspectableManager.Inspections)
            {
                string location = _inspectableManager.GetInspectableObjectLocation(inspection.Key);
                string name = inspection.Value.InspectableObjectName;

                foreach (var log in inspection.Value.InspectionEvidences)
                {
                    var info = log.Value.ToolName == "Visual" ? "" : $" - {log.Value.Reading}";

                    if (log.Value.IsCompliant)
                    {
                        string tempString = $"{location} - {name} - {log.Value.ToolName}{info}";

                        _compliantList.Add(tempString);
                    }
                    else if (!log.Value.IsCompliant)
                    {
                        string tempString = $"{location} - {name} - {log.Value.ToolName}{info}";

                        _nonCompliantList.Add(tempString);
                    }
                }
            }
            _compliantList.Sort();
            _nonCompliantList.Sort();
        }

        /// <summary>
        /// Function that gets the learner name from the ScormIntegrator. If the learner name is empty, it 
        /// returns "Instructor". It also removes any trailing or leading spaces from the name.
        /// </summary>
        /// <returns>The learner name in the format "FirstName LastName".</returns>
        public string GetLearnerName()
        {
            string learnerName = string.IsNullOrEmpty(ScormIntegrator.LearnerName)
                ? "Instructor"
                : ScormIntegrator.LearnerName;

            // Split the name on the comma, and trim any leading/trailing spaces
            string[] names = learnerName.Split(',');

            string firstName = names.Length > 1 ? names[1].Trim() : string.Empty;
            string lastName = names[0].Trim();

            learnerName = $"{firstName} {lastName}".Trim(); // Combine the trimmed names

            return learnerName;
        }
    }
}