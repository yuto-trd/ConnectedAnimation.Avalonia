using Avalonia;
using Avalonia.ConnectedAnimation;

namespace TestProject1;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Above()
    {
        var expect = RelativeLocation.Above;
        var actual = Helper.GetRelativeLocation(
            new Rect(70, 374, 60, 60),
            new Rect(50, 50, 150, 100));

        Assert.That(actual, Is.EqualTo(expect));
    }
    
    [Test]
    public void Below()
    {
        var expect = RelativeLocation.Below;
        var actual = Helper.GetRelativeLocation(
            new Rect(50, 50, 150, 100),
            new Rect(70, 374, 60, 60));

        Assert.That(actual, Is.EqualTo(expect));
    }
    
    [Test]
    public void Right()
    {
        var expect = RelativeLocation.Right;
        var actual = Helper.GetRelativeLocation(
            new Rect(50, 50, 150, 100),
            new Rect(374, 70, 60, 60));

        Assert.That(actual, Is.EqualTo(expect));
    }

    [Test]
    public void Left()
    {
        var expect = RelativeLocation.Left;
        var actual = Helper.GetRelativeLocation(
            new Rect(374, 70, 60, 60),
            new Rect(50, 50, 150, 100));

        Assert.That(actual, Is.EqualTo(expect));
    }
}