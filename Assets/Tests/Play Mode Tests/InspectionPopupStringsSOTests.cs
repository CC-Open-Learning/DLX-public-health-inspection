using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using VARLab.PublicHealth;

/// <summary>
///     This file contains the testing for the inspectable manager. 
///     All the functionality of the inspectable manager is tested here.
/// </summary>
public class InspectionPopupStringsSOTest
{
    //Scriptable object
    InspectionPopupStringsSO inspectionPopupStringsSO;

    [SetUp]
    [Category("BuildServer")]
    public void RunBeforeEveryTest()
    {
        inspectionPopupStringsSO = ScriptableObject.CreateInstance<InspectionPopupStringsSO>();
    }


    /// <summary>
    /// Ensure that before the inspection is completed, the current visua inspection message is set to the correct value
    /// </summary>
    [UnityTest]
    [Category("BuildServer")]
    public IEnumerator EnsureVisualInspectionMessageIsSetCorrectly()
    {
        //Arrange 
        var toolUsed = VARLab.PublicHealth.Tools.Visual;
        inspectionPopupStringsSO.VisualInfo = "Visual inspection test.";

        //Act
        inspectionPopupStringsSO.SetCurrentInspectionMessage(toolUsed, null);

        yield return null;

        //Assert
        Assert.AreEqual(inspectionPopupStringsSO.VisualInfo, inspectionPopupStringsSO.CurrentInspectionMesssage);
    }

    /// <summary>
    /// Ensure that before the inspection is completed, the current IR inspection message is set to the correct value
    /// </summary>
    [UnityTest]
    [Category("BuildServer")]
    public IEnumerator EnsureIRInspectionMessageIsSetCorrectly()
    {
        //Arrange 
        var toolUsed = VARLab.PublicHealth.Tools.IRThermometer;
        inspectionPopupStringsSO.IRInfo = "IR Thermometer inspection test.";

        //Act
        inspectionPopupStringsSO.SetCurrentInspectionMessage(toolUsed, null);

        yield return null;

        //Assert
        Assert.AreEqual(inspectionPopupStringsSO.IRInfo, inspectionPopupStringsSO.CurrentInspectionMesssage);
    }

    /// <summary>
    /// Ensure that before the inspection is completed, the current probe inspection message is set to the correct value
    /// </summary> 
    [UnityTest]
    [Category("BuildServer")]
    public IEnumerator EnsureProbeInspectionMessageIsSetCorrectly()
    {
        //Arrange 
        var toolUsed = VARLab.PublicHealth.Tools.ProbeThermometer;
        inspectionPopupStringsSO.ProbeInfo = "Probe Thermometer inspection test.";

        //Act
        inspectionPopupStringsSO.SetCurrentInspectionMessage(toolUsed, null);

        yield return null;

        //Assert
        Assert.AreEqual(inspectionPopupStringsSO.ProbeInfo, inspectionPopupStringsSO.CurrentInspectionMesssage);
    }

    /// <summary>
    /// Ensure that the previous compliant photo inspection message is set correctly
    [UnityTest]
    [Category("BuildServer")]
    public IEnumerator EnsurePreviousCompliantPhotoInspectionIsSetCorrectly()
    {
        //Arrange 
        var toolUsed = VARLab.PublicHealth.Tools.Visual;
        inspectionPopupStringsSO.VisualPhotoInspection = "Visual inspection photo test.";
        inspectionPopupStringsSO.CompliantEnding = "compliant.";

        //Act
        inspectionPopupStringsSO.SetCurrentInspectionMessage(toolUsed, true, true);

        yield return null;

        //Assert
        Assert.AreEqual(inspectionPopupStringsSO.VisualPhotoInspection + inspectionPopupStringsSO.CompliantEnding, inspectionPopupStringsSO.CurrentInspectionMesssage);
    }

    /// <summary>
    /// Ensure that the previous non-compliant photo inspection message is set correctly
    /// </summary>  
    [UnityTest]
    [Category("BuildServer")]
    public IEnumerator EnsurePreviousNonCompliantPhotoInspectionIsSetCorrectly()
    {
        //Arrange 
        var toolUsed = VARLab.PublicHealth.Tools.Visual;
        inspectionPopupStringsSO.VisualPhotoInspection = "Visual inspection photo test.";
        inspectionPopupStringsSO.NonCompliantEnding = "non-compliant.";

        //Act
        inspectionPopupStringsSO.SetCurrentInspectionMessage(toolUsed, false, true);

        yield return null;

        //Assert
        Assert.AreEqual(inspectionPopupStringsSO.VisualPhotoInspection + inspectionPopupStringsSO.NonCompliantEnding, inspectionPopupStringsSO.CurrentInspectionMesssage);
    }

