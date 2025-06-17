using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace VARLab.PublicHealth
{
    /// <summary>
    /// This class is to act as a wrapper/collection for the listeners to buttons to allow an easy call to clear all listeners on a button.
    /// </summary>
    public class ActionWrapper
    {
        private List<Action> _actions = new List<Action>();
        private Button _btn;

        public ActionWrapper(Button btn) { _btn = btn; }

        /// <summary>
        /// Add a single action to a button
        /// </summary>
        /// <param name="a"> The action to add as a listener </param>
        public void AddAction(Action a)
        {
            _actions.Add(a);
            _btn.clicked += a;
        }

        /// <summary>
        /// Remove a single action from the button
        /// </summary>
        /// <param name="a"></param>
        public void RemoveAction(Action a)
        {
            _actions.Remove(a);
            _btn.clicked -= a;
        }

        /// <summary>
        /// Add multiple actions in a single call
        /// </summary>
        /// <param name="actions"></param>
        public void AddMultiple(Action[] actions)
        {
            foreach (Action a in actions) { AddAction(a); }
        }

        /// <summary>
        /// This method is used to clear all listeners from a button
        /// </summary>
        public void ClearBtn()
        {
            foreach (Action a in _actions)
            {
                _btn.clicked -= a;
            }

            _actions.Clear();
        }

        /// <summary>
        /// This is made to add all actions to a button
        /// </summary>
        public void AddAllToBtn()
        {
            foreach (Action a in _actions)
            {
                _btn.clicked += a;
            }
        }
    }
}
