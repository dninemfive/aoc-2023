﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace d9.aoc._23;
public static class Problem5
{
    private static Dictionary<string, XToYMap<long>> _mapMap = new();
    [SolutionToProblem(5)]
    public static IEnumerable<object> Solve(string[] lines)
    {
        IEnumerable<long> seeds = lines.First().Split(": ")[1].ToMany<long>();
        _mapMap = ParseLines(lines[2..]);
        foreach(XToYMap<long> map in _mapMap.Values)
        {
            Console.WriteLine(map.FullString);
        }
        yield return seeds.Select(LocationFor).Min();
        Range<long>[] pairs = new Range<long>[seeds.Count() / 2];     
        for(int i = 0; i < pairs.Length; i++)
            pairs[i] = new(seeds.ElementAt(i * 2), seeds.ElementAt(i * 2 + 1));
        Console.WriteLine($"Seeds:       {seeds.Order().Select(x => $"{x,10}").ListNotation()}");
        yield return LowestLocationFor(pairs);
    }   
    public static Dictionary<string, XToYMap<long>> ParseLines(string[] lines)
    {
        string title = "";
        List<string> nonTitleLines = new();
        Dictionary<string, XToYMap<long>> result = new();
        foreach(string line in lines.Append(""))
        {
            if (line.Contains("-to-"))
            {
                title = line;
            }
            else if(string.IsNullOrWhiteSpace(line))
            {
                XToYMap<long> map = new(title, nonTitleLines);
                result[map.InputType] = map;
                nonTitleLines.Clear();
            }
            else
            {
                nonTitleLines.Add(line);
            }
        }
        return result;
    }
    public static long LocationFor(long seed)
    {
        Console.Write($"LocationFor({seed})");
        (string type, long val) cur = ("seed", seed);
        while(cur.type != "location")
        {
            cur = _mapMap[cur.type][cur];
            Console.Write($" -> {cur.val,11}");
        }
        Console.WriteLine();
        return cur.val;
    }
    public static long LowestLocationFor(params Range<long>[] seedRanges)
    {
        // for each location range in order of smallest to largest,
        //      for each humidity range which maps to that location range,
        //          for each temperature range which maps to that humidity range,
        //              ...
        //                  for each seed range,
        //                      if the range maps to any of the soil ranges,
        //                          find the first seed which maps to that range
        string[] keys = [ "seed", "soil", "fertilizer", "water", "light", "temperature", "humidity" ];
        keys = keys.Reverse().ToArray();
        IEnumerable<MapRange<long>> overlapping(string destKey, string sourceKey)
            => _mapMap[sourceKey].Ranges.Where(x => _mapMap[destKey].Ranges.Any(y => x.Destination.OverlapsWith(y.Source)));
        // location range 1
        //  humidity range 1
        //   
        foreach(MapRange<long> range in _mapMap["humidity"].Ranges)
        {
            foreach(MapRange<long> range2 in _mapMap["temperature"].Ranges.Where(x => x.Destination.OverlapsWith(range.Source)))
            {

            }
        }
    }
}
public class XToYMap<T>(string title, IEnumerable<string> nonTitleLines)
    where T : struct, INumber<T>
{
    public string InputType => title.Split("-")[0];
    public string ResultType => title.SplitAndTrim("-", " ")[2];
    public string Name => $"{InputType}-to-{ResultType} map";
    private readonly List<MapRange<T>> _ranges = [ .. nonTitleLines.Select(x => new MapRange<T>(x))
                                                                .OrderBy(x => x.Source.Start) ];
    public IEnumerable<MapRange<T>> Ranges => _ranges;
    public (string type, T val) this[(string type, T val) input]
    {
        get
        {
            if (input.type != InputType)
                throw new ArgumentException($"A {Name} can't convert a value of type {input.type}!");
            foreach (MapRange<T> range in _ranges)
            {
                T? result = range[input.val];
                if (result is not null)
                    return (ResultType, result.Value);
            }
            return (ResultType, input.val);
        }
    }
    public IEnumerable<T> BreakPointsFor(Range<T> range)
    {
        IEnumerable<Range<T>> matchingRanges = _ranges.Select(x => x.Source).Where(range.OverlapsWith);
        yield return range.Start;
        yield return range.End;
        foreach (Range<T> range2 in matchingRanges)
        {
            foreach (T t in new List<T>() { range2.Start - T.One, range2.Start, range2.End, range2.End + T.One })
                if (range.Contains(t))
                    yield return t;
        }
    }
    public override string ToString() => Name;
    public string FullString
        => $"{Name} {{\n\t{_ranges.Select(x => $"{x}").Aggregate((x, y) => $"{x}\n\t{y}")}\n}}";
}
public readonly struct Range<T>(T start, T length)
    where T : INumber<T>
{
    public readonly T Start = start;
    public readonly T End = start + length;
    public readonly T Length = length;
    public bool Contains(T t) => t >= Start && t <= End;
    // https://stackoverflow.com/questions/3269434/whats-the-most-efficient-way-to-test-if-two-ranges-overlap
    public bool OverlapsWith(Range<T> other)
        => Start <= other.End && End >= other.Start;
    public IEnumerable<T> AllValues
    {
        get
        {
            for (T t = Start; t <= End; t++)
                yield return t;
        }
    }
    public override string ToString()
        => $"[{Start}, {End}]";
}
public class MapRange<T>
    where T : struct, INumber<T>
{
    public Range<T> Source { get; private set; }
    public Range<T> Destination { get; private set; }
    public T Diff => Destination.Start - Source.Start;
    public MapRange(string line)
    {
        List<T> values = line.ToMany<T>().ToList();
        (T sourceStart, T destStart, T length) = (values[1], values[0], values[2]);
        Source = new(sourceStart, length);
        Destination = new(destStart, length);
    }
    public T? this[T t]
        => Source.Contains(t) ? t + Diff : null;
    public override string ToString() => $"{{{Source} -> {Destination} ({Diff})}}";
}