    /// <summary>
    /// Ensure that the previous compliant visual inspection message is set correctly
    /// </summary>
    [UnityTest]
    [Category("BuildServer")]
    public IEnumerator EnsurePreviousCompliantVisualInspectionIsSetCorrectly()
    {
        //Arrange 
        var toolUsed = VARLab.PublicHealth.Tools.Visual;
        inspectionPopupStringsSO.VisualInspection = "Visual inspection test.";
        inspectionPopupStringsSO.CompliantEnding = "compliant.";

        //Act
        inspectionPopupStringsSO.SetCurrentInspectionMessage(toolUsed, true);

        yield return null;

        //Assert
        Assert.AreEqual(inspectionPopupStringsSO.VisualInspection + inspectionPopupStringsSO.CompliantEnding, inspectionPopupStringsSO.CurrentInspectionMesssage);
    }

    /// <summary>
    /// Ensure that the previous non-compliant visual inspection message is set correctly
    /// </summary> 
    [UnityTest]
    [Category("BuildServer")]
    public IEnumerator EnsurePreviousNonCompliantVisualInspectionIsSetCorrectly()
    {
        //Arrange 
        var toolUsed = VARLab.PublicHealth.Tools.Visual;
        inspectionPopupStringsSO.VisualInspection = "Visual inspection test.";
        inspectionPopupStringsSO.NonCompliantEnding = "non-compliant.";

        //Act
        inspectionPopupStringsSO.SetCurrentInspectionMessage(toolUsed, false);

        yield return null;

        //Assert
        Assert.AreEqual(inspectionPopupStringsSO.VisualInspection + inspectionPopupStringsSO.NonCompliantEnding, inspectionPopupStringsSO.CurrentInspectionMesssage);
    }

    /// <summary>
    /// Ensure that the previous compliant IR inspection message is set correctly
    /// </summary> 
    [UnityTest]
    [Category("BuildServer")]
    public IEnumerator EnsurePreviousCompliantIRInspectionIsSetCorrectly()
    {
        //Arrange 
        var toolUsed = VARLab.PublicHealth.Tools.IRThermometer;
        inspectionPopupStringsSO.IRInspection = "IR Thermometer inspection test.";
        inspectionPopupStringsSO.CompliantEnding = "compliant.";

        //Act
        inspectionPopupStringsSO.SetCurrentInspectionMessage(toolUsed, true);

        yield return null;

        //Assert
        Assert.AreEqual(inspectionPopupStringsSO.IRInspection + inspectionPopupStringsSO.CompliantEnding, inspectionPopupStringsSO.CurrentInspectionMesssage);
    }

    /// <summary>
    /// Ensure that the previous non-compliant IR inspection message is set correctly
    /// </summary>
    [UnityTest]
    [Category("BuildServer")]
    public IEnumerator EnsurePreviousNonCompliantIRInspectionIsSetCorrectly()
    {
        //Arrange 
        var toolUsed = VARLab.PublicHealth.Tools.IRThermometer;
        inspectionPopupStringsSO.IRInspection = "IR Thermometer inspection test.";
        inspectionPopupStringsSO.NonCompliantEnding = "non-compliant.";

        //Act
        inspectionPopupStringsSO.SetCurrentInspectionMessage(toolUsed, false);

        yield return null;

        //Assert
        Assert.AreEqual(inspectionPopupStringsSO.IRInspection + inspectionPopupStringsSO.NonCompliantEnding, inspectionPopupStringsSO.CurrentInspectionMesssage);
    }

    /// <summary>
    /// Ensure that the previous compliant probe inspection message is set correctly
    /// </summary>
    [UnityTest]
    [Category("BuildServer")]
    public IEnumerator EnsurePreviousCompliantProbeInspectionIsSetCorrectly()
    {
        //Arrange 
        var toolUsed = VARLab.PublicHealth.Tools.ProbeThermometer;
        inspectionPopupStringsSO.ProbeInspection = "Probe Thermometer inspection test.";
        inspectionPopupStringsSO.CompliantEnding = "compliant.";

        //Act
        inspectionPopupStringsSO.SetCurrentInspectionMessage(toolUsed, true);

        yield return null;

        //Assert
        Assert.AreEqual(inspectionPopupStringsSO.ProbeInspection + inspectionPopupStringsSO.CompliantEnding, inspectionPopupStringsSO.CurrentInspectionMesssage);
    }

    /// <summary>
    /// Ensure that the previous non-compliant probe inspection message is set correctly
    /// </summary>
    [UnityTest]
    [Category("BuildServer")]
    public IEnumerator EnsurePreviousNonCompliantProbeInspectionIsSetCorrectly()
    {
        //Arrange 
        var toolUsed = VARLab.PublicHealth.Tools.ProbeThermometer;
        inspectionPopupStringsSO.ProbeInspection = "Probe Thermometer inspection test.";
        inspectionPopupStringsSO.NonCompliantEnding = "non-compliant.";

        //Act
        inspectionPopupStringsSO.SetCurrentInspectionMessage(toolUsed, false);

        yield return null;

        //Assert
        Assert.AreEqual(inspectionPopupStringsSO.ProbeInspection + inspectionPopupStringsSO.NonCompliantEnding, inspectionPopupStringsSO.CurrentInspectionMesssage);
    }



}
