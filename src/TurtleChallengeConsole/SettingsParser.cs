namespace TurtleChallengeConsole;

class SettingsParser
{
    public static char[] ValidDirections = new char[] { 'N', 'E', 'S', 'W' };

    private BoardSize? _boardSize;
    private TurtleStart? _turtleStart;
    private BoundedCoordinate? _exit;
    private HashSet<BoundedCoordinate> _mines = new HashSet<BoundedCoordinate>();

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

            ushort column = ParseCoordinateColumnValue(coords, "mine");
            ushort row = ParseCoordinateRowValue(coords, "mine");
            var coord = new BoundedCoordinate(column, row);

            if (_mines.Contains(coord))
                throw new InvalidOperationException($"Duplicate mine detected - {coord} already exists in list ({string.Join(",", _mines.Select(m => m.ToString()))})");

            _mines.Add(coord);
        }
    }

    private void ReadExitCoordenates(string line)
    {
        var coords = line.Split(",", StringSplitOptions.TrimEntries);

        if (coords.Length != 2)
            throw new ArgumentException("Invalid board size definition in settings file.");

        ushort column = ParseCoordinateColumnValue(coords, "exit");
        ushort row = ParseCoordinateRowValue(coords, "exit");

        _exit = new BoundedCoordinate(column, row);
    }

    private void ReadTurtleStartData(string line)
    {
        var coords = line.Split(",", StringSplitOptions.TrimEntries);

        if (coords.Length != 3)
            throw new ArgumentException("Invalid turtle start position definition in settings file.");

        ushort column = ParseCoordinateColumnValue(coords, "turtle start");
        ushort row = ParseCoordinateRowValue(coords, "turtle start");
        char direction = ParseDirectionValue(coords);

        _turtleStart = new TurtleStart(new BoundedCoordinate(column, row), direction.MapToDirection());
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
        if (column >= _boardSize!.Columns)
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
