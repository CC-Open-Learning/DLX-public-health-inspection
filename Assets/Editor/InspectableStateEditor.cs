using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VARLab.PublicHealth;
using Button = UnityEngine.UIElements.Button;

public class InspectableStateEditor : EditorWindow
{
    [SerializeField] private VisualTreeAsset _rootInspectableStateEditor = default;

    [SerializeField] private VisualTreeAsset _inspectableFoldout = default;

    [SerializeField] private VisualTreeAsset _inspectableDropdown = default;

    private InspectableManager _inspectableManager;

    private Dictionary<string, List<InspectableObject>> _poiAndInspectables;

    private ScrollView _scrollView;

    private TextField _fileName;


    /// <summary>
    ///     Constant string for the directory that the scenario's will be saved to
    /// </summary>
    private const string ScenariosDirectoryPath = "Assets/Resources/Scenarios/";
    private const string MenuPath = "PHI/Inspectable State Editor";
    private const string WindowTitle = "Inspectable State Editor";
    private const string LoadFileDropdownField = "ddf_loadFile";
    private const string StatesScrollView = "sv_states";
    private const string FileNameTextField = "tf_fileName";
    private const string SaveScenarioBtn = "btn_saveScenario";
    private const string LoadScenarioBtn = "btn_ApplyScenario";
    private const string SaveTextBtn = "Save New Scenario";
    private const string SaveUpdateTextBtn = "Update Scenario File";
    private const string ApplyTextBtn = "Apply To Scene";

    /// <summary>
    ///     This builds the window and sets the min size that the window can be. 
    /// </summary>
    [MenuItem(MenuPath)]
    public static void ShowExample()
    {
        InspectableStateEditor wnd = GetWindow<InspectableStateEditor>();
        wnd.titleContent = new GUIContent(WindowTitle);
        //min size
        wnd.minSize = new Vector2(500, 800);
    }

    /// <summary>
    ///     This method setups up the UI elements and sets up the data that needs to be tracked from the user. 
    /// </summary>
    public void CreateGUI()
    {

        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // Instantiate UXML
        VisualElement labelFromUXML = _rootInspectableStateEditor.Instantiate();
        root.Add(labelFromUXML);
        root.RemoveFromClassList("StateViewItems");

        GetPOIsAndInspectableObjects();

        SetupData(root);
    }

    /// <summary>
    /// This function will generate and sort the dictionary _poiAndInspectables.
    /// </summary>
    public void GetPOIsAndInspectableObjects()
    {
        //Get the list of all the inspectable objects in the scene 
        _inspectableManager = FindAnyObjectByType<InspectableManager>();
        _inspectableManager.UpdateInspectableObjectsList();

        //Create a new dictionary
        _poiAndInspectables = new();

        List<string> poiNames = new();

        // Create a list of unique POIs
        foreach (InspectableObject inspectableObject in _inspectableManager.InspectableObjects)
        {
            inspectableObject.SetLocation();

            if (!poiNames.Contains(inspectableObject.Location))
            {
                poiNames.Add(inspectableObject.Location);
            }
        }

        // Alphabetize the POI names and add to dictionary
        poiNames.Sort();

        foreach (string poi in poiNames)
        {
            _poiAndInspectables.TryAdd(poi, new List<InspectableObject>());
        }

        // Get the inspectables in each POI
        foreach (InspectableObject inspectableObject in _inspectableManager.InspectableObjects)
        {
            _poiAndInspectables[inspectableObject.Location].Add(inspectableObject);
        }

        // Alphabetize the lists of inspectables
        foreach (string poi in _poiAndInspectables.Keys)
        {
            _poiAndInspectables[poi].Sort((p, q) => p.InspectableObjectName.CompareTo(q.InspectableObjectName));
        }
    }



