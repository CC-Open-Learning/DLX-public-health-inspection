using NUnit.Framework;
using System;
using VARLab.PublicHealth;

public class ProgressBarTests
{
    public ProgressBar _progressBar;
    public TimeSpan ts;

    //set of constants to compare to the progress bar strings 
    //These string values are derived from what is to be displayed on the progress bar UI (From Figma)  
    public const string LocationProgress = "0/10";
    public const string NonComplianceProgress = "6";
    public const string TimeProgress = "1 hr";

    [SetUp]
    [Category("BuildServer")]
    public void Setup()
    {
        _progressBar = new ProgressBar();
        ts = new TimeSpan(1, 0, 0);
    }


    [Test]
    [Category("BuildServer")]
    public void EnsureUpdateProgressCorrectlySetsValues()
    {
        //This method is used to build the progress bar by calling the UpdateProgress method updating the string values inside the progress bar
        //poi count = 10, visited poi count = 0, non-compliance count = 6, timer = 1 hour
        _progressBar.UpdateProgress(10, 0, 6, new TimeSpan(1, 0, 0));

        //check to see if the progress bar strings are equal to the constants
        Assert.AreEqual(LocationProgress, _progressBar.LocationProgress);
        Assert.AreEqual(NonComplianceProgress, _progressBar.NonComplianceProgress);
        Assert.AreEqual(TimeProgress, _progressBar.TimeProgressString);

    }

    [Test]
    [Category("BuildServer")]
    public void EnsureDataIsCorrectlyChangedWhenCalledAgain()
    {
        //write a test to change the values of the progress bar class and ensure that all values are changed correctly
        _progressBar.UpdateProgress(10, 0, 6, new TimeSpan(1, 0, 0));

        //check current values
        Assert.AreEqual(LocationProgress, _progressBar.LocationProgress);
        Assert.AreEqual(NonComplianceProgress, _progressBar.NonComplianceProgress);

        //change values
        _progressBar.UpdateProgress(20, 10, 12, new TimeSpan(2, 0, 0));

        //check new values
        Assert.AreEqual("10/20", _progressBar.LocationProgress);
        Assert.AreEqual("12", _progressBar.NonComplianceProgress);
        Assert.AreEqual("2 hrs", _progressBar.TimeProgressString);
    }

    [Test]
    [Category("BuildServer")]
    public void EnsureConvertingTimeSpanToStringIsCorrect()
    {
        //write a test to ensure that the time span is correctly converted to a string
        //test 1
        TimeSpan ts = new TimeSpan(1, 0, 0);
        string s = _progressBar.ConvertTimeSpanToString(ts);
        Assert.AreEqual("1 hr", s);

        //test 2
        ts = new TimeSpan(2, 0, 0);
        s = _progressBar.ConvertTimeSpanToString(ts);
        Assert.AreEqual("2 hrs", s);

        //test 3
        ts = new TimeSpan(0, 0, 0);
        s = _progressBar.ConvertTimeSpanToString(ts);
        Assert.AreEqual("", s);

        //test 4
        ts = new TimeSpan(0, 1, 0);
        s = _progressBar.ConvertTimeSpanToString(ts);
        Assert.AreEqual("1 min", s);

        //test 5
        ts = new TimeSpan(0, 2, 0);
        s = _progressBar.ConvertTimeSpanToString(ts);
        Assert.AreEqual("2 mins", s);

        //test 6
        ts = new TimeSpan(0, 0, 1);
        s = _progressBar.ConvertTimeSpanToString(ts);
        Assert.AreEqual("1 sec", s);

        //test 7
        ts = new TimeSpan(0, 0, 2);
        s = _progressBar.ConvertTimeSpanToString(ts);
        Assert.AreEqual("2 secs", s);

        //test 8
        ts = new TimeSpan(1, 1, 1);
        s = _progressBar.ConvertTimeSpanToString(ts);
        Assert.AreEqual("1 hr 1 min", s);

        //test 9
        ts = new TimeSpan(2, 2, 2);
        s = _progressBar.ConvertTimeSpanToString(ts);
        Assert.AreEqual("2 hrs 2 mins", s);

        //test 10
        ts = new TimeSpan(1, 2, 3);
        s = _progressBar.ConvertTimeSpanToString(ts);
        Assert.AreEqual("1 hr 2 mins", s);

        //test 11
        ts = new TimeSpan(0, 1, 2);
        s = _progressBar.ConvertTimeSpanToString(ts);
        Assert.AreEqual("1 min 2 secs", s);
    }
}
