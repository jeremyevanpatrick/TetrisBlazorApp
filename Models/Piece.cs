using System.Collections.Generic;

namespace TetrisApp.Models
{
    public class Piece
    {
        public bool IsActive { get; set; }
        public Color Type { get; set; }
        public int[][] Matrix { get; set; }
        public Position BoardPosition { get; set; }

        public Piece(Color type)
        {
            Type = type;
            Matrix = TypePositions[type];
            BoardPosition = new Position(4, 0);
        }

        public Piece Clone()
        {
            return new Piece(Type)
            {
                Matrix = CloneMatrix(),
                BoardPosition = BoardPosition.Clone()
            };
        }

        public int[][] CloneMatrix()
        {
            int[][] clonedMatrix = new int[Matrix.Length][];
            for (var y=0; y < Matrix.Length;y++)
            {
                clonedMatrix[y] = new int[Matrix[y].Length];
                for (var x = 0; x < Matrix[y].Length; x++)
                {
                    clonedMatrix[y][x] = Matrix[y][x];
                }
            }
            return clonedMatrix;
        }

        private static Dictionary<Color, int[][]> TypePositions = new Dictionary<Color, int[][]>()
        {
            {
                Color.empty,
                new int[][]
                {
                    new int[] {}
                }
            },
            {
                Color.turquoise,
                new int[][]
                {
                    new int[] { 0, 0, 0, 0 },
                    new int[] { 1, 1, 1, 1 },
                    new int[] { 0, 0, 0, 0 },
                    new int[] { 0, 0, 0, 0 }
                }
            },
            {
                Color.yellow,
                new int[][]
                {
                    new int[] { 1, 1 },
                    new int[] { 1, 1 }
                }
            },
            {
                Color.purple,
                new int[][]
                {
                    new int[] { 0, 1, 0 },
                    new int[] { 1, 1, 1 },
                    new int[] { 0, 0, 0 }
                }
            },
            {
                Color.blue,
                new int[][]
                {
                    new int[] { 1, 0, 0 },
                    new int[] { 1, 1, 1 },
                    new int[] { 0, 0, 0 }
                }
            },
            {
                Color.orange,
                new int[][]
                {
                    new int[] { 0, 0, 1 },
                    new int[] { 1, 1, 1 },
                    new int[] { 0, 0, 0 }
                }
            },
            {
                Color.red,
                new int[][]
                {
                    new int[] { 1, 1, 0 },
                    new int[] { 0, 1, 1 },
                    new int[] { 0, 0, 0 }
                }
            },
            {
                Color.green,
                new int[][]
                {
                    new int[] { 0, 1, 1 },
                    new int[] { 1, 1, 0 },
                    new int[] { 0, 0, 0 }
                }
            }
        };

    }
}
