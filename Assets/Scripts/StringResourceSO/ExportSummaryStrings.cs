using System.Collections.Generic;
using UnityEngine;

namespace VARLab.PublicHealth
{
    [CreateAssetMenu(menuName = "PHI/Scriptable Objects/ExportSummaryStrings")]
    public class ExportSummaryStrings : ScriptableObject
    {
        public List<string> SummaryStrings = new List<string>();

    }
}
