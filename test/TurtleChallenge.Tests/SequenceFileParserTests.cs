using FluentAssertions;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace TurtleChallenge.Tests;

public class SequenceFileParserTests
{
    [Fact]
    public void ParseThrowsOnEmptyFile()
    {
        var action = () => new SequenceFileParser().Parse(new string[] { });
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ValidFileWithSingleSequenceParsesCorrectly()
    {
        var builder = new SequenceFileBuilder().ClearSequences().AddRandomSequence();
        var sequences = new SequenceFileParser().Parse(builder.Build());

        sequences.Count.Should().Be(1);
    }

    [Fact]
    public void ValidFileWithMultipleSequenceParsesCorrectly()
    {
        var builder = new SequenceFileBuilder();
        var sequences = new SequenceFileParser().Parse(builder.Build());

        sequences.Count.Should().Be(SequenceFileBuilder.DefaultSequenceCount);
    }

    [Fact]
    public void ValidFileWithEmptySequenceSymbolsThrows()
    {
        var builder = new SequenceFileBuilder().ClearSequences().AddSpecificSequence("");
        var act = () => new SequenceFileParser().Parse(builder.Build());

        act.Should().Throw<InvalidDataException>();
    }
    
    [Fact]
    public void ValidFileWithInvalidSequenceSymbolsThrows()
    {
        var builder = new SequenceFileBuilder().ClearSequences().AddSpecificSequence("mar");
        var act = () => new SequenceFileParser().Parse(builder.Build());

        act.Should().Throw<ArgumentException>();
    }
}

public class SequenceFileBuilder
{
    public const int DefaultSequenceCount = 3;
    public List<string> _sequences = new List<string>();

    public SequenceFileBuilder()
    {
        for (int i = 0; i < DefaultSequenceCount; i++)
            _sequences.Add(GenerateRandomSequence());
    }

    public string[] Build()
    {
        return _sequences.ToArray();
    }

    public SequenceFileBuilder ClearSequences()
    {
        _sequences.Clear();
        return this;
    }

    public SequenceFileBuilder AddRandomSequence()
    {
        _sequences.Add(GenerateRandomSequence());
        return this;
    }

    public SequenceFileBuilder AddSpecificSequence(Moves[] moves)
    {
        _sequences.Add(moves.ToStringSequence());
        return this;
    }

    public SequenceFileBuilder AddSpecificSequence(string movesString)
    {
        _sequences.Add(movesString);
        return this;
    }
    string GenerateRandomSequence(ushort size = 5)
    {
        char[] sequence = new char[size];
        for (int i = 0; i < size; i++)
            sequence[i] = CreateValidMoveLetter();
        return new string(sequence);
    }

    char CreateValidMoveLetter()
       => SequenceFileParser.ValidActions[new Random().Next(SequenceFileParser.ValidActions.Length)];


}