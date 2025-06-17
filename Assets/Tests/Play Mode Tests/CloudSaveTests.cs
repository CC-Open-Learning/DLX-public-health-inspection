using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using VARLab.CloudSave;
using VARLab.PublicHealth;

public class CloudSaveTests
{
    // Stroage for game objects & components
    GameObject _cloudSaveContainer;
    CloudSaving _cloudComponent;
    GuidManager _guidManager;
    AzureSaveSystem _azureSaveSystem;

    // Mock Object 
    Mock<ICloudSaveSystem> _mockCloudSaveSystem;

    // Storage for serialized string for load
    string _serializedStorage;

    [SetUp]
    public void SetUp()
    {
        // Setting up game objects
        _cloudSaveContainer = new GameObject();
        _cloudComponent = _cloudSaveContainer.AddComponent<CloudSaving>();
        _cloudComponent.Initialize();
        _guidManager = _cloudSaveContainer.AddComponent<GuidManager>();


        // setting up Mocks
        _mockCloudSaveSystem = new Mock<ICloudSaveSystem>();
        _mockCloudSaveSystem.Setup(x => x.Save(It.IsAny<string>(), It.IsAny<string>())).Returns(MockSave);
        _mockCloudSaveSystem.Setup(x => x.Load(It.IsAny<string>())).Returns(MockLoad);
        _mockCloudSaveSystem.Setup(x => x.Delete(It.IsAny<string>()));
        _cloudComponent._saveSystem = _mockCloudSaveSystem.Object;


        // copied from VARLab.Cloudsave.Tests  sets up GUID manager for serialzier
        /// <summary>Number of entries in <see cref="MockSaveSystem.GetMockData"/>.</summary>
        const int MOCK_DATA_SIZE = 6;
        _guidManager.PrepareToUpdateEntries();
        var components = new MockSerializedObject[MOCK_DATA_SIZE];
        var initialRotations = new Quaternion[components.Length];
        var initialScales = new Vector3[components.Length];

        for (int i = 0; i < components.Length; i++)
        {
            components[i] = new GameObject($"Mock Object {i}").AddComponent<MockSerializedObject>();
            _guidManager.TrackObject(components[i], i.ToString());

            components[i].transform.localScale = Vector3.one * (i + 0.25f);
            initialScales[i] = components[i].transform.localScale;

            var x = 45 * (i + 1);

            var eulerAngles = new Vector3(x, 0f, 0f);
            var rotation = Quaternion.Euler(eulerAngles);

            components[i].transform.rotation = rotation;
            initialRotations[i] = components[i].transform.rotation;
        }


    }

    /// <summary>
    /// clean up test objects
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        foreach (GameObject gameObject in GameObject.FindObjectsOfType<GameObject>())
        {
            if (gameObject != null)
            {
                GameObject.DestroyImmediate(gameObject);
            }
        }
    }

    [UnityTest]
    [Category("BuildServer")]
    public IEnumerator AssigningAzureSystem()
    {
        // Arrange

        // Setting up game objects
        GameObject.Destroy(_cloudSaveContainer);
        _cloudSaveContainer = new GameObject();
        _cloudComponent = _cloudSaveContainer.AddComponent<CloudSaving>();
        _azureSaveSystem = _cloudSaveContainer.AddComponent<AzureSaveSystem>();
        SerializedObject serializedObject = new SerializedObject(_cloudComponent);
        SerializedProperty azurePropetry = serializedObject.FindProperty("_azureSaveSystem");
        azurePropetry.objectReferenceValue = _azureSaveSystem;
        serializedObject.ApplyModifiedProperties();
        _cloudComponent.Initialize();

        // Act
        yield return null;

        // Assert
        Assert.IsTrue(_cloudComponent._saveSystem is AzureSaveSystem);

    }

    /// <summary>
    /// this tests the system calling the save functionality
    /// </summary>
    /// <returns></returns>
    [UnityTest]
    [Category("BuildServer")]
    public IEnumerator SingleSave()
    {
        // Arrange

        // Act
        _cloudComponent.Save();
        yield return null;
        // Assert
        _mockCloudSaveSystem.Verify(x => x.Save(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    /// <summary>
    /// This tests the ability to queue up mupltiple save requests
    /// </summary>
    /// <returns></returns>
    [UnityTest]
    [Category("BuildServer")]
    public IEnumerator DoubleSaveQueued()
    {
        // Arrange

        // Act
        _cloudComponent.Save();
        _cloudComponent.Save();
        yield return null;
        yield return null;
        yield return null;
        yield return null;
        // Assert
        _mockCloudSaveSystem.Verify(x => x.Save(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
    }

    /// <summary>
    /// this tests single delete requests
    /// </summary>
    /// <returns></returns>
    [UnityTest]
    [Category("BuildServer")]
    public IEnumerator SingleDelete()
    {
        // Arrange

        // Act
        _cloudComponent.Delete();
        yield return null;
        // Assert
        _mockCloudSaveSystem.Verify(x => x.Delete(It.IsAny<string>()), Times.Once);
    }

    /// <summary>
    /// this tests single load call
    /// </summary>
    /// <returns></returns>
    [UnityTest]
    [Category("BuildServer")]
    public IEnumerator SingleLoad()
    {
        // Arrange
        JsonCloudSerializer jserializer = new JsonCloudSerializer();
        _serializedStorage = jserializer.Serialize();

        // Act
        _cloudComponent.Load();
        yield return null;
        // Assert
        _mockCloudSaveSystem.Verify(x => x.Load(It.IsAny<string>()), Times.Once);
    }


    /// <summary>
    /// this tests single load call that failed and has null data
    /// </summary>
    /// <returns></returns>
    [UnityTest]
    [Category("BuildServer")]
    public IEnumerator SingleLoadFail()
    {
        // Arrange
        _serializedStorage = null;

        // Act
        _cloudComponent.Load();
        yield return null;
        // Assert
        Assert.IsFalse(_cloudComponent.LoadSuccess);
        _mockCloudSaveSystem.Verify(x => x.Load(It.IsAny<string>()), Times.Once);
    }

    /// <summary>
    /// Mock functionality for saving
    /// </summary>
    /// <returns></returns>
    public Coroutine MockSave()
    {
        _cloudComponent.RequestCompletedEventHandler(_mockCloudSaveSystem, null);
        return null;
    }

    /// <summary>
    /// Mock functionality for loading
    /// </summary>
    /// <returns></returns>
    public CoroutineWithData MockLoad()
    {
        CoroutineWithData toReturn = new CoroutineWithData(_cloudComponent, FakeLoadRequest());
        return toReturn;
    }

    /// <summary>
    /// this is for Mock loading to set up a coroutine with new data
    /// </summary>
    /// <returns></returns>
    public IEnumerator FakeLoadRequest()
    {
        yield return _serializedStorage;
    }
}

// copied from VARLab.Cloudsave.Tests because linking through assembly wouldn't work for some reason. 
[CloudSaved]
[JsonObject(MemberSerialization.OptIn)]
public class MockSerializedObject : MonoBehaviour, ICloudSerialized, ICloudDeserialized
{
    [JsonProperty]
    private Vector3 scale;

    [JsonProperty]
    public Vector3 rotation;

    public void OnDeserialize()
    {
        SetScale();
        SetRotation();
    }

    public void OnSerialize()
    {
        rotation = transform.rotation.eulerAngles;
        scale = transform.localScale;
    }

    private void SetScale()
    {
        transform.localScale = scale;
    }

    private void SetRotation()
    {
        transform.rotation = Quaternion.Euler(rotation);
    }
}
