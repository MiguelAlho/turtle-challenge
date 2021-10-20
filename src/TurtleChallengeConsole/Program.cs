﻿using System.Collections;

namespace TurtleChallengeConsole;

public class Program
{
    public static int Main(string[] args)
    {
        if (args.Length != 2)
        {
            Console.WriteLine("Invalid arguments for the application");
            Console.WriteLine("Usage: .\\TurtleChallengeConsole.exe <settings file path> <sequence file path>");
            return -1;
        }

        var settingsFile = args[0];
        var sequenceFile = args[1];

        //read settings file and setup simulator
        Settings settings;
        try
        {
            string[] settingsFileLines = File.ReadAllLines(settingsFile);
            settings = new SettingsParser().Parse(settingsFileLines);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Invalid settings file data");
            Console.WriteLine($"{ex.Message}");
            return -2;
        }

        Sequences sequences;
        try
        {
            string[] sequenceFileLines = File.ReadAllLines(sequenceFile);
            sequences = new SequenceFileParser().Parse(sequenceFileLines);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Invalid settings file data");
            Console.WriteLine($"{ex.Message}");
            return -3;
        }

        //read sequences and process each one
        int i = 1;
        foreach (Moves[] sequence in sequences)
        {
            var result = new Simulator(settings, sequence).RunSequence();

            Console.WriteLine($"Sequence {i}: {GetOutputForResult(result)}!");
            i++;
        }

        return 0;
    }

    private static object GetOutputForResult(SimulationResult result) => result switch
    {
        SimulationResult.Success => "Success",
        SimulationResult.MineHit => "Mine Hit",
        SimulationResult.StillInDanger => "Still in Danger",
        SimulationResult.OutOfBounds => "Out of Bounds",
        _ => throw new NotImplementedException(),
    };
}

/// <summary>
/// Setups up a new board to run a sequence
/// </summary>
class Simulator
{
    private readonly Settings _settings;
    private readonly Moves[] _moves;

    private Coordinate _turtlePosition;
    private Direction _direction;

    public Simulator(Settings settings, Moves[] moves)
    {
        _settings = settings;
        _moves = moves;

        _turtlePosition = new Coordinate(settings.TurtleStart.Start.Column, settings.TurtleStart.Start.Row);
        _direction = settings.TurtleStart.Direction;
    }

    public SimulationResult RunSequence()
    {
        foreach (var move in _moves)
        {
            MutateTurtle(move);

            if (IsTurtleOutOfBounds())
                return SimulationResult.OutOfBounds;

            if (IsTurtleAtMine())
                return SimulationResult.MineHit;

            if (IsTurtleAtExit())
                return SimulationResult.Success;
        }

        return SimulationResult.StillInDanger;
    }

    private bool IsTurtleAtExit()
        => _turtlePosition.Column == _settings.Exit.Column
            && _turtlePosition.Row == _settings.Exit.Row;

    private bool IsTurtleAtMine()
        => _settings.Mines.Contains(new Coordinate((ushort)_turtlePosition.Column, (ushort)_turtlePosition.Row));

    private bool IsTurtleOutOfBounds()
        => _turtlePosition.Column < 0
            || _turtlePosition.Column >= _settings.BoardSize.Columns
            || _turtlePosition.Row < 0
            || _turtlePosition.Row >= _settings.BoardSize.Rows;

    private void MutateTurtle(Moves move)
    {
        switch (move)
        {
            case Moves.Move:
                _turtlePosition = GetNewTurtlePosition(_direction);
                break;
            case Moves.Rotate:
                _direction = _direction.Rotate();
                break;
            default:
                throw new NotImplementedException();
                break;
        }

        //is within bounds?
    }

    private Coordinate GetNewTurtlePosition(Direction direction) => direction switch
    {
        Direction.North => new Coordinate(_turtlePosition.Column, _turtlePosition.Row - 1),
        Direction.East => new Coordinate(_turtlePosition.Column + 1, _turtlePosition.Row),
        Direction.South => new Coordinate(_turtlePosition.Column, _turtlePosition.Row + 1),
        Direction.West => new Coordinate(_turtlePosition.Column - 1, _turtlePosition.Row),
        _ => throw new ArgumentOutOfRangeException("Unknown direction submitted")
    };
}

