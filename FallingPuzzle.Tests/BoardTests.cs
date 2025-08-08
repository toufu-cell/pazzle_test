using System;
using System.Linq;
using FluentAssertions;
using Xunit;
using FallingPuzzle.Core;

namespace FallingPuzzle.Tests
{
    public class BoardTests
    {
        [Fact]
        public void SevenBag_IsDeterministic_WithSeed()
        {
            var bag1 = new SevenBag(seed: 123);
            var bag2 = new SevenBag(seed: 123);
            var seq1 = Enumerable.Range(0, 28).Select(_ => bag1.Next()).ToArray();
            var seq2 = Enumerable.Range(0, 28).Select(_ => bag2.Next()).ToArray();
            seq1.Should().Equal(seq2);
            seq1.Distinct().Count().Should().Be(7);
        }

        [Fact]
        public void Board_SpawnAndMoveWithinBounds()
        {
            var board = new Board(seed: 42);
            board.IsGameOver.Should().BeFalse();
            board.MoveLeft().Should().BeTrue();
            board.MoveRight().Should().BeTrue();
            board.Rotate(RotationDirection.Clockwise).Should().BeTrue();
        }

        [Fact]
        public void Rotation_UsesSrsKicks_WhenBlockedByWall()
        {
            var board = new Board(width: 10, height: 20, hiddenTopRows: 2, seed: 1);
            // Move piece to left wall
            for (int i = 0; i < 10; i++) board.MoveLeft();
            var before = board.Current.Position;
            var rotated = board.Rotate(RotationDirection.CounterClockwise);
            rotated.Should().BeTrue();
            board.Current.Position.Should().NotBe(before);
        }

        [Fact]
        public void HardDrop_LocksAndReturnsDistance()
        {
            var board = new Board(seed: 7);
            int dist = board.HardDrop();
            dist.Should().BeGreaterThan(0);
            board.Score.Should().Be(0); // hard drop scoring handled externally in this core
        }

        [Fact]
        public void LineClear_ScoresAndLevelsAndCombos()
        {
            var board = new Board(width: 4, height: 4, hiddenTopRows: 0, seed: 999);
            // Manually stack to force a single line clear using I piece hard drop twice
            // First piece
            board.Current = new FallingPiece(TetrominoType.I, new Int2(1, 3), Orientation.Right);
            board.HardDrop();
            // Second piece to fill line
            board.Current = new FallingPiece(TetrominoType.I, new Int2(0, 3), Orientation.Spawn);
            board.HardDrop();

            board.LinesClearedTotal.Should().BeGreaterThan(0);
            board.Score.Should().BeGreaterThan(0);
            board.Level.Should().BeGreaterOrEqualTo(1);
        }

        [Fact]
        public void GhostDistance_IsZeroWhenTouching()
        {
            var board = new Board(width: 10, height: 20, hiddenTopRows: 0, seed: 5);
            // Place current piece at bottom
            while (board.SoftDrop()) { }
            board.GetGhostDropDistance().Should().Be(0);
        }
    }
}