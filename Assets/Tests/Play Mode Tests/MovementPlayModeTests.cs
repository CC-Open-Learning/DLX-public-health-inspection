using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using VARLab.PublicHealth;

public class MovementPlayModeTest
{
    private GameObject _prefabInstance;
    private NavMeshAgent _playerNavMesh;

    [SetUp]
    [Category("BuildServer")]
    public void SetUp()
    {
        _prefabInstance = Resources.Load("MovementTest") as GameObject;
    }

    /// <summary>
    /// This is a quick test to make sure sure the child object is being instantiated 
    /// </summary>
    [Test]
    [Category("BuildServer")]
    public void CheckIfPlayerCameraExist()
    {
        // Getting the camera component from player prefab
        Camera camera = _prefabInstance.GetComponentInChildren<Camera>();

        //Assert
        Assert.IsNotNull(camera);
    }

    /// <summary>
    /// This test is used to see if the Nav Mesh Agent is attached to the player prefab
    /// </summary>
    [Test]
    [Category("BuildServer")]
    public void CheckNavMeshAgentIsAttachedToPlayer()
    {
        // Arrange
        _playerNavMesh = _prefabInstance.GetComponentInChildren<PlayerController>().Player;

        // Assert
        Assert.IsNotNull(_playerNavMesh);
    }

    /// <summary>
    /// This test is used to check if the clickable tiles are instantiated 
    /// </summary>
    [Test]
    [Category("BuildServer")]
    public void ClickableIfTilesExist()
    {
        bool _hasTag = false;
        foreach (Transform child in _prefabInstance.transform)
        {
            if (child.tag == "MovePoint")
            {
                _hasTag = true;
                return;
            }
        }
        // Assert
        Assert.IsTrue(_hasTag);
    }

    ///// <summary>
    ///// This test is to check if the player object moves to a specified location
    ///// </summary>
    ///// <returns></returns>
    //[UnityTest]
    //public IEnumerator CheckPlayersMovement()
    //{
    //    // Arrange
    //    var hitPoint = new Vector3(0, 200, 0);
    //    _playerNavMesh = _prefabInstance.GetComponentInChildren<PlayerController>().Player;
    //    _playerNavMesh.gameObject.transform.position = new Vector3(0, 0, 0);

    //    // Act
    //    _playerNavMesh.SetDestination(hitPoint);

    //    yield return null;

    //    // Assert
    //    Assert.AreEqual(hitPoint, _playerNavMesh.transform.position);
    //}
}
