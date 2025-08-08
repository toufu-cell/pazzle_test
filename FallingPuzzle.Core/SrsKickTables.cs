using System.Collections.Generic;

namespace FallingPuzzle.Core
{
    public static class SrsKickTables
    {
        // JLSTZ: common SRS kicks
        private static readonly Dictionary<(Orientation from, Orientation to), Int2[]> Jlstz = new()
        {
            { (Orientation.Spawn, Orientation.Right),  new [] { new Int2(0,0), new Int2(-1,0), new Int2(-1,1), new Int2(0,-2), new Int2(-1,-2) } },
            { (Orientation.Right, Orientation.Spawn),  new [] { new Int2(0,0), new Int2(1,0),  new Int2(1,-1), new Int2(0,2),  new Int2(1,2) } },

            { (Orientation.Right, Orientation.Reverse),new [] { new Int2(0,0), new Int2(1,0),  new Int2(1,-1), new Int2(0,2),  new Int2(1,2) } },
            { (Orientation.Reverse, Orientation.Right),new [] { new Int2(0,0), new Int2(-1,0), new Int2(-1,1), new Int2(0,-2), new Int2(-1,-2) } },

            { (Orientation.Reverse, Orientation.Left), new [] { new Int2(0,0), new Int2(1,0),  new Int2(1,1),  new Int2(0,-2), new Int2(1,-2) } },
            { (Orientation.Left, Orientation.Reverse), new [] { new Int2(0,0), new Int2(-1,0), new Int2(-1,-1),new Int2(0,2),  new Int2(-1,2) } },

            { (Orientation.Left, Orientation.Spawn),   new [] { new Int2(0,0), new Int2(-1,0), new Int2(-1,-1),new Int2(0,2),  new Int2(-1,2) } },
            { (Orientation.Spawn, Orientation.Left),   new [] { new Int2(0,0), new Int2(1,0),  new Int2(1,1),  new Int2(0,-2), new Int2(1,-2) } },
        };

        // I: special SRS kicks
        private static readonly Dictionary<(Orientation from, Orientation to), Int2[]> I = new()
        {
            { (Orientation.Spawn, Orientation.Right),  new [] { new Int2(0,0), new Int2(-2,0), new Int2(1,0),  new Int2(-2,-1), new Int2(1,2) } },
            { (Orientation.Right, Orientation.Spawn),  new [] { new Int2(0,0), new Int2(2,0),  new Int2(-1,0), new Int2(2,1),  new Int2(-1,-2) } },

            { (Orientation.Right, Orientation.Reverse),new [] { new Int2(0,0), new Int2(-1,0), new Int2(2,0),  new Int2(-1,2),  new Int2(2,-1) } },
            { (Orientation.Reverse, Orientation.Right),new [] { new Int2(0,0), new Int2(1,0),  new Int2(-2,0), new Int2(1,-2),  new Int2(-2,1) } },

            { (Orientation.Reverse, Orientation.Left), new [] { new Int2(0,0), new Int2(2,0),  new Int2(-1,0), new Int2(2,1),  new Int2(-1,-2) } },
            { (Orientation.Left, Orientation.Reverse), new [] { new Int2(0,0), new Int2(-2,0), new Int2(1,0),  new Int2(-2,-1), new Int2(1,2) } },

            { (Orientation.Left, Orientation.Spawn),   new [] { new Int2(0,0), new Int2(1,0),  new Int2(-2,0), new Int2(1,-2),  new Int2(-2,1) } },
            { (Orientation.Spawn, Orientation.Left),   new [] { new Int2(0,0), new Int2(-1,0), new Int2(2,0),  new Int2(-1,2),  new Int2(2,-1) } },
        };

        public static IReadOnlyList<Int2> GetKicks(TetrominoType type, Orientation from, Orientation to)
        {
            if (type == TetrominoType.I)
            {
                return I[(from, to)];
            }
            if (type == TetrominoType.O)
            {
                // O piece has no kicks in SRS; origin shifts are handled by shape equivalence
                return new[] { new Int2(0,0) };
            }
            return Jlstz[(from, to)];
        }
    }
}