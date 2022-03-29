using WebApi.Models;
using Xunit;

namespace Test;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        var c = new Category("test", null);
        Assert.Equal("test", c.Name);
    }
}