enum SimulationResult
{
    StillInDanger,
    MineHit,
    Success,
    OutOfBounds
}


class SequenceFileParser
{
    public static char[] ValidActions = new char[] { 'm', 'r' };

    internal Sequences Parse(string[] sequenceFileLines)
    {
        if (sequenceFileLines.Length == 0)
        {
            throw new ArgumentException("No sequences found in file.");
        }

        List<Moves[]> sequences = new List<Moves[]>();

        foreach (var line in sequenceFileLines)
        {
            var moveSymbols = line.Trim().ToCharArray();

            if (moveSymbols.Length == 0)
                throw new InvalidDataException("Empty sequences are not allowed");

            foreach (var symbol in moveSymbols)
            {
                if (!ValidActions.Contains(symbol))
                    throw new ArgumentException($"Invalid symbol '{symbol}' found in sequence file.");
            }

            sequences.Add(moveSymbols.Select(m => MoveFrom(m)).ToArray());
        }

        return new Sequences(sequences);
    }

    public static Moves MoveFrom(char symbol) => symbol switch
    {
        'm' => Moves.Move,
        'r' => Moves.Rotate,
        _ => throw new ArgumentOutOfRangeException($"Unsupported move symbol '{symbol}' found")
    };
}


class SettingsParser
{
    public static char[] ValidDirections = new char[] { 'N', 'E', 'S', 'W' };
    public static readonly int MaxBoardSize = 10_000;

    private BoardSize? _boardSize;
    private TurtleStart? _turtleStart;
    private Coordinate? _exit;
    private List<Coordinate> _mines = new List<Coordinate>();

    public Settings Parse(string[] settingsFileLines)
    {
        if (settingsFileLines.Length < 3)
        {
            throw new ArgumentException("Not enough items in the file. File must define board size, starting position and exit position.");
        }

        ReadBoardSize(settingsFileLines[0]);
        ReadTurtleStartData(settingsFileLines[1]);
        ReadExitCoordenates(settingsFileLines[2]);

        if (settingsFileLines.Length >= 3)
            ReadMines(settingsFileLines[3..]);

        return new Settings(_boardSize!, _turtleStart!, _exit!, _mines);
    }

    private void ReadMines(string[] mineLines)
    {
        foreach (string mineLine in mineLines)
        {
            var coords = mineLine.Split(",", StringSplitOptions.TrimEntries);

            if (coords.Length != 2)
                throw new ArgumentException("Invalid mine definition in settings file.");

            int column = ParseCoordinateColumnValue(coords, "mine");
            int row = ParseCoordinateRowValue(coords, "mine");

            _mines.Add(new Coordinate(column, row));
        }
    }

    private void ReadExitCoordenates(string line)
    {
        var coords = line.Split(",", StringSplitOptions.TrimEntries);

        if (coords.Length != 2)
            throw new ArgumentException("Invalid board size definition in settings file.");

        int column = ParseCoordinateColumnValue(coords, "exit");
        int row = ParseCoordinateRowValue(coords, "exit");

        _exit = new Coordinate(column, row);
    }

    private void ReadTurtleStartData(string line)
    {
        var coords = line.Split(",", StringSplitOptions.TrimEntries);

        if (coords.Length != 3)
            throw new ArgumentException("Invalid turtle start position definition in settings file.");

        int column = ParseCoordinateColumnValue(coords, "turtle start");
        int row = ParseCoordinateRowValue(coords, "turtle start");
        char direction = ParseDirectionValue(coords);

        _turtleStart = new TurtleStart(new Coordinate(column, row), direction.MapToDirection());
    }

