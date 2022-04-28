using System.Linq;
using System.Threading.Tasks;
using WebApi.Models;
using Xunit;

namespace Test;

public class UnitTest1 : IClassFixture<TestDatabaseFixture>
{
    public UnitTest1(TestDatabaseFixture fixture)
        => Fixture = fixture;
    public TestDatabaseFixture Fixture { get; }
    
    [Fact]
    public void Test1()
    {
        using var context = Fixture.CreateContext();
        Assert.True(context.Users.Count() == 1);
    }
}
