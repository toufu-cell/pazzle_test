using System;
using System.Collections.Generic;
using System.Linq;

namespace FallingPuzzle.Core
{
    public sealed class SevenBag
    {
        private readonly Random _random;
        private Queue<TetrominoType> _queue = new Queue<TetrominoType>();

        public SevenBag(int? seed = null)
        {
            _random = seed.HasValue ? new Random(seed.Value) : new Random();
            Refill();
        }

        private void Refill()
        {
            var values = Enum.GetValues(typeof(TetrominoType)).Cast<TetrominoType>().ToList();
            // Fisher–Yates
            for (int i = values.Count - 1; i > 0; i--)
            {
                int j = _random.Next(i + 1);
                (values[i], values[j]) = (values[j], values[i]);
            }
            foreach (var v in values)
            {
                _queue.Enqueue(v);
            }
        }

        public TetrominoType Next()
        {
            if (_queue.Count == 0)
            {
                Refill();
            }
            var t = _queue.Dequeue();
            if (_queue.Count == 0)
            {
                Refill();
            }
            return t;
        }

        public IEnumerable<TetrominoType> PeekNext(int count)
        {
            while (_queue.Count < count)
            {
                Refill();
            }
            return _queue.Take(count).ToArray();
        }
    }
}