using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VARLab.StandardUILibrary;

namespace VARLab.PublicHealth
{
    /// <summary>
    /// This class is used to manage the tooltips for the interactable objects in the game. It will store the tooltip text and the POIs to show the tooltip from.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class ObjectTooltips : MonoBehaviour
    {
        /// <summary>
        ///     The message to display in the tooltip
        /// </summary>
        public string TooltipText;

        /// <summary>
        ///     The list of POIs to show the tooltip from
        /// </summary>
        public List<POI> ShowFrom;

        /// <summary>
        ///     The tooltip object
        /// </summary>
        private TooltipSimple tooltip;

        /// <summary>
        ///     The duration the mouse has been hovering over the object
        /// </summary>
        private float hoverDuration = 0;

        /// <summary>
        ///     Constant for the delay before showing the tooltip
        /// </summary>
        private const float TooltipHoverDelay = 0.5f;

        /// <summary>
        ///    Enum of where the tooltip should float
        /// </summary>
        private const ITooltip.TooltipFloat TooltipFloat = ITooltip.TooltipFloat.Top;

        /// <summary>
        ///     Flag to determine if the mouse is over the object
        /// </summary>
        private bool isMouseOver = false;

        /// <summary>
        /// This is to avoid serializing the "Tooltip" gameObject under "Interfaces" for every single interactable
        /// object that requires a tooltip. The "Tooltip" gameObject holds the tooltip UIDocument reference
        /// </summary>
        private void Awake()
        {
            tooltip = GameObject.Find("/UI/ToolTipDoors").GetComponent<TooltipSimple>();

            if (tooltip == null)
            {
                Debug.LogError("Error: \"Tooltip\" gameObject not found! \"Tooltip\" GameObject requires <UIDocument> and <TooltipSimple> components");
            }
        }

        /// <summary>
        ///     On mouse enter, start the MouseOverLoop coroutine
        /// </summary>
        public void MouseEnter()
        {
            hoverDuration = 0;
            isMouseOver = true;
            StartCoroutine(MouseOverLoop());
        }


        /// <summary>
        ///     On mouse exit stop the MouseOverLoop coroutine and hide the tooltip
        /// </summary>
        public void MouseExit()
        {
            isMouseOver = false;
            tooltip.Hide();
        }

        /// <summary>
        /// While the mouse is still hovered over the same object, hoverDuration continues to increment. When greater 
        /// than TooltipHoverDelay, show the tooltip for the current object
        /// </summary>
        private IEnumerator MouseOverLoop()
        {
            while (isMouseOver)
            {
                hoverDuration += Time.smoothDeltaTime;

                if (hoverDuration > TooltipHoverDelay)
                {
                    tooltip.SetLabel(TooltipText);
                    tooltip.Move(new Rect(Input.mousePosition.x, Input.mousePosition.y, 0.0f, 0.0f), TooltipFloat);
                    tooltip.Show();
                }

                yield return null;
            }
        }
    }
}
