using System;
using System.Collections.Generic;
using System.Linq;

namespace FallingPuzzle.Core
{
    public enum Cell
    {
        Empty = -1,
        I = 0,
        O = 1,
        T = 2,
        S = 3,
        Z = 4,
        J = 5,
        L = 6
    }

    public sealed class FallingPiece
    {
        public TetrominoType Type { get; private set; }
        public Orientation Orientation { get; private set; }
        public Int2 Position { get; private set; } // position of rotation origin in grid coords

        public FallingPiece(TetrominoType type, Int2 position, Orientation orientation = Orientation.Spawn)
        {
            Type = type;
            Position = position;
            Orientation = orientation;
        }

        public IEnumerable<Int2> GetBlockCells()
        {
            foreach (var b in TetrominoShapes.GetBlocks(Type, Orientation))
            {
                yield return Position + b;
            }
        }

        public FallingPiece With(Int2? position = null, Orientation? orientation = null)
        {
            return new FallingPiece(Type, position ?? Position, orientation ?? Orientation);
        }

        public FallingPiece Rotate(RotationDirection dir)
        {
            return new FallingPiece(Type, Position, TetrominoShapes.Rotate(Orientation, dir));
        }

        public FallingPiece Move(Int2 delta)
        {
            return new FallingPiece(Type, Position + delta, Orientation);
        }
    }

    public sealed class Board
    {
        public int Width { get; }
        public int Height { get; }
        public int HiddenTopRows { get; }

        private readonly Cell[,] _cells;

        public FallingPiece Current { get; private set; }
        public TetrominoType? Hold { get; private set; }
        public bool HoldUsedThisTurn { get; private set; }
        public SevenBag Bag { get; }

        public int Score { get; private set; }
        public int Level { get; private set; } = 1;
        public int LinesClearedTotal { get; private set; }
        public int ComboCount { get; private set; } = -1; // -1 means no previous clear
        public bool BackToBack { get; private set; } // for future Tetris/T-Spin; we apply for 4-line clears

        public bool IsGameOver { get; private set; }

        public Board(int width = 10, int height = 20, int hiddenTopRows = 2, int? seed = null)
        {
            Width = width;
            Height = height;
            HiddenTopRows = hiddenTopRows;
            _cells = new Cell[width, height + hiddenTopRows];
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height + hiddenTopRows; y++)
                _cells[x, y] = Cell.Empty;

            Bag = new SevenBag(seed);
            SpawnNewPiece();
        }

        public Cell GetCell(int x, int y) => _cells[x, y];

        private Int2 GetSpawnPosition(TetrominoType type)
        {
            // Spawn near top center; SRS typical origins differ per piece, but we use a unified origin
            int x = Width / 2;
            int y = Height + HiddenTopRows - 2; // leave room above
            return new Int2(x, y);
        }

        private void SpawnNewPiece(TetrominoType? forced = null)
        {
            var type = forced ?? Bag.Next();
            var pos = GetSpawnPosition(type);
            var piece = new FallingPiece(type, pos, Orientation.Spawn);
            if (!CanPlace(piece))
            {
                IsGameOver = true;
                return;
            }
            Current = piece;
            HoldUsedThisTurn = false;
        }

        private bool CanPlace(FallingPiece piece)
        {
            foreach (var c in piece.GetBlockCells())
            {
                if (c.X < 0 || c.X >= Width || c.Y < 0 || c.Y >= Height + HiddenTopRows)
                    return false;
                if (_cells[c.X, c.Y] != Cell.Empty)
                    return false;
            }
            return true;
        }

        public bool MoveLeft() => TryMove(new Int2(-1, 0));
        public bool MoveRight() => TryMove(new Int2(1, 0));
        public bool SoftDrop()
        {
            bool moved = TryMove(new Int2(0, -1));
            if (moved)
            {
                // Soft drop scoring: +1 per cell
                Score += 1;
            }
            return moved;
        }

        public int HardDrop()
        {
            int dropped = 0;
            while (TryMove(new Int2(0, -1)))
            {
                dropped++;
            }
            // Hard drop scoring: +2 per cell
            Score += dropped * 2;
            LockPiece();
            return dropped;
        }

        private bool TryMove(Int2 delta)
        {
            if (IsGameOver) return false;
            var moved = Current.Move(delta);
            if (CanPlace(moved))
            {
                Current = moved;
                return true;
            }
            return false;
        }

