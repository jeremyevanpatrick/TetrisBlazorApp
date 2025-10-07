using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TetrisApp.Models
{
    public class MovePath : IEnumerable<Move>
    {
        private List<Move> _items = new List<Move>();

        public void Add(Move item)
        {
            Items.Add(item);
        }

        public void RemoveAt(int index)
        {
            List<Move> temp = new List<Move>();
            for (int x=0; x<Items.Count; x++)
            {
                if (index != x)
                {
                    temp.Add(Items[x]);
                }
            }
            Items = temp;
        }

        public int Count => Items.Count;

        public Move this[int index]
        {
            get => Items[index];
            set => Items[index] = value;
        }

        public List<Move> Items
        {
            get => _items;
            set => _items = value;
        }

        public IEnumerator<Move> GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        public Move GetLastDirection()
        {
            Move[] moves = Items.Where(x => x != Move.rotateclockwise && x != Move.rotatecounterclockwise).ToArray();
            if (moves.Any())
            {
                return moves.Last();
            }
            return Move.none;
        }

        public Move GetFromDirection()
        {
            return FlipDir( GetLastDirection() );
        }

        private Move FlipDir(Move c)
        {
            if (Move.left.Equals(c))
            {
                return Move.right;
            }
            if (Move.right.Equals(c))
            {
                return Move.left;
            }
            if (Move.up.Equals(c))
            {
                return Move.down;
            }
            if (Move.down.Equals(c))
            {
                return Move.up;
            }
            return Move.none;
        }

    }

    public enum Move
    {
        none,
        up,
        left,
        down,
        right,
        rotateclockwise,
        rotatecounterclockwise
    }
}
