﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static aoc_solns_2023.Problem1;

namespace aoc_solns_2023;
public static class Problem2
{
    public static IEnumerable<object> Solve(string inputFile)
    {
        IEnumerable<string> lines = File.ReadAllLines(inputFile);
        IEnumerable<Game> games = lines.Select(x => new Game(x));
        yield return games.Where(x => x.PossibleWith(("red", 12), ("green", 13), ("blue", 14)))
                          .Sum(x => x.Id);
        yield return games.Select(x => x.MinimumRequiredColors)
                          .Sum(x => x.Power());
    }
    public class Game(string line)
    {
        public int Id = int.Parse(line.Split(": ")[0].Split(" ") [1]);
        public List<Handful> Handfuls = line.Split(": ")[1].Split("; ")
                                            .Select(x => new Handful(x))
                                            .ToList();
        public class Handful
        {
            public Dictionary<string, int> Colors = new();
            public Handful(string desc)
            {
                foreach(string s in desc.Split(", "))
                {
                    string[] info = s.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                    Colors[info[1]] = int.Parse(info[0]);
                }
            }
            public int this[string key] => Colors.TryGetValue(key, out int result) ? result : 0;
        }
        public bool PossibleWith(params (string color, int quantity)[] colors)
            => !Handfuls.Any(handful => colors.Any(tuple => handful[tuple.color] > tuple.quantity));
        public IEnumerable<string> UniqueColors => Handfuls.SelectMany(x => x.Colors.Keys)
                                                           .Distinct()
                                                           .Order();
        public int MinimumRequired(string color)
            => Handfuls.Select(x => x[color]).Max();
        public Dictionary<string, int> MinimumRequiredColors
            => new(UniqueColors.Select(x => new KeyValuePair<string, int>(x, MinimumRequired(x))));
    }
    public static int Power(this Dictionary<string, int> cubeSet)
        => cubeSet.Values.Aggregate((x, y) => x * y);
}
