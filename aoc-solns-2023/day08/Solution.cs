﻿namespace d9.aoc._23.day8;
public static class Solution
{
    private static Dictionary<string, (string left, string right)> _nodes = new();
    [SolutionToProblem(8, true)]
    public static IEnumerable<object> Solve(string[] lines)
    {
        string tape = lines.First();
        _nodes = lines.Skip(2)
                      .Select(x => x.SplitAndTrim(" = (", ", ", ")"))
                      .ToDict(keys: x => x[0], values: x => (x[1], x[2]));
        yield return 0b0;
        yield return NavigateBetween("AAA", "ZZZ", tape).Count();
        yield return GhostPathLength(tape);
    }
    public static IEnumerable<(string position, long ct)> NavigateBetween(string start, string end, string tape)
        => start.NavigateUntil(x => x.position == end, tape);
    public static IEnumerable<(string position, long ct)> NavigateUntil(this string start, Func<(string position, long ct), bool> end, string input)
    {
        Tape tape = new(input);
        string cur = start;
        long ct = 0;
        while (!end((cur, ct)))
            yield return (cur = tape.Step(cur), ++ct);
    }
    public static string Step(this Tape tape, string cur)
    {
        (string left, string right) = _nodes[cur];
        char c = tape.Advance();
        return c == 'L' ? left : right;
    }
    public static long GhostPathLength(string tape)
        => _nodes.Keys.Where(x => x.EndsWith('A'))
                      .SelectMany(tape.ZPositions)
                      .LeastCommonMultiple();
    public static IEnumerable<long> ZPositions(this string input, string cur)
    {
        Tape tape = new(input);
        HashSet<(string cur, long index)> visitedStates = new();
        long ct = 0;
        // once we see a (position, ct) pair twice, the sequence will repeat, so we don't need to continue
        // (it turns out for my input there's always exactly one NumberPair<int> where the item ends with Z,
        //  but this code is more general and pretty cool i think)
        while (!visitedStates.Contains((cur, tape.Index)))
        {
            if (cur.EndsWith('Z'))
                yield return ct;
            visitedStates.Add((cur, tape.Index));
            cur = tape.Step(cur);
            ct++;
        }
    }
}