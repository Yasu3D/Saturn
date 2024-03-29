using NUnit.Framework;
using SaturnGame;

public class SaturnMathTests
{
    [Test]
    public void FractionToDecibelTest()
    {
        Assert.AreEqual(-80f, SaturnMath.FractionToDecibel(0f), 0.001f);

        Assert.Greater(SaturnMath.FractionToDecibel(0.001f), -80f);
        Assert.Less(SaturnMath.FractionToDecibel(0.001f), -75f);

        Assert.Greater(SaturnMath.FractionToDecibel(0.25f), -80f);
        Assert.Less(SaturnMath.FractionToDecibel(0.25f), -7f);

        Assert.Greater(SaturnMath.FractionToDecibel(0.499f), -7.1f);
        Assert.Less(SaturnMath.FractionToDecibel(0.499f), -7f);

        Assert.AreEqual( -7f, SaturnMath.FractionToDecibel(0.5f), 0.001f);

        // Slope around the crossover should not be discontinuous
        float leftSideDiff = SaturnMath.FractionToDecibel(0.5f) - SaturnMath.FractionToDecibel(0.499f);
        float rightSideDiff = SaturnMath.FractionToDecibel(0.501f) - SaturnMath.FractionToDecibel(0.5f);
        Assert.AreEqual(leftSideDiff, rightSideDiff, 0.001f);

        Assert.Greater(SaturnMath.FractionToDecibel(0.501f), -7f);
        Assert.Less(SaturnMath.FractionToDecibel(0.501f), -6.9f);

        Assert.AreEqual( 0f, SaturnMath.FractionToDecibel(1f), 0.001f);
    }

    [Test]
    public void DecibelToFractionTest()
    {
        Assert.AreEqual(0f, SaturnMath.DecibelToFraction(-80f), 0.001f);

        Assert.Greater(SaturnMath.DecibelToFraction(-79.9f), 0f);
        Assert.Less(SaturnMath.DecibelToFraction(-79.9f), 0.1f);

        Assert.Greater(SaturnMath.DecibelToFraction(-30f), 0f);
        Assert.Less(SaturnMath.DecibelToFraction(-30f), 0.5f);

        Assert.Greater(SaturnMath.DecibelToFraction(-7.001f), 0.49f);
        Assert.Less(SaturnMath.DecibelToFraction(-7.001f), 0.5f);

        Assert.AreEqual(0.5f, SaturnMath.DecibelToFraction(-7f), 0.001f);

        // Slope around the crossover should not be discontinuous
        float leftSideDiff = SaturnMath.DecibelToFraction(-7f) - SaturnMath.DecibelToFraction(-7.001f);
        float rightSideDiff = SaturnMath.DecibelToFraction(-6.999f) - SaturnMath.DecibelToFraction(-7f);
        Assert.AreEqual(leftSideDiff, rightSideDiff, 0.001f);

        Assert.Greater(SaturnMath.DecibelToFraction(-6.999f), 0.5f);
        Assert.Less(SaturnMath.DecibelToFraction(-6.999f), 0.51f);

        Assert.AreEqual(1f, SaturnMath.DecibelToFraction(0f), 0.001f);
    }

    [Test]
    public void FractionToDecibelAndBackTest()
    {
        // 1 / 0.001 = 1000 iterations
        for (float fraction = 0f; fraction <= 1f; fraction += 0.001f)
            Assert.AreEqual(fraction, SaturnMath.DecibelToFraction(SaturnMath.FractionToDecibel(fraction)), 0.001f);

        // 80 / 0.1 = 800 iterations
        for (float decibel = -80f; decibel <= 0f; decibel += 0.1f)
            Assert.AreEqual(decibel, SaturnMath.FractionToDecibel(SaturnMath.DecibelToFraction(decibel)), 0.001f);
    }
}