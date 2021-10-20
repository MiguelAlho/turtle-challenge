using AutoFixture;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using TurtleChallengeConsole;
using Xunit;

namespace TurtleChallenge.Tests;

public class SettingsParserTests
{
    [Fact]
    public void ParseThrowsOnEmptyFile()
    {
        var action = () => new SettingsParser().Parse(new string[] { });
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ValidFileWithNoMinesParsesCorrectly()
    {
        var builder = new SettingsFileBuilder().ClearMines();
        var settings = new SettingsParser().Parse(builder.Build());

        settings.BoardSize.Columns.Should().Be((ushort)builder.BoardSizeColumns);
        settings.BoardSize.Rows.Should().Be((ushort)builder.BoardSizeRows);
        settings.TurtleStart.Start.Column.Should().Be((ushort)builder.TurtleStartColumn);
        settings.TurtleStart.Start.Row.Should().Be((ushort)builder.TurtleStartRow);
        settings.TurtleStart.Direction.Should().Be(builder.TurtleStartDirection.MapToDirection());
        settings.Exit.Column.Should().Be((ushort)builder.ExitColumn);
        settings.Exit.Row.Should().Be((ushort)builder.ExitRow);

        settings.Mines.Count.Should().Be(0);
    }

    [Fact]
    public void ValidFileWithMinesParsesCorrectly()
    {
        var builder = new SettingsFileBuilder();
        var settings = new SettingsParser().Parse(builder.Build());

        settings.BoardSize.Columns.Should().Be((ushort)builder.BoardSizeColumns);
        settings.BoardSize.Rows.Should().Be((ushort)builder.BoardSizeRows);
        settings.TurtleStart.Start.Column.Should().Be((ushort)builder.TurtleStartColumn);
        settings.TurtleStart.Start.Row.Should().Be((ushort)builder.TurtleStartRow);
        settings.TurtleStart.Direction.Should().Be(builder.TurtleStartDirection.MapToDirection());
        settings.Exit.Column.Should().Be((ushort)builder.ExitColumn);
        settings.Exit.Row.Should().Be((ushort)builder.ExitRow);

        //compare mines
        settings.Mines.Count.Should().Be(builder.Mines.Count());
        foreach (var mine in builder.Mines)
            settings.Mines.Contains(new BoundedCoordinate((ushort)mine.column, (ushort)mine.row)).Should().BeTrue();
    }

    //TODO: invalid cases....too exaustive to do at the moment for a sample
}



public class SettingsFileBuilder
{
    private static Fixture _fixture = new Fixture();

    //use int to be permissive on error creation
    public int BoardSizeColumns { get; private set; } = _fixture.CreateNonZeroShortAsInt();
    public int BoardSizeRows { get; private set; } = _fixture.CreateNonZeroShortAsInt();

    public int TurtleStartColumn { get; private set; }
    public int TurtleStartRow { get; private set; }
    public char TurtleStartDirection { get; private set; }

    public int ExitColumn { get; private set; }
    public int ExitRow { get; private set; }

    private List<(int column, int row)> mines;
    public IEnumerable<(int column, int row)> Mines => mines;

    public SettingsFileBuilder()
    {
        _fixture.Customize<ushort>(c => c.FromSeed(s => 1));

        TurtleStartColumn = _fixture.CreateMaxedShortAsInt(BoardSizeColumns);
        TurtleStartRow = _fixture.CreateMaxedShortAsInt(BoardSizeRows);
        TurtleStartDirection = CreateValidDirectionLetter();

        ExitColumn = _fixture.CreateMaxedShortAsInt(BoardSizeColumns);
        ExitRow = _fixture.CreateMaxedShortAsInt(BoardSizeRows);

        mines = CreateMines();
    }

    public SettingsFileBuilder ClearMines()
    {
        mines = new List<(int column, int row)>();
        return this;
    }

    public SettingsFileBuilder AddSingleMine()
    {
        mines.Add(new(_fixture.CreateMaxedShortAsInt(BoardSizeColumns), _fixture.CreateMaxedShortAsInt(BoardSizeRows)));
        return this;
    }

    
    public string[] Build() 
        => new string[]
            {
                $"{BoardSizeColumns},{BoardSizeRows}",
                $"{TurtleStartColumn},{TurtleStartRow},{TurtleStartDirection}",
                $"{ExitColumn},{ExitRow}"
            }
        .Concat(mines.Select(x => $"{x.column},{x.row}"))
        .ToArray();

    char CreateValidDirectionLetter()
        => SettingsParser.ValidDirections[new Random().Next(SettingsParser.ValidDirections.Length)];

    List<(int column, int row)> CreateMines()
    {
        //3 for now as a default
        var newMines = new List<(int column, int row)>();
        for (int i = 0; i < 3; i++)
            newMines.Add(new(_fixture.CreateMaxedShortAsInt(BoardSizeColumns), _fixture.CreateMaxedShortAsInt(BoardSizeRows)));

        return newMines;
    }
}

public static class FixtureExtensions
{
    public static int CreateMaxedShortAsInt(this IFixture fixture, int max) 
        => Random.Shared.Next(max);

    public static int CreateNonZeroShortAsInt(this IFixture fixture)
    {
        var val = fixture.CreateMaxedShortAsInt((int)ushort.MaxValue);
        return val == 0
            ? val + 1
            : val;
    }
}
