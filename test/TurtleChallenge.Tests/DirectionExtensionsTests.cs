using FluentAssertions;
using System;
using TurtleChallengeConsole;
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

    [Theory]
    [InlineData('N', Direction.North)]
    [InlineData('E', Direction.East)]
    [InlineData('S', Direction.South)]
    [InlineData('W', Direction.West)]
    internal void MaprFromCharacterReturnsNewDirection(char input, Direction expected) 
        => input.MapToDirection().Should().Be(expected);

    [Fact]
    public void InvalidCharThrowsIfMappingToDirection()
    {
        var action = () => 'X'.MapToDirection();
        action.Should().Throw<ArgumentOutOfRangeException>();
    }
}
