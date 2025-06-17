using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using VARLab.PublicHealth;


/// <summary>
///     Custom property drawer for the AnimBehaviour class, this will allow us to select the animator and the trigger parameter
/// </summary>
[CustomPropertyDrawer(typeof(AnimBehaviour))]
public class AnimBehaviourDrawer : PropertyDrawer
{
    private DropdownField _actionDropDown;
    private DropdownField _stateDropDown;
    private VisualElement _rootContainer;
    private SerializedProperty _triggerParamString;
    private SerializedProperty _boolParamString;
    private PropertyField _animatorField;
    private PropertyField _triggerBoolMapField;


    /// <summary>
    /// Create the custom property GUI for the AnimBehaviour class 
    /// </summary>
    /// <param name="animBehaviour">The SerializedProperty to make the custom GUI for</param>
    /// <returns>Visual Element the root container.</returns>
    public override VisualElement CreatePropertyGUI(SerializedProperty animBehaviour)
    {
        animBehaviour.serializedObject.Update();
        // Create property container element.
        _rootContainer = new VisualElement();

        // Create property fields.
        _animatorField = new PropertyField(animBehaviour.FindPropertyRelative("Animator"));

        // Get properties from the AnimBehaviour class
        _triggerParamString = animBehaviour.FindPropertyRelative("_triggerParamString");
        _boolParamString = animBehaviour.FindPropertyRelative("_boolParamString");

        _animatorField.RegisterValueChangeCallback(evt => AnimatorFieldChanged(evt, animBehaviour));

        // Add fields to the container.
        _rootContainer.Add(_animatorField);
        _rootContainer.Add(_triggerBoolMapField);

        return _rootContainer;
    }


    /// <summary>
    /// Callback for when the animator field is changed. 
    /// </summary>
    /// <param name="change">the change holding the changed property</param>
    /// <param name="animBehaviour"></param>
    private void AnimatorFieldChanged(SerializedPropertyChangeEvent change, SerializedProperty animBehaviour)
    {
        SetUpDropDowns(change.changedProperty, animBehaviour);
    }

    /// <summary>
    /// Update the dropdowns with the parameters from the animator controller
    /// </summary>
    /// <param name="animBehaviour">Serialized AnimBehaviour class</param>
    /// <param name="animator">Animator Controller component</param>
    private void SetUpDropDowns(SerializedProperty animator, SerializedProperty animBehaviour)
    {
        //Check if the animator has been set
        if (animator.objectReferenceValue == null)
        {
            return;
        }


        Animator animatorComponent = (Animator)animator.objectReferenceValue;

        List<string> animatorTriggerParameters = new List<string>();
        List<string> animatorBoolParameters = new List<string>();

        for (int i = 0; i < animatorComponent.parameterCount; i++)
        {
            if (animatorComponent.parameters[i].type == AnimatorControllerParameterType.Trigger)
                animatorTriggerParameters.Add(animatorComponent.parameters[i].name);

            if (animatorComponent.parameters[i].type == AnimatorControllerParameterType.Bool)
                animatorBoolParameters.Add(animatorComponent.parameters[i].name);
        }

        // Create a dropdown field for the trigger parameter
        _actionDropDown = new DropdownField("Action", animatorTriggerParameters, 0);
        _actionDropDown.BindProperty(_triggerParamString);

        // Create a dropdown field for the bool parameter
        _stateDropDown = new DropdownField("State Parameter", animatorBoolParameters, 0);
        _stateDropDown.BindProperty(_boolParamString);

        ResetRootContainer();
        animBehaviour.serializedObject.ApplyModifiedProperties();

    }


    /// <summary>
    /// Reset all the fields in the root container
    /// </summary>
    private void ResetRootContainer()
    {
        _rootContainer.Clear();
        _rootContainer.Add(_animatorField);
        _rootContainer.Add(_actionDropDown);
        _rootContainer.Add(_stateDropDown);

    }

}