    /// <summary>
    /// This method sets up the functionality within the UI and populates the data. 
    /// </summary>
    /// <param name="root">rootVisualElement</param>
    private void SetupData(VisualElement root)
    {

        //Get all the scenario files in the directory add them to the dropdown
        var scenarioFiles = Directory.GetFiles(ScenariosDirectoryPath, "*.asset").Select(Path.GetFileName).ToArray();

        //find the load file dropdown 
        var loadFileDropdown = root.Q<DropdownField>(LoadFileDropdownField);

        //drop the file extensions
        loadFileDropdown.choices = scenarioFiles.Select(file => Path.GetFileNameWithoutExtension(file)).ToList();
        loadFileDropdown.RegisterValueChangedCallback<string>(evt =>
        {
            //protect error against resetting the index of the dropdown
            if (loadFileDropdown.index < 0)
                return;

            UpdateScrollView(evt.newValue);
            _fileName.value = evt.newValue;
        });

        //Body Section
        //find the scrollview 
        _scrollView = root.Q<ScrollView>(StatesScrollView);
        PopulateScrollView(_scrollView);

        //Footer Section
        //find the file name textfield
        _fileName = root.Q<TextField>(FileNameTextField);
        _fileName.RegisterValueChangedCallback(evt =>
        {
            //Change the text of the save button if the file name already exists
            if (loadFileDropdown.choices.Contains(evt.newValue))
            {
                root.Q<Button>(SaveScenarioBtn).text = SaveUpdateTextBtn;
            }
            else
            {
                root.Q<Button>(SaveScenarioBtn).text = SaveTextBtn;
            }

            if (AcceptableFileName(_fileName.text))
            {
                //Set the button to be interactable
                root.Q<Button>(SaveScenarioBtn).SetEnabled(true);
            }
            else
            {
                //Set the button to be not interactable
                root.Q<Button>(SaveScenarioBtn).SetEnabled(false);
            }
        });

        //find the save button
        var saveButton = root.Q<Button>(SaveScenarioBtn);
        saveButton.text = SaveTextBtn;
        saveButton.SetEnabled(false);
        saveButton.clickable.clicked += () =>
        {
            CreateCurrentStateFile(_scrollView, _fileName.value);
            //Update the dropdown with the new file
            loadFileDropdown.choices = Directory.GetFiles(ScenariosDirectoryPath, "*.asset").Select(file => Path.GetFileNameWithoutExtension(file)).ToList();
            //Set the dropdown to the new file
            loadFileDropdown.value = _fileName.value;
            //Change the text of the save button back to the original text
            saveButton.text = SaveUpdateTextBtn;
        };

        //find the load button
        var applyChangesButton = root.Q<Button>(LoadScenarioBtn);
        applyChangesButton.text = ApplyTextBtn;
        applyChangesButton.clickable.clicked += () =>
        {
            //Apply the changes made in the dropdowns to the inspectable objects in the scene
            ApplyChanges();
            //Apply the changes to the internal dictionary.
            GetPOIsAndInspectableObjects();

            loadFileDropdown.index = -1;
        };
    }

    /// <summary>
    /// Using the _poiAndInspectables dictionary made when the window was opened, 
    /// populate the foldouts and dropdowns with the poi's and the inspectable objects with states. 
    /// Only add to the state selector inspectables that have more then 1 state to select from.
    /// </summary>
    /// <param name="scrollView"></param>
    private void PopulateScrollView(ScrollView scrollView)
    {
        //Iterate through the dictionary and create the foldouts and dropdowns
        foreach (KeyValuePair<string, List<InspectableObject>> entry in _poiAndInspectables)
        {
            //Add the foldout to the scrollview
            //Create a new foldout using the foldout visual tree asset
            Foldout foldout = _inspectableFoldout.Instantiate().Q<Foldout>();
            foldout.text = entry.Key.ToString();

            //Add the dropdowns to the foldout
            for (int i = 0; i < entry.Value.Count; i++)
            {
                // Only add to the dropdown list inspectables that have more then 1 state to select from
                if (entry.Value[i].GetAllScenarioNames().Count() > 1)
                {
                    //Create the dropdown and add it to the foldout
                    //entry contains the key which is the POI name and the value which is a list of the inspectable objects
                    //Create a new dropdown using the dropdown visual tree asset
                    DropdownField dropdownInspectables = _inspectableDropdown.Instantiate().Q<DropdownField>();

                    dropdownInspectables.label = entry.Value[i].InspectableObjectName;
                    dropdownInspectables.choices = entry.Value[i].GetAllScenarioNames();
                    dropdownInspectables.index = 0;
                    dropdownInspectables.AddToClassList("StateViewItems");


                    foldout.Add(dropdownInspectables);
                }
            }

            // Add the foldout to the scrollview if there is at least 1 inspectable to select a state.
            if (foldout.childCount > 0)
            {
                scrollView.Add(foldout);
            }
        }
    }

