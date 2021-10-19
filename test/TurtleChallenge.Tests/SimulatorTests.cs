﻿using FluentAssertions;
using System;
using System.Collections.Generic;
using Xunit;

namespace TurtleChallenge.Tests;

public class SimulatorTests
{
    static Settings SimpleBoard() => new Settings(
        new BoardSize(2, 2),
        new TurtleStart(new BoundedCoordinate(0, 0), Direction.East),
        new BoundedCoordinate(1,1),
        new List<BoundedCoordinate>() { new BoundedCoordinate(0,1),}
    );

    static Settings SimpleBoardWithMineAtUpperLeftcorner() => new Settings(
        new BoardSize(2, 2),
        new TurtleStart(new BoundedCoordinate(0, 0), Direction.East),
        new BoundedCoordinate(1, 1),
        new List<BoundedCoordinate>() { new BoundedCoordinate(1,0), }
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
        var simulator = new Simulator(SimpleBoard());
        simulator.RunSequence(OkSequence.ToMoveArray()).Should().Be(SimulationResult.Success);
    }

    [Fact]
    public void SimulationHittingMineReturnsMineHit()
    {
        var simulator = new Simulator(SimpleBoard());
        simulator.RunSequence(MineSequence.ToMoveArray()).Should().Be(SimulationResult.MineHit);
    }

    [Fact]
    public void SimulationWithoutReachingExitNorExplodingReturnsStillInDanger()
    {
        var simulator = new Simulator(SimpleBoard());
        simulator.RunSequence(StuckSequence.ToMoveArray()).Should().Be(SimulationResult.StillInDanger);
    }

    [Theory]
    [InlineData(OOBSequence_Up)]
    [InlineData(OOBSequence_Left)]
    [InlineData(OOBSequence_Right)]
    public void SimulationMovingOutOfBoundReturnsOutOfBounds(string inputSequence)
    {
        var simulator = new Simulator(SimpleBoard());
        simulator.RunSequence(inputSequence.ToMoveArray()).Should().Be(SimulationResult.OutOfBounds);
    }

    [Fact]
    public void SimulationMovingOutOfBoundSouthReturnsOutOfBounds()
    {
        var simulator = new Simulator(SimpleBoardWithMineAtUpperLeftcorner());
        simulator.RunSequence(OOBSequence_Down.ToMoveArray()).Should().Be(SimulationResult.OutOfBounds);
    }
}
