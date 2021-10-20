using FluentAssertions;
using System.Collections.Generic;
using System.Linq;
using TurtleChallengeConsole;
using Xunit;

namespace TurtleChallenge.Tests;

public class SimulatorTests
{
    static Settings SimpleBoard() => new Settings(
        new BoardSize(2, 2),
        new TurtleStart(new BoundedCoordinate(0, 0), Direction.East),
        new BoundedCoordinate(1,1),
        new HashSet<BoundedCoordinate>() { new BoundedCoordinate(0,1),}
    );

    static Settings SimpleBoardWithMineAtUpperLeftcorner() => new Settings(
        new BoardSize(2, 2),
        new TurtleStart(new BoundedCoordinate(0, 0), Direction.East),
        new BoundedCoordinate(1, 1),
        new HashSet<BoundedCoordinate>() { new BoundedCoordinate(1,0), }
    );

    const string OkSequence = "mrm";
    const string MineSequence = "rm";
    const string StuckSequence = "m";
    const string OOBSequence_Up = "rrrm";
    const string OOBSequence_Left = "mm";
    const string OOBSequence_Right = "rrm";
    const string OOBSequence_Down = "rmm"; //needs SimpleBoardWithMineAtUpperLeftcorner


    [Fact]
    public void SimulationReachingExitReturnsSuccess()
    {
        var simulator = new Simulator(SimpleBoard(), MoveArrayFrom(OkSequence));
        simulator.RunSequence().Should().Be(SimulationResult.Success);
    }

    [Fact]
    public void SimulationHittingMineReturnsMineHit()
    {
        var simulator = new Simulator(SimpleBoard(), MoveArrayFrom(MineSequence));
        simulator.RunSequence().Should().Be(SimulationResult.MineHit);
    }

    [Fact]
    public void SimulationWithoutReachingExitNorExplodingReturnsStillInDanger()
    {
        var simulator = new Simulator(SimpleBoard(), MoveArrayFrom(StuckSequence));
        simulator.RunSequence().Should().Be(SimulationResult.StillInDanger);
    }

    [Theory]
    [InlineData(OOBSequence_Up)]
    [InlineData(OOBSequence_Left)]
    [InlineData(OOBSequence_Right)]
    public void SimulationMovingOutOfBoundReturnsOutOfBounds(string inputSequence)
    {
        var simulator = new Simulator(SimpleBoard(), MoveArrayFrom(inputSequence));
        simulator.RunSequence().Should().Be(SimulationResult.OutOfBounds);
    }

    [Fact]
    public void SimulationMovingOutOfBoundSouthReturnsOutOfBounds()
    {
        var simulator = new Simulator(SimpleBoardWithMineAtUpperLeftcorner(), MoveArrayFrom(OOBSequence_Down));
        simulator.RunSequence().Should().Be(SimulationResult.OutOfBounds);
    }

    Moves[] MoveArrayFrom(string moves)
    {
        return moves.Select(m => SequenceFileParser.MoveFrom(m)).ToArray();
    }
}

