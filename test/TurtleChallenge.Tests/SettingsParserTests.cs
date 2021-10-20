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

        settings.BoardSize.Columns.Should().Be(builder.BoardSizeColumns);
        settings.BoardSize.Rows.Should().Be(builder.BoardSizeRows);
        settings.TurtleStart.Start.Column.Should().Be(builder.TurtleStartColumn);
        settings.TurtleStart.Start.Row.Should().Be(builder.TurtleStartRow);
        settings.TurtleStart.Direction.Should().Be(builder.TurtleStartDirection.MapToDirection());
        settings.Exit.Column.Should().Be(builder.ExitColumn);
        settings.Exit.Row.Should().Be(builder.ExitRow);

        settings.Mines.Count.Should().Be(0);
    }

    [Fact]
    public void ValidFileWithMinesParsesCorrectly()
    {
        var builder = new SettingsFileBuilder();
        var settings = new SettingsParser().Parse(builder.Build());

        settings.BoardSize.Columns.Should().Be(builder.BoardSizeColumns);
        settings.BoardSize.Rows.Should().Be(builder.BoardSizeRows);
        settings.TurtleStart.Start.Column.Should().Be(builder.TurtleStartColumn);
        settings.TurtleStart.Start.Row.Should().Be(builder.TurtleStartRow);
        settings.TurtleStart.Direction.Should().Be(builder.TurtleStartDirection.MapToDirection());
        settings.Exit.Column.Should().Be(builder.ExitColumn);
        settings.Exit.Row.Should().Be(builder.ExitRow);

        //compare mines
        settings.Mines.Count.Should().Be(builder.Mines.Count());
        foreach (var mine in builder.Mines)
            settings.Mines.Contains(new Coordinate(mine.column, mine.row)).Should().BeTrue();
    }

    //TODO: invalid cases....too exaustive to do at the moment for a sample
}



public class SettingsFileBuilder
{
    private static Fixture _fixture = new Fixture();

    //use int to be permissive on error creation
    public int BoardSizeColumns { get; private set; } = _fixture.CreateMaxedNonZeroInt(SettingsParser.MaxBoardSize);
    public int BoardSizeRows { get; private set; } = _fixture.CreateMaxedNonZeroInt(SettingsParser.MaxBoardSize);

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

        TurtleStartColumn = _fixture.CreateMaxedPositiveInt(BoardSizeColumns);
        TurtleStartRow = _fixture.CreateMaxedPositiveInt(BoardSizeRows);
        TurtleStartDirection = CreateValidDirectionLetter();

        ExitColumn = _fixture.CreateMaxedPositiveInt(BoardSizeColumns);
        ExitRow = _fixture.CreateMaxedPositiveInt(BoardSizeRows);

        mines = CreateMines();
    }

    public SettingsFileBuilder ClearMines()
    {
        mines = new List<(int column, int row)>();
        return this;
    }

    public SettingsFileBuilder AddSingleMine()
    {
        mines.Add(new(_fixture.CreateMaxedPositiveInt(BoardSizeColumns), _fixture.CreateMaxedPositiveInt(BoardSizeRows)));
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
            newMines.Add(new(_fixture.CreateMaxedPositiveInt(BoardSizeColumns), _fixture.CreateMaxedPositiveInt(BoardSizeRows)));

        return newMines;
    }
}

public static class FixtureExtensions
{
    public static int CreateMaxedPositiveInt(this IFixture fixture, int max) 
        => Random.Shared.Next(max);

    public static int CreateMaxedNonZeroInt(this IFixture fixture, int max)
    {
        var value = fixture.CreateMaxedPositiveInt(max);
        return value == 0
            ? value + 1
            : value;

    }
}
