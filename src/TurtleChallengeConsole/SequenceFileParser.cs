namespace TurtleChallengeConsole;

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

    internal static Moves MoveFrom(char symbol) => symbol switch
    {
        'm' => Moves.Move,
        'r' => Moves.Rotate,
        _ => throw new ArgumentOutOfRangeException($"Unsupported move symbol '{symbol}' found")
    };
}
