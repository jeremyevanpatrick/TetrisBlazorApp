
namespace TetrisApp.Models
{
    public class BoardPositions
    {
        public Position[][] Positions { get; set; }

        public Position[][] ClonePositions()
        {
            Position[][] clonedPositions = new Position[Positions.Length][];

            for (var y = 0; y < Positions.Length; y++)
            {
                clonedPositions[y] = new Position[Positions[y].Length];

                for (var x = 0; x < Positions[y].Length; x++)
                {
                    clonedPositions[y][x] = Positions[y][x].Clone();
                }
            }

            return clonedPositions;
        }

    }

    public class Position
    {
        public Position(int x, int y, Color type = Color.empty)
        {
            XPos = x;
            YPos = y;
            Type = type;
        }

        public Position Clone()
        {
            return new Position(XPos, YPos, Type);
        }

        public Color Type { get; set; }
        public int XPos { get; set; }
        public int YPos { get; set; }

    }
}
