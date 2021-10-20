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