using System.Collections.Generic;

namespace VARLab.PublicHealth
{
    /// <summary>
    ///     This class will be the single source of truth for the inspection data
    /// </summary>
    public class Inspection
    {
        public string InspectableObjectName;

        public Dictionary<string, Evidence> InspectionEvidences; // The key is the tool used for the inspection

        public Inspection() { }

        /// <summary>
        ///     Constructor for the inspection
        /// </summary>
        /// <param name="toolUsed">tool used for inspection</param>
        /// <param name="isCompliant">result of inspection</param>
        /// <param name="obj">the object inspected on</param>
        public Inspection(Tools toolUsed, bool isCompliant, InspectableObject obj)
        {
            InspectableObjectName = obj.InspectableObjectName;
            InspectionEvidences = new Dictionary<string, Evidence>
            {
                { toolUsed.ToString(), new Evidence(toolUsed, isCompliant, obj) }
            };

        }

        /// <summary>
        /// Adds evidence to the inspection
        /// </summary>
        /// <param name="toolUsed">The tool used</param>
        /// <param name="isCompliant">Inspection result</param>
        public void AddInspectionEvidence(Tools toolUsed, bool isCompliant, InspectableObject obj)
        {
            InspectionEvidences.Add(toolUsed.ToString(), new Evidence(toolUsed, isCompliant, obj));
        }

        /// <summary>
        /// Removes evidence from the inspection
        /// </summary>
        /// <param name="toolUsed">The tool used</param>
        public void RemoveInspectionEvidence(string toolUsed)
        {
            InspectionEvidences.Remove(toolUsed);
        }

        /// <summary>
        /// Check if the inspection contains evidence
        /// </summary>
        public bool ContainsInspectionEvidence(string toolUsed)
        {
            return InspectionEvidences.ContainsKey(toolUsed);
        }

    }
}