    /// <summary>
    ///     This method will take the currently selected elements within the scroll view and populate a scriptable object dictionary. 
    /// </summary>
    /// <param name="sv">The scrollview that contains all the data from built from the <see cref="GetPOIsAndInspectableObjects"/> and whatever changes the user made to it</param>
    /// <param name="fileName">The file name entered in the file name text field</param>
    public void CreateCurrentStateFile(ScrollView sv, string fileName)
    {
        //Create a new scenario data object and name the object the value of fileName
        ScenarioData scenarioData = CreateInstance<ScenarioData>();

        //Set the scenario name
        scenarioData.ScenarioName = fileName;

        //Create a new list of inspectable objects
        List<ScenarioData.Inspectable> inspectableList = new();

        if (_poiAndInspectables != null)
        {
            foreach (KeyValuePair<string, List<InspectableObject>> entry in _poiAndInspectables)
            {
                for (int i = 0; i < entry.Value.Count; i++)
                {
                    if (entry.Value[i].GetAllScenarioNames().Count == 1)
                    {
                        var choice = entry.Value[i].GetAllScenarioNames();

                        ScenarioData.Inspectable inspectable = new()
                        {
                            //Create the ID for the inspectable object 
                            InspectableID = entry.Value[i].InspectableObjectID,
                            InspectableState = choice[0].ToString(),
                        };

                        // Add the inspectable to the scenario file
                        inspectableList.Add(inspectable);
                    }
                }
            }
        }

        //Iterate through the scrollview and get the values of the dropdowns
        foreach (Foldout foldout in sv.Children().Cast<Foldout>())
        {
            foreach (DropdownField dropdown in foldout.Children().Cast<DropdownField>())
            {
                ScenarioData.Inspectable inspectable = new()
                {
                    //Create the ID for the inspectable object 
                    InspectableID = foldout.text + "_" + dropdown.label,
                    InspectableState = dropdown.value
                };

                inspectableList.Add(inspectable);
            }
        }

        //if the file exists, update the file otherwise create a new file
        if (File.Exists(ScenariosDirectoryPath + fileName + ".asset"))
        {
            //Load the file
            ScenarioData existingScenarioData = AssetDatabase.LoadAssetAtPath<ScenarioData>(ScenariosDirectoryPath + fileName + ".asset");

            //Update the list
            existingScenarioData.InspectablesAndStates = inspectableList;
#if UNITY_EDITOR
            EditorUtility.SetDirty(existingScenarioData);
#endif
        }
        else
        {
            //Add the list to the scenario data object
            scenarioData.InspectablesAndStates = inspectableList;

            //Create the file path
            string filePath = ScenariosDirectoryPath + fileName + ".asset";

            //Create the file
            AssetDatabase.CreateAsset(scenarioData, filePath);

            //Add to scenario manager list
            AssetDatabase.SaveAssets();

            //Get the scenario manager
            ScenarioManager scenarioManager = FindAnyObjectByType<ScenarioManager>();

            //Add the scenario to the list
            scenarioManager.AddScenario(scenarioData);
        }

        //Save the changes
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }


    /// <summary>
    ///     This method will update the inspectable manager with the states provided within the scrollview
    /// </summary>
    private void ApplyChanges()
    {
        //Iterate through the dropdowns and apply the changes to the inspectable objects in the scene
        foreach (Foldout foldout in _scrollView.Children().Cast<Foldout>())
        {
            foreach (DropdownField dropdown in foldout.Children().Cast<DropdownField>())
            {
                //Get the inspectable object ID from the dropdown label and foldout text
                string inspectableID = foldout.text + "_" + dropdown.label;
                _inspectableManager.UpdateInspectableObjectState(inspectableID, dropdown.value);
            }
        }
    }

    /// <summary>
    ///     This method will update the scrollview with the items from the file loaded from the load file dropdown.
    /// </summary>
    /// <param name="fileName">the name of the file to be loaded</param>
    private void UpdateScrollView(string fileName)
    {
        //Load the file
        ScenarioData scenarioData = AssetDatabase.LoadAssetAtPath<ScenarioData>(ScenariosDirectoryPath + fileName + ".asset");

        //Get the list of inspectable objects
        List<ScenarioData.Inspectable> inspectableList = scenarioData.InspectablesAndStates;

        //Iterate through the list and update the dropdowns
        foreach (Foldout foldout in _scrollView.Children().Cast<Foldout>())
        {
            foreach (DropdownField dropdown in foldout.Children().Cast<DropdownField>())
            {
                //Create the inspectable ID
                string inspectableID = foldout.text + "_" + dropdown.label;
                string inspectableData = inspectableList.Find(inspectable => inspectable.InspectableID == inspectableID).InspectableState;

                dropdown.value = inspectableData;
            }
        }
    }

    /// <summary>
    ///     This method will check to ensure that the file name is acceptable for saving.
    /// </summary>
    /// <param name="filename"></param>
    /// <returns></returns>
    private bool AcceptableFileName(string filename)
    {
        //Check to see if the file name is empty
        if (string.IsNullOrEmpty(filename))
        {
            return false;
        }

        //Check to see if the file name contains any special characters
        if (Regex.IsMatch(filename, @"[!@#$%^&*()_+=\[{\]};:<>|./?,-]"))
        {
            return false;
        }

        return true;
    }
}
