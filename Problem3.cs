﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace d9.aoc._23;
public static class Problem3
{
    [SolutionToProblem(3)]
    public static IEnumerable<object> Solve(string[] inputLines)
    {
        int height = inputLines.Length, width = inputLines.Select(x => x.Length).Max();
        char[,] grid = new char[width, height];
        for(int y = 0; y < height; y++)
        {
            for(int x = 0; x < width; x++)
            {
                grid[x, y] = inputLines[y][x];
            }
        }
        string debugOutput = "";
        for(int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                char c = grid[x, y];
                if((x, y).IsAdjacentToSymbolIn(grid))
                {
                    debugOutput += c;
                } else
                {
                    debugOutput += c switch
                    {
                        '.' => ' ',
                        >= '0' and <= '9' => c,
                        _ => 'S'
                    };
                }
            }
            debugOutput += '\n';
        }
        File.WriteAllText("3_debugOutput.txt", debugOutput);
        yield return grid.PartNumbers().Sum();
    }
    public static IEnumerable<int> PartNumbers(this char[,] plan)
    {
        string currentNumber = "";
        bool isPartNumber = false;
        foreach((int x, int y) in plan.AllPoints())
        {
            char cur = plan[x, y];
            Console.WriteLine($"({x,3}, {y,3}): {cur} {(isPartNumber ? 'T' : ' ')} {currentNumber}");
            if (!cur.IsDigit())
            {
                if (currentNumber.Length > 0 && isPartNumber)
                    yield return int.Parse(currentNumber);
                (currentNumber, isPartNumber) = ("", false);
                continue;
            }
            currentNumber += cur;
            if ((x, y).IsAdjacentToSymbolIn(plan))
                isPartNumber = true;
        } 
    }
    public static bool IsSymbol(this char c) 
        => !c.IsDigit() && c != '.';
    public static bool IsDigit(this char c)
        => c is >= '0' and <= '9';
    public static bool IsAdjacentToSymbolIn(this Point p, char[,] array)
        => array.ValuesAdjacentTo(p, includeSelf: true).Any(IsSymbol);
    public static bool IsAdjacentToSymbolIn(this (int x, int y) p, char[,] array)
        => new Point(p).IsAdjacentToSymbolIn(array);
}
public readonly struct Point(int x, int y)
{
    public readonly int X = x;
    public readonly int Y = y;
    public Point((int x, int y) tuple) : this(tuple.x, tuple.y) { }
    public static implicit operator Point((int x, int y) tuple)
        => new(tuple.x, tuple.y);
    public static implicit operator (int x, int y)(Point p)
        => (p.X, p.Y);
    public static Point operator +(Point a, Point b)
        => new(a.X + b.X, a.Y + b.Y);
    public void Deconstruct(out int x, out int y)
    {
        x = X;
        y = Y;
    }
}
public static class ArrayUtils
{
    public static bool IsInBoundsOf<T>(this Point point, T[,] array)
    {
        (int x, int y) = point;
        return x >= 0 && x < array.GetLength(0) && y >= 0 && y < array.GetLength(1);
    }
    public static IEnumerable<Point> PointsAdjacentTo<T>(this T[,] array, Point point, bool includeSelf = false)
    {
        for(int xo = -1; xo <= 1; xo++)
        {
            for(int yo = -1; yo <= 1; yo++)
            {
                if (!includeSelf && xo is 0 && yo is 0)
                    continue;
                Point cur = point + (xo, yo);
                if (cur.IsInBoundsOf(array))
                    yield return cur;
            }
        }
    }
    public static IEnumerable<T> ValuesAdjacentTo<T>(this T[,] array, Point point, bool includeSelf = false)
        => array.PointsAdjacentTo(point, includeSelf).Select(p => array[p.X, p.Y]);
    public static IEnumerable<Point> AllPoints<T>(this T[,] array)
    {
        for (int y = 0; y < array.GetLength(1); y++)
            for (int x = 0; x < array.GetLength(0); x++)
                yield return (x, y);
    }
}