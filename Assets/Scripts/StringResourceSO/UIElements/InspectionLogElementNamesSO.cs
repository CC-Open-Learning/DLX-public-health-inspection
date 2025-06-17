using UnityEngine;

namespace VARLab.PublicHealth
{
    [CreateAssetMenu(fileName = "ModalPopup", menuName = "ScriptableObjects/NonCompliance")]
    public class InspectionLogElementNamesSO : ScriptableObject
    {
        // Structure
        public string MainContainer = "LogContainer";
        public string EmptyLogContainer = "EmptyLogContainer";
        public string InspectionLogContainer = "InspectionLogContainer";
        public string Foldout = "Foldout";
        public string Row = "NonCompRow";
        public string ToolContainer = "ToolContainer";

        // Content
        public string TabContent = "TabContent";
        public string Location = "Location";
        public string Item = "Item";
        public string ToolPhoto = "Photo";
        public string ToolLabel = "ToolUsed";
        public string InfoBtn = "InfoTextButton";
        public string InfoIcon = "InfoIcon";
        public string DeleteButton = "DeleteButton";
        public string SortIcon = "Icon";


        //Sort buttons for inspection log
        public string AllBtnName = "AllBtn";
        public string CompliantBtnName = "CompliantBtn";
        public string NonCompliantBtnName = "NonCompliantBtn";

        //Row item content names
        public string CompliancyIcon = "CompliantImg";

        //Styles 
        public string ActiveBtnStyleSelector = "active-button";
        public string InactiveBtnStyleSelector = "inactive-button";
        public string ActiveIconColourSelector = "active-icon-colour";
        public string InactiveIconColourSelector = "inactive-icon-colour";
        public string TemplateContainerClass = "template-container";
    }
}