    private static char ParseDirectionValue(string[] coords)
    {
        if (!char.TryParse(coords[2], out var direction))
            throw new ArgumentException("Invalid turtle start direction value in settings file.");
        if (!ValidDirections.Contains(direction))
            throw new ArgumentOutOfRangeException("Invalid turtle start direction value in settings file - direction {direction} is an invalid character.");

        return direction;
    }

    private void ReadBoardSize(string line)
    {
        var coords = line.Split(",", StringSplitOptions.TrimEntries);

        if (coords.Length != 2)
            throw new ArgumentException("Invalid board size definition in settings file.");

        if (!int.TryParse(coords[0], out var columns))
            throw new ArgumentOutOfRangeException("Invalid board size columns value in settings file.");
        if (columns <= 0 || columns > MaxBoardSize)
            throw new ArgumentOutOfRangeException($"Invalid columns value in settings file - columns value must be in the ]0..{MaxBoardSize}] range.");

        if (!int.TryParse(coords[1], out var rows))
            throw new ArgumentOutOfRangeException("Invalid board size rows value in settings file.");
        if (rows <= 0 || rows > MaxBoardSize)
            throw new ArgumentOutOfRangeException($"Invalid rows value in settings file - rows value must be in the ]0..{MaxBoardSize}] range.");


        _boardSize = new BoardSize(columns, rows);
    }

    private int ParseCoordinateColumnValue(string[] coords, string coordType)
    {
        if (!int.TryParse(coords[0], out var column))
            throw new ArgumentOutOfRangeException($"Invalid {coordType} coordinate column value in settings file.");
        if (column < 0 || column >= _boardSize!.Columns)
            throw new ArgumentOutOfRangeException($"Invalid {coordType} coordinate column value in settings file - column value {column} must be in the [0..{_boardSize.Columns}] range.");

        return column;
    }

    private int ParseCoordinateRowValue(string[] coords, string coordType)
    {
        if (!int.TryParse(coords[1], out var row))
            throw new ArgumentOutOfRangeException($"Invalid {coordType} coordinate row value in settings file.");
        if (row < 0 || row >= _boardSize!.Rows)
            throw new ArgumentOutOfRangeException($"Invalid {coordType} coordinate row value in settings file - column value {row} must be in the [0..{_boardSize.Rows}] range.");

        return row;
    }
}

class Sequences : IEnumerable
{
    readonly Moves[][] _sequences;

    public Sequences(IEnumerable<IEnumerable<Moves>> moveSet)
    {
        _sequences = moveSet.Select(m => m.ToArray()).ToArray();
    }

    public int Count => _sequences.Length;

    IEnumerator IEnumerable.GetEnumerator() => _sequences.GetEnumerator();
}

public enum Moves
{
    Move = 0,
    Rotate = 1
}

class Settings
{
    public BoardSize BoardSize { get; private set; }
    public TurtleStart TurtleStart { get; private set; }
    public Coordinate Exit { get; private set; }
    public HashSet<Coordinate> Mines { get; private set; } = new HashSet<Coordinate>();

    public Settings(BoardSize boardSize, TurtleStart turtleStart, Coordinate exit, List<Coordinate> mines)
    {
        BoardSize = boardSize;
        TurtleStart = turtleStart;
        Exit = exit;

        foreach (var coord in mines)
        {
            if (Mines.Contains(coord))
                throw new ArgumentException($"Duplicate mine setting found: {coord}");

            Mines.Add(coord);
        }
    }
}

record BoardSize(int Columns, int Rows);
record Coordinate(int Column, int Row);
record TurtleStart(Coordinate Start, Direction Direction);

enum Direction
{
    North = 0,
    East = 1,
    South = 2,
    West = 3,
}

internal static class DirectionExtensions
{
    public static Direction Rotate(this Direction currentDirection)
        => (Direction)(((int)currentDirection + 1) % 4);

    public static Direction MapToDirection(this char directionCharacter)
        => directionCharacter switch
        {
            'N' => Direction.North,
            'E' => Direction.East,
            'S' => Direction.South,
            'W' => Direction.West,
            _ => throw new ArgumentOutOfRangeException()
        };
}