        public bool Rotate(RotationDirection direction)
        {
            if (IsGameOver) return false;
            var targetOrientation = TetrominoShapes.Rotate(Current.Orientation, direction);
            var kicks = SrsKickTables.GetKicks(Current.Type, Current.Orientation, targetOrientation);
            foreach (var k in kicks)
            {
                var candidate = new FallingPiece(Current.Type, Current.Position + k, targetOrientation);
                if (CanPlace(candidate))
                {
                    Current = candidate;
                    return true;
                }
            }
            return false;
        }

        public void HoldPiece()
        {
            if (IsGameOver) return;
            if (HoldUsedThisTurn) return;

            var currentType = Current.Type;
            if (Hold.HasValue)
            {
                var swapType = Hold.Value;
                Hold = currentType;
                SpawnNewPiece(swapType);
            }
            else
            {
                Hold = currentType;
                SpawnNewPiece();
            }
            HoldUsedThisTurn = true;
        }

        public void TickGravityOnce()
        {
            if (IsGameOver) return;
            if (!TryMove(new Int2(0, -1)))
            {
                // landed, lock immediately (simplified; no lock delay in core)
                LockPiece();
            }
        }

        public void SetCurrentUnsafe(FallingPiece piece)
        {
            if (!CanPlace(piece))
            {
                throw new InvalidOperationException($"Cannot place piece {piece.Type} at {piece.Position} with {piece.Orientation}");
            }
            Current = piece;
        }

        private void LockPiece()
        {
            foreach (var c in Current.GetBlockCells())
            {
                _cells[c.X, c.Y] = (Cell)((int)Current.Type);
            }
            ResolveLinesAndScore();
            SpawnNewPiece();
        }

        private void ResolveLinesAndScore()
        {
            var fullLines = new List<int>();
            for (int y = 0; y < Height + HiddenTopRows; y++)
            {
                bool full = true;
                for (int x = 0; x < Width; x++)
                {
                    if (_cells[x, y] == Cell.Empty)
                    {
                        full = false;
                        break;
                    }
                }
                if (full)
                {
                    fullLines.Add(y);
                }
            }

            if (fullLines.Count > 0)
            {
                // Clear lines
                foreach (var y in fullLines)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        _cells[x, y] = Cell.Empty;
                    }
                }
                // Collapse
                int writeY = 0;
                for (int readY = 0; readY < Height + HiddenTopRows; readY++)
                {
                    bool emptyRow = true;
                    for (int x = 0; x < Width; x++)
                    {
                        if (_cells[x, readY] != Cell.Empty)
                        {
                            emptyRow = false;
                            break;
                        }
                    }
                    if (!emptyRow)
                    {
                        if (writeY != readY)
                        {
                            for (int x = 0; x < Width; x++)
                            {
                                _cells[x, writeY] = _cells[x, readY];
                                _cells[x, readY] = Cell.Empty;
                            }
                        }
                        writeY++;
                    }
                }

                // Score
                int lines = fullLines.Count;
                LinesClearedTotal += lines;
                ComboCount = ComboCount < 0 ? 0 : ComboCount + 1;

                int baseScore = lines switch
                {
                    1 => 100,
                    2 => 300,
                    3 => 500,
                    4 => 800,
                    _ => 0
                };

                bool isBtbCandidate = lines == 4; // no T-Spin in MVP
                if (isBtbCandidate)
                {
                    if (BackToBack)
                    {
                        baseScore = (int)Math.Round(baseScore * 1.5);
                    }
                    BackToBack = true;
                }
                else
                {
                    BackToBack = false;
                }

                int comboBonus = ComboCount > 0 ? 50 * ComboCount : 0;
                Score += baseScore + comboBonus;

                // Level up every 10 lines
                int newLevel = 1 + (LinesClearedTotal / 10);
                if (newLevel > Level)
                {
                    Level = newLevel;
                }
            }
            else
            {
                ComboCount = -1;
            }
        }

        public int GetGhostDropDistance()
        {
            int distance = 0;
            var piece = Current;
            while (true)
            {
                var moved = piece.Move(new Int2(0, -1));
                if (CanPlace(moved))
                {
                    distance++;
                    piece = moved;
                }
                else
                {
                    break;
                }
            }
            return distance;
        }
    }
}