using FluentAssertions;
using Xunit;

namespace TurtleChallenge.Tests;

public class DirectionExtensionsTests
{
    [Theory]
    [InlineData(Direction.North, Direction.East)]
    [InlineData(Direction.East, Direction.South)]
    [InlineData(Direction.South, Direction.West)]
    [InlineData(Direction.West, Direction.North)]
    internal void RotateReturnsNewDirection(Direction input, Direction expected)
    {
        var direction = input;
        var newDirection = direction.Rotate();

        newDirection.Should().Be(expected);
    }
}
