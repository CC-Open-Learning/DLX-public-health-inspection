using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VARLab.PublicHealth;

public class InspectableStateEditorTest
{
    //inspectable state viewer
    InspectableStateEditor _view;

    //UIElements
    ScrollView _scrollView;

    //FileDropdownField
    DropdownField _fileDropdownField;

    //constants for testing 
    const string StateDirectoryPath = "Assets/Resources/Scenarios/";
    const string FileName = "testing/EditModeTest";
    const string SecondFileName = "testing/EditModeTest2";
    const string POIName = "TestPOI";
    const string InspectableObjectName = "TestObject";
    const string InspectableObjectID = "TestPOI_TestObject";
    const string InspectableObjectState = "Test1";

    /// <summary>
    ///     This is the setup function that runs before every test
    /// </summary>
    [SetUp]
    public void RunBeforeEveryTest()
    {
        //Setup UI Class
        _view = new InspectableStateEditor();

        //Create the scrollview
        _scrollView = CreateScrollView();

        //Setup FileDropdownField
        _fileDropdownField = new DropdownField();
        _fileDropdownField.label = "TestDropdown";
        _fileDropdownField.index = 0;
        _fileDropdownField.choices = new List<string>() { FileName, SecondFileName };

        //Use the create state file function to create a file
        _view.CreateCurrentStateFile(_scrollView, FileName);
    }

    /// <summary>
    ///     This test checks that file name from setup was created and referenceable.
    /// </summary>
    [Test]
    public void StateFileExistWithTheGivenName()
    {
        Assert.True(FileWithGivenNameExists());
    }


    /// <summary>
    ///     This test checks if the file dropdown field has the correct value after the callback is made. 
    /// </summary>
    [Test]
    public void FileDropdownFieldUpdatesElementsCorrectly()
    {
        //change the value of the dropdown to use callback
        _fileDropdownField.index = 1;

        //save it to a file
        _view.CreateCurrentStateFile(_scrollView, SecondFileName);

        //check if the dropdown has the correct value
        Assert.AreEqual(_fileDropdownField.value, SecondFileName);
    }

    /// <summary>
    ///     This test checks if the file was create with the correct information we would expect from the setup
    /// </summary>
    [Test]
    public void FileContainsCorrectContent()
    {
        // Get the scriptable object from the file path 
        ScenarioData fileState = (ScenarioData)AssetDatabase.LoadAssetAtPath(StateDirectoryPath + FileName + ".asset", typeof(ScenarioData));
        // Create a second scriptable object with the data we expect to be in the file
        ScenarioData expectedState = ScriptableObject.CreateInstance<ScenarioData>();
        ScenarioData.Inspectable inspectable = new()
        {
            InspectableID = InspectableObjectID,
            InspectableState = InspectableObjectState
        };

        expectedState.InspectablesAndStates = new()
        {
            inspectable
        };

        // Check if the two scriptable objects are equal
        Assert.AreEqual(fileState.InspectablesAndStates, expectedState.InspectablesAndStates);
    }

    /// <summary>
    ///     This test checks if the dropdown in the scrollview has the correct value after the callback is made
    /// </summary>
    [Test]
    public void FileLoadUpdatesElementsCorrectly()
    {
        //get the dropdown in the scrollview 
        //ScrollView -> Foldout -> Dropdown
        DropdownField dropdown = (DropdownField)_scrollView.ElementAt(0).ElementAt(0);
        Debug.Log(dropdown.value); //"Test1" 

        //change the value of the dropdown to use callback
        dropdown.index = 1;

        //save it to a file
        _view.CreateCurrentStateFile(_scrollView, SecondFileName);

        //check if the dropdown has the correct value
        Assert.AreEqual(dropdown.value, "Test2");
    }


    /// <summary>
    /// Creates a scrollview with a foldout and a dropdown
    /// </summary>
    /// <returns cref=ScrollView</returns>
    private ScrollView CreateScrollView()
    {
        //create a scrollview
        ScrollView scrollView = new ScrollView();


        //Add the foldout to the scrollview
        Foldout foldout = new() { text = POIName };
        scrollView.Add(foldout);

        //Create the dropdown and add it to the foldout
        //entry contains the key which is the POI name and the value which is a list of the inspectable objects
        DropdownField dropdownInspectables = new()
        {
            label = InspectableObjectName,
            choices = new List<string>() { InspectableObjectState, "Test2" },
            index = 0,
        };

        foldout.Add(dropdownInspectables);

        return scrollView;
    }


    /// <summary>
    ///     Checks if the file with the given name exists
    /// </summary>
    /// <returns>True if the file is found</returns>
    private bool FileWithGivenNameExists()
    {
        string file = StateDirectoryPath + FileName + ".asset";
        return File.Exists(file);
    }
}
