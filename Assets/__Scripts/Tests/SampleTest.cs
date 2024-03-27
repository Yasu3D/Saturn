using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class SampleTest
{
    [Test]
    public void SampleTestAlwaysPasses()
    {
        Assert.IsTrue(1 + 1 == 2);
    }

    [UnityTest]
    public IEnumerator SampleUnityTestAlwaysPasses()
    {
        float prevTime = Time.time;
        // Use yield to skip a frame.
        yield return null;
        Assert.IsTrue(Time.time > prevTime);
    }
}
