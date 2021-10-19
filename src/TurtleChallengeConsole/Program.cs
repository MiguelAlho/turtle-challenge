namespace TurtleChallenge;

public class Program {

    public static int Main(string[] args)
    {
        if(args.Length != 2)
        {
            Console.WriteLine("Invalid arguments for the application");
            Console.WriteLine("Usage: .\\TurtleChallengeConsole.exe <settings file path> <sequence file path>");
            return -1;
        }

        var settingsFile = args[0];
        var sequenceFile = args[1];

        //read settings file and setup simulator
        try
        {
            string[] settingsFileLines = File.ReadAllLines(settingsFile);
        }
        catch(Exception ex)
        {
            Console.WriteLine("Invalid settings file data");
            Console.WriteLine($"{ex.Message}");
            return -2;
        }

        //read sequences and process each one

        return 0;
    }
}

class SettingsParser
{
    public static char[] ValidDirections = new char[] { 'N', 'E', 'S', 'W' };

    private BoardSize? _boardSize;
    private TurtleStart? _turtleStart;
    private Coordinate? _exit;
    private List<Coordinate> _mines = new List<Coordinate>();

    public Settings Parse(string[] settingsFileLines)
    {
        if(settingsFileLines.Length < 3)
        {
            throw new ArgumentException("Not enough items in the file. File must define board size, starting position and exit position.");
        }

        readBoardSize(settingsFileLines[0]);
        readTurtleStartData(settingsFileLines[1]);
        readExitCoordenates(settingsFileLines[2]);

        if(settingsFileLines.Length >= 3)
        readMines(settingsFileLines[3..]);

        return new Settings(_boardSize!, _turtleStart!, _exit!, _mines);
    }

    private void readMines(string[] mineLines)
    {
        foreach(string mineLine in mineLines)
        {
            var coords = mineLine.Split(",", StringSplitOptions.TrimEntries);

            if (coords.Length != 2)
                throw new ArgumentException("Invalid mine definition in settings file.");

            ushort column = ParseCoordinateColumnValue(coords, "mine");
            ushort row = ParseCoordinateRowValue(coords, "mine");

            _mines.Add(new Coordinate(column, row));
        }
    }   

    private void readExitCoordenates(string line)
    {
        var coords = line.Split(",", StringSplitOptions.TrimEntries);

        if (coords.Length != 2)
            throw new ArgumentException("Invalid board size definition in settings file.");

        ushort column = ParseCoordinateColumnValue(coords, "exit");
        ushort row = ParseCoordinateRowValue(coords, "exit");

        _exit = new Coordinate(column, row);
    }

    private void readTurtleStartData(string line)
    {
        var coords = line.Split(",", StringSplitOptions.TrimEntries);

        if (coords.Length != 3)
            throw new ArgumentException("Invalid turtle start position definition in settings file.");

        ushort column = ParseCoordinateColumnValue(coords, "turtle start");
        ushort row = ParseCoordinateRowValue(coords, "turtle start");
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

    private void readBoardSize(string line)
    {
        var coords = line.Split(",", StringSplitOptions.TrimEntries);
        
        if(coords.Length != 2)
            throw new ArgumentException("Invalid board size definition in settings file.");
        
        if (!ushort.TryParse(coords[0], out var columns))
            throw new ArgumentOutOfRangeException("Invalid board size columns value in settings file.");
        if (columns == 0)
            throw new ArgumentOutOfRangeException($"Invalid columns value in settings file - columns value must be greater than 0.");

        if (!ushort.TryParse(coords[1], out var rows))
            throw new ArgumentOutOfRangeException("Invalid board size rows value in settings file.");
        if (rows == 0)
            throw new ArgumentOutOfRangeException($"Invalid rows value in settings file - rows valuemust be greater than 0.");


        _boardSize = new BoardSize(columns, rows);
    }

    private ushort ParseCoordinateColumnValue(string[] coords, string coordType)
    {
        if (!ushort.TryParse(coords[0], out var column))
            throw new ArgumentOutOfRangeException($"Invalid {coordType} coordinate column value in settings file.");
        if (column >= _boardSize!.Rows)
            throw new ArgumentOutOfRangeException($"Invalid {coordType} coordinate column value in settings file - column value {column} must be less then board size {_boardSize.Columns}.");

        return column;
    }

    private ushort ParseCoordinateRowValue(string[] coords, string coordType)
    {
        if (!ushort.TryParse(coords[1], out var row))
            throw new ArgumentOutOfRangeException($"Invalid {coordType} coordinate row value in settings file.");
        if (row >= _boardSize!.Rows)
            throw new ArgumentOutOfRangeException($"Invalid {coordType} coordinate row value in settings file - column value {row} must be less then board size {_boardSize.Rows}.");

        return row;
    }
}

class Settings
{
    public BoardSize BoardSize { get; private set; }
    public TurtleStart TurtleStart { get; private set; }
    public Coordinate Exit { get; private set; }
    public Dictionary<Coordinate, Mine> Mines { get; private set; } = new Dictionary<Coordinate, Mine>();

    public Settings(BoardSize boardSize, TurtleStart turtleStart, Coordinate exit, List<Coordinate> mines)
    {
        BoardSize = boardSize;
        TurtleStart = turtleStart;
        Exit = exit;

        foreach (var coord in mines)
            Mines.Add(coord, new());
    }
}

record BoardSize(ushort Columns, ushort Rows);
record Coordinate(ushort Column, ushort Row);
record TurtleStart(Coordinate Start, Direction Direction);

class Turtle{}

class Mine{}

class Exit{}

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