using System;
using System.Collections.Generic;

namespace FallingPuzzle.Core
{
    public enum TetrominoType
    {
        I,
        O,
        T,
        S,
        Z,
        J,
        L
    }

    public enum RotationDirection
    {
        Clockwise,
        CounterClockwise
    }

    public enum Orientation
    {
        Spawn = 0,
        Right = 1,
        Reverse = 2,
        Left = 3
    }

    /// <summary>
    /// Minimal 2D integer vector to avoid Unity dependency.
    /// </summary>
    public readonly struct Int2 : IEquatable<Int2>
    {
        public readonly int X;
        public readonly int Y;

        public Int2(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static Int2 operator +(Int2 a, Int2 b) => new Int2(a.X + b.X, a.Y + b.Y);
        public static Int2 operator -(Int2 a, Int2 b) => new Int2(a.X - b.X, a.Y - b.Y);

        public bool Equals(Int2 other) => X == other.X && Y == other.Y;
        public override bool Equals(object obj) => obj is Int2 other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(X, Y);
        public override string ToString() => $"({X},{Y})";
    }

    /// <summary>
    /// Returns block offsets for each tetromino type per orientation, using SRS conventions.
    /// Coordinates are in cells; origin is the piece position (grid cell) referenced by SRS definition.
    /// </summary>
    public static class TetrominoShapes
    {
        // Orientation order: Spawn (Up), Right, Reverse (Down), Left
        private static readonly Dictionary<TetrominoType, Int2[][]> Shapes = new()
        {
            // Using standard SRS block coordinates relative to the rotation origin.
            // Reference: Tetris Guideline SRS
            [TetrominoType.I] = new[]
            {
                new[] { new Int2(-1,0), new Int2(0,0), new Int2(1,0), new Int2(2,0) },  // Spawn (horizontal)
                new[] { new Int2(1,-1), new Int2(1,0), new Int2(1,1), new Int2(1,2) },   // Right (vertical)
                new[] { new Int2(-1,1), new Int2(0,1), new Int2(1,1), new Int2(2,1) },  // Reverse (horizontal)
                new[] { new Int2(0,-1), new Int2(0,0), new Int2(0,1), new Int2(0,2) },   // Left (vertical)
            },
            [TetrominoType.O] = new[]
            {
                new[] { new Int2(0,0), new Int2(1,0), new Int2(0,1), new Int2(1,1) },   // Spawn
                new[] { new Int2(0,0), new Int2(1,0), new Int2(0,1), new Int2(1,1) },   // Right (same)
                new[] { new Int2(0,0), new Int2(1,0), new Int2(0,1), new Int2(1,1) },   // Reverse
                new[] { new Int2(0,0), new Int2(1,0), new Int2(0,1), new Int2(1,1) },   // Left
            },
            [TetrominoType.T] = new[]
            {
                new[] { new Int2(-1,0), new Int2(0,0), new Int2(1,0), new Int2(0,1) },  // Spawn
                new[] { new Int2(0,-1), new Int2(0,0), new Int2(0,1), new Int2(1,0) },   // Right
                new[] { new Int2(-1,0), new Int2(0,0), new Int2(1,0), new Int2(0,-1) },  // Reverse
                new[] { new Int2(0,-1), new Int2(0,0), new Int2(0,1), new Int2(-1,0) },  // Left
            },
            [TetrominoType.S] = new[]
            {
                new[] { new Int2(0,0), new Int2(1,0), new Int2(-1,1), new Int2(0,1) },  // Spawn
                new[] { new Int2(0,0), new Int2(0,1), new Int2(1,-1), new Int2(1,0) },   // Right
                new[] { new Int2(0,0), new Int2(1,0), new Int2(-1,1), new Int2(0,1) },  // Reverse
                new[] { new Int2(0,0), new Int2(0,1), new Int2(1,-1), new Int2(1,0) },   // Left
            },
            [TetrominoType.Z] = new[]
            {
                new[] { new Int2(-1,0), new Int2(0,0), new Int2(0,1), new Int2(1,1) },  // Spawn
                new[] { new Int2(1,0), new Int2(1,1), new Int2(0,0), new Int2(0,-1) },   // Right
                new[] { new Int2(-1,0), new Int2(0,0), new Int2(0,1), new Int2(1,1) },  // Reverse
                new[] { new Int2(1,0), new Int2(1,1), new Int2(0,0), new Int2(0,-1) },   // Left
            },
            [TetrominoType.J] = new[]
            {
                new[] { new Int2(-1,0), new Int2(0,0), new Int2(1,0), new Int2(-1,1) }, // Spawn
                new[] { new Int2(0,-1), new Int2(0,0), new Int2(0,1), new Int2(1,1) },   // Right
                new[] { new Int2(1,-1), new Int2(-1,0), new Int2(0,0), new Int2(1,0) },  // Reverse
                new[] { new Int2(-1,-1), new Int2(0,-1), new Int2(0,0), new Int2(0,1) }, // Left
            },
            [TetrominoType.L] = new[]
            {
                new[] { new Int2(-1,0), new Int2(0,0), new Int2(1,0), new Int2(1,1) },  // Spawn
                new[] { new Int2(0,-1), new Int2(0,0), new Int2(0,1), new Int2(1,-1) },  // Right
                new[] { new Int2(-1,0), new Int2(0,0), new Int2(1,0), new Int2(-1,-1) }, // Reverse
                new[] { new Int2(0,-1), new Int2(0,0), new Int2(0,1), new Int2(-1,1) },  // Left
            },
        };

        public static IReadOnlyList<Int2> GetBlocks(TetrominoType type, Orientation orientation)
        {
            return Shapes[type][(int)orientation];
        }

        public static Orientation Rotate(Orientation o, RotationDirection dir)
        {
            return dir == RotationDirection.Clockwise
                ? (Orientation)(((int)o + 1) & 3)
                : (Orientation)(((int)o + 3) & 3);
        }
    }
}