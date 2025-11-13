using FluentAssertions;
using Xunit;

namespace LineageLauncher.IntegrationTests;

public sealed class BasicIntegrationTest
{
    [Fact]
    public void Solution_ShouldCompile()
    {
        // This test simply verifies that all projects compile together correctly
        // More comprehensive integration tests will be added later
        true.Should().BeTrue();
    }
}
