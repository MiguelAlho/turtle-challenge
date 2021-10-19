using FluentAssertions;
using Xunit;

namespace TurtleChallenge.Tests;

public class DirectionExtensionsTests
{
    [Fact]
    public void RotateReturnsNewDirection()
    {
        var direction = Direction.North;
        var newDirection = direction.Rotate();

        newDirection.Should().Be(Direction.East);
    }
}
