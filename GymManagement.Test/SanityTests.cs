using FluentAssertions;

namespace GymManagement.Tests;

public class SanityTests
{
    [Fact]
    public void TestProject_Should_Run()
    {
        true.Should().BeTrue();
    }
}