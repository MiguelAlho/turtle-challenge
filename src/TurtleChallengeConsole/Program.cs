namespace TurtleChallenge;

public class Program {

    public static void Main()
    {
        // See https://aka.ms/new-console-template for more information
        Console.WriteLine("Hello, World!");
    }
}

class Turtle
{

}

class Mine
{

}

class Exit
{

}

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

}