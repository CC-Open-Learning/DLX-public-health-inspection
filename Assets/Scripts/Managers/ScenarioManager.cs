using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace VARLab.PublicHealth
{
    /// <summary>
    ///     This class is used to manage the scenarios in the game. It will load a scenario based on the scenario name. If there is no scenario name it will load a random scenario.
    /// </summary>
    public class ScenarioManager : MonoBehaviour
    {
        [SerializeField, Tooltip("List of available Scenarios")] private List<ScenarioData> _scenarioList;

        /// <summary> <see cref="InspectableManager.InitializeInspectableObjects(ScenarioData)"/> </summary>
        public UnityEvent<ScenarioData> OnScenarioLoad;

        /// <summary> <see cref="SaveDataSupport.SaveScenario(string)"/></summary>
        public UnityEvent<string> OnScenarioSave;

        /// <summary>Index</summary>
        private int _scenarioIndex;

        private void Start()
        {
            // Start the Analytics Timer
            AnalyticsHelper.StartTimer();
        }

        // For testing purposes we can swap between scenarios using the keyboard. 
        private void Update()
        {
            // triggers update elapsed time if no inspections have been made.
            AnalyticsHelper.UpdateTimer();
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.A))
            {
                OnScenarioLoad.Invoke(_scenarioList[0]);
                Debug.Log("Scenario 1 Loaded");
            }
            if (Input.GetKeyDown(KeyCode.B))
            {
                OnScenarioLoad.Invoke(_scenarioList[1]);
                Debug.Log("Scenario 2 Loaded");
            }
            if (Input.GetKeyDown(KeyCode.C))
            {
                OnScenarioLoad.Invoke(_scenarioList[2]);
                Debug.Log("Scenario 3 Loaded");
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                OnScenarioLoad.Invoke(_scenarioList[3]);
                Debug.Log("Scenario 4 Loaded");
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                OnScenarioLoad.Invoke(_scenarioList[4]);
                Debug.Log("Scenario 5 Loaded");
            }
            if (Input.GetKeyDown(KeyCode.F))
            {
                OnScenarioLoad.Invoke(_scenarioList[5]);
                Debug.Log("Scenario 6 Loaded");
            }
#endif
        }

        /// <summary>
        /// This function will check if there is a save file or not and either load the saved file or 
        /// load a random scenario.
        /// </summary>
        /// <param name="scenarioName">name of the saved scenario.</param>
        public void InitializeScenario(string scenarioName = null)
        {
            // if there isn't a saved scenario a random scenario index is generated and the scenario name is saved.
            // else the scenario index is retreived using the scenario name.
            if (string.IsNullOrEmpty(scenarioName))
            {
                // Randomizes the value in the _scenarioIndex
                RandomizeScenario();

                // Invoke the event to save the scenario name   
                OnScenarioSave.Invoke(_scenarioList[_scenarioIndex].ScenarioName);
            }
            else
            {
                // Using the scenario name to get the index of the scenario
                GetScenarioIndexByName(scenarioName);
            }

            OnScenarioLoad.Invoke(_scenarioList[_scenarioIndex]);
        }

        /// <summary>
        /// Generates a random number from 0(included) to _scenarioList.Count()-1.
        /// </summary>
        /// <returns>Scenario Index</returns>
        private int RandomizeScenario()
        {
            _scenarioIndex = Random.Range(0, _scenarioList.Count - 1);
            return _scenarioIndex;
        }

        /// <summary>
        /// Find the index o the saved scenario by name.
        /// </summary>
        /// <param name="scenarioName"></param>
        private void GetScenarioIndexByName(string scenarioName)
        {
            _scenarioIndex = _scenarioList.FindIndex(x => x.ScenarioName == scenarioName);
        }

        /// <summary>
        ///     This method is used to add a scenario to the list of scenarios
        /// </summary>
        /// <param name="sd"></param>
        public void AddScenario(ScenarioData sd)
        {
            _scenarioList.Add(sd);
        }

        public string GetScenarioName()
        {
            return _scenarioList[_scenarioIndex].ScenarioName;
        }
    }
}
