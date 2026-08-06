using Core;

namespace Tests;

public class UnitTest1
{
    [Fact]
    public void ContainerTest()
    {
        var container = new Container<List<string>>();
        Assert.Equal(default, container.Item);
    }
}
