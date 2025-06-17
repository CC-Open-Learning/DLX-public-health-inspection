using System;
using UnityEngine.UIElements;

namespace VARLab.PublicHealth
{
    /// <summary>
    /// This is a normal C# class that is created and used as a singleton that acts as a "service" to register callbacks for UI elements to disable interactions with inspectables based on mouse hovering ui
    /// </summary>
    public class UIBlocker
    {
        public static UIBlocker Instance => _instance.Value;

        private static readonly Lazy<UIBlocker> _instance = new Lazy<UIBlocker>(() => new UIBlocker());

        /// <summary>
        /// Callback to be called when the Mouse enters the panel.
        /// </summary>
        /// <param name="evt"></param>
        private void MouseEnterCallback(MouseEnterEvent evt, VisualElement elem)
        {
            if (PauseBehaviour.IsPaused) return;
            // check if the target of the event is the background element (because UI events "bubble" through the UI tree, so it could be an event for a different element).
            if (evt.target == elem)
            {
                Interactions.Instance.SetInteract(false);
            }
        }

        /// <summary>
        /// Callback to be called when the Mouse leaves the panel.
        /// </summary>
        /// <param name="evt"></param>
        private void MouseLeaveCallback(MouseLeaveEvent evt, VisualElement elem)
        {
            if (PauseBehaviour.IsPaused) return;
            // check if the target of the event is the background element (because UI events "bubble" through the UI tree, so it could be an event for a different element).
            if (evt.target == elem)
            {
                Interactions.Instance.ReEnableActions();
            }
        }

        /// <summary>
        /// Method that registers the MouseEnterCallback on the MouseEnter UI event.
        /// </summary>
        public void RegisterMouseEnterCallback(VisualElement elem)
        {
            // Registers a callback on the MouseEnterEvent which will call MouseEnterCallback
            elem.RegisterCallback<MouseEnterEvent, VisualElement>(MouseEnterCallback, elem);
        }

        /// <summary>
        /// Method that registers the MouseLeaveCallback on the MouseLeave UI event.
        /// </summary>
        public void RegisterMouseLeaveCallback(VisualElement elem)
        {
            // Registers a callback on the MouseLeaveEvent which will call MouseLeaveCallback
            elem.RegisterCallback<MouseLeaveEvent, VisualElement>(MouseLeaveCallback, elem);
        }
    }
}
