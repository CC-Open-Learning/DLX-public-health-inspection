using UnityEditor;
using UnityEngine;
using VARLab.PublicHealth;

/// <summary>
///     The purpose of this class is allow the user to interact with some of the inspectable manager's properties in the inspector. 
///     The user can update the inspectable list, generate IDs for all inspectables, and remove the location prefix from the names of the inspectable objects.
///     Note: Removing the prefix from the names of the object may not be necessary after the initial ID change, but is included for potential merge conflict resolution.   
/// </summary>
[CustomEditor(typeof(InspectableManager))]
public class InspectableManagerEditor : Editor
{
    private readonly static string UpdateButtonLabel = "Update Inspectable Object List";
    private readonly static string GenerateIDsLabel = "Generate IDs for all Inspectables";

    /// <summary>
    ///     GUI update function
    /// </summary>
    public override void OnInspectorGUI()
    {
        //update the serialized object
        serializedObject.Update();

        //Check for changes
        EditorGUI.BeginChangeCheck();

        //add the button
        if (GUILayout.Button(UpdateButtonLabel)) { UpdateList(); }
        if (GUILayout.Button(GenerateIDsLabel)) { GenerateIDs(); }

        //add the serialized fields
        _ = EditorGUILayout.PropertyField(serializedObject.FindProperty("LogCompliantEvent"), true);
        _ = EditorGUILayout.PropertyField(serializedObject.FindProperty("OnInspectionChanged"), true);
        _ = EditorGUILayout.PropertyField(serializedObject.FindProperty("OnInspectionDeleted"), true);
        _ = EditorGUILayout.PropertyField(serializedObject.FindProperty("OnInspectionMade"), true);
        _ = EditorGUILayout.PropertyField(serializedObject.FindProperty("LogInspectionDeleted"), true);
        _ = EditorGUILayout.PropertyField(serializedObject.FindProperty("ObjectsToAddTag"), true);
        _ = EditorGUILayout.PropertyField(serializedObject.FindProperty("ImageManagerObj"), true);

        //Leave last so the so you don't to scroll past this list to see the other fields
        _ = EditorGUILayout.PropertyField(serializedObject.FindProperty("InspectableObjects"), true);

        //end of check, apply changes
        if (EditorGUI.EndChangeCheck()) { serializedObject.ApplyModifiedProperties(); }
    }


    /// <summary>
    ///     This function is responsible for calling the update inspectable list function when the button is pressed
    /// </summary>
    public void UpdateList()
    {
        if (serializedObject.targetObject is InspectableManager inspectableManager)
        {
            inspectableManager.UpdateInspectableObjectsList();
        }
    }


    /// <summary>
    ///    This function is responsible for calling the generate IDs function when the button is pressed
    /// </summary>
    public void GenerateIDs()
    {
        if (serializedObject.targetObject is InspectableManager inspectableManager)
        {
            inspectableManager.GenerateIDs();
            inspectableManager.UpdateInspectableObjectsList();
        }
    }
}
