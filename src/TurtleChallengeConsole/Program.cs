using System.Collections;

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

    private UnboundedCoordinate _turtlePosition;
    private Direction _direction;

    public Simulator(Settings settings, Moves[] moves)
    {
        _settings = settings;
        _moves = moves;

        _turtlePosition = new UnboundedCoordinate(settings.TurtleStart.Start.Column, settings.TurtleStart.Start.Row);
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
        => _settings.Mines.Contains(new BoundedCoordinate((ushort)_turtlePosition.Column, (ushort)_turtlePosition.Row));

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

    private UnboundedCoordinate GetNewTurtlePosition(Direction direction) => direction switch
    {
        Direction.North => new UnboundedCoordinate(_turtlePosition.Column, _turtlePosition.Row - 1),
        Direction.East => new UnboundedCoordinate(_turtlePosition.Column + 1, _turtlePosition.Row),
        Direction.South => new UnboundedCoordinate(_turtlePosition.Column, _turtlePosition.Row + 1),
        Direction.West => new UnboundedCoordinate(_turtlePosition.Column - 1, _turtlePosition.Row),
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
    public BoundedCoordinate Exit { get; private set; }
    public HashSet<BoundedCoordinate> Mines { get; private set; }

    public Settings(BoardSize boardSize, TurtleStart turtleStart, BoundedCoordinate exit, HashSet<BoundedCoordinate> mines)
    {
        BoardSize = boardSize;
        TurtleStart = turtleStart;
        Exit = exit;
        Mines = mines;
    }
}

record BoardSize(ushort Columns, ushort Rows);
record BoundedCoordinate(ushort Column, ushort Row);

/// <summary>
/// Unbounded Coordinate has a column / row pair that 
/// can extend outside normal board limits to detect out of bounds movement
/// </summary>
/// <param name="Column"></param>
/// <param name="Row"></param>
record UnboundedCoordinate(int Column, int Row);
record TurtleStart(BoundedCoordinate Start, Direction Direction);

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