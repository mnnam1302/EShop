namespace EShop.Identity.Tests;

public class Test : IntegrationTest
{
    public Test(IntegrationFixture integrationFixture) : base(integrationFixture)
    {
    }

    [Fact]
    public void Do()
    {
        Console.WriteLine("Hello");
    }
}