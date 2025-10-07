using System;
using System.Collections.Generic;
using System.Linq;

namespace TetrisApp.Models
{
    public static class Utilities
    {
        public static void Log(string str)
        {
            System.Console.WriteLine(str);
        }

        public static void LogBoardGraph(BoardPositions tempPositions)
        {
            string abc = "";
            for (var y = 0; y < tempPositions.Positions.Length; y++)
            {
                for (var x = 0; x < tempPositions.Positions[y].Length; x++)
                {
                    abc = abc + (Color.empty.Equals(tempPositions.Positions[y][x].Type) ? "o" : "x");
                }
                abc = abc + "\n\r";
            }
            Console.WriteLine(abc);
        }

        public static void LogBoardState(BoardPositions tempPositions)
        {
            string abc = "";
            for (var y = 0; y < tempPositions.Positions.Length; y++)
            {
                for (var x = 0; x < tempPositions.Positions[y].Length; x++)
                {
                    abc = abc + "(" + x + "," + y + "," + (Color.empty.Equals(tempPositions.Positions[y][x].Type) ? " " : "x") + "), ";
                }
                abc = abc + "\n\r";
            }
            Console.WriteLine(abc);
        }

        public static void RotateTestPiece(Piece testPiece, int direction)
        {
            //clockwise
            if (direction == 1)
            {
                //reflect horizontally
                List<List<int>> rotatedMatrix1 = new List<List<int>>();
                for (var y = testPiece.Matrix.Length - 1; y >= 0; y--)
                {
                    List<int> rotatedMatrixPrime = new List<int>();
                    for (var x = 0; x < testPiece.Matrix[y].Length; x++)
                    {
                        rotatedMatrixPrime.Insert(0, testPiece.Matrix[y][x]);
                    }
                    rotatedMatrix1.Insert(0, rotatedMatrixPrime);
                }

                //flip diagonally
                List<List<int>> rotatedMatrix2 = new List<List<int>>();
                for (var y = testPiece.Matrix.Length - 1; y >= 0; y--)
                {
                    List<int> rotatedMatrixPrime = new List<int>();
                    for (var x = 0; x < testPiece.Matrix[y].Length; x++)
                    {
                        rotatedMatrixPrime.Insert(0, testPiece.Matrix[x][y]);
                    }
                    rotatedMatrix2.Insert(0, rotatedMatrixPrime);
                }

                testPiece.Matrix = rotatedMatrix2.Select(a => a.ToArray()).ToArray();

            }
            else if (direction == -1)
            {
                //counter-clockwise

                //flip diagonally
                List<List<int>> rotatedMatrix2 = new List<List<int>>();
                for (var y = testPiece.Matrix.Length - 1; y >= 0; y--)
                {
                    List<int> rotatedMatrixPrime = new List<int>();
                    for (var x = 0; x < testPiece.Matrix[y].Length; x++)
                    {
                        rotatedMatrixPrime.Insert(0, testPiece.Matrix[x][y]);
                    }
                    rotatedMatrix2.Insert(0, rotatedMatrixPrime);
                }

                //reflect horizontally
                List<List<int>> rotatedMatrix1 = new List<List<int>>();
                for (var y = testPiece.Matrix.Length - 1; y >= 0; y--)
                {
                    List<int> rotatedMatrixPrime = new List<int>();
                    for (var x = 0; x < testPiece.Matrix[y].Length; x++)
                    {
                        rotatedMatrixPrime.Insert(0, testPiece.Matrix[y][x]);
                    }
                    rotatedMatrix1.Insert(0, rotatedMatrixPrime);
                }

                testPiece.Matrix = rotatedMatrix1.Select(a => a.ToArray()).ToArray();

            }

        }

        public static Move GetCollision(Position[][] boardPositions, Piece testPiece)
        {
            Move collision = Move.none;

            for (var y = 0; y < testPiece.Matrix.Length; y++)
            {
                for (var x = 0; x < testPiece.Matrix[y].Length; x++)
                {
                    if (testPiece.Matrix[y][x] == 1)
                    {
                        int targetX = testPiece.BoardPosition.XPos + x;
                        int targetY = testPiece.BoardPosition.YPos + y;

                        //if the piece is out of bounds
                        if (targetX < 0)
                        {
                            collision = Move.left;
                        }
                        else if (targetX > 9)
                        {
                            collision = Move.right;
                        }
                        else if (targetY < 0)
                        {
                            collision = Move.up;
                        }
                        else if (targetY > 21)
                        {
                            collision = Move.down;
                        }
                        else if (boardPositions[targetY][targetX].Type != Color.empty)
                        {
                            //or if the piece is in the same position as another piece
                            if (x == 0)
                            {
                                collision = Move.left;
                            }
                            else if (x == testPiece.Matrix[y].Length - 1)
                            {
                                collision = Move.right;
                            }
                            else if (y == 0)
                            {
                                collision = Move.up;
                            }
                            else if (y == testPiece.Matrix.Length - 1)
                            {
                                collision = Move.down;
                            }
                            else
                            {
                                collision = Move.down;
                            }
                        }

                    }
                }
            }

            return collision;
        }

        public static bool CheckForGameOver(Position[] absPositions)
        {
            bool isGameOver = true;

            //loop through each piece position
            for (var z = 0; z < absPositions.Length; z++)
            {
                //check if any part of the last piece locked onto a visible part of the board
                if (absPositions[z].YPos > 1)
                {
                    isGameOver = false;
                }
            }

            return isGameOver;
        }

        public static Position[] GetAbsolutePositions(Piece piece)
        {
            List<Position> absPositions = new List<Position>();
            for (var y = 0; y < piece.Matrix.Length; y++)
            {
                for (var x = 0; x < piece.Matrix[y].Length; x++)
                {
                    if (piece.Matrix[y][x] == 1)
                    {
                        absPositions.Add(new Position(piece.BoardPosition.XPos + x, piece.BoardPosition.YPos + y, piece.Type));
                    }
                }
            }

            return absPositions.ToArray();
        }

        public static Tuple<int, Position[]> LockPieceToBoard(BoardPositions boardPositions, Piece activePiece)
        {
            Position[] absPositions = GetAbsolutePositions(activePiece);

            Position[][] tempPositions = boardPositions.ClonePositions();

            //loop through each piece position
            for (var z = 0; z < absPositions.Length; z++)
            {
                //update the position type on the board
                tempPositions[absPositions[z].YPos][absPositions[z].XPos].Type = absPositions[z].Type;
            }

            boardPositions.Positions = tempPositions;

            int linesCleared = TryClearLines(boardPositions, absPositions);

            return new Tuple<int, Position[]>(linesCleared, absPositions);
        }

        public static int TryClearLines(BoardPositions boardPositions, Position[] absPositions)
        {
            List<int> rowsToClear = new List<int>();

            //check for a clearable row
            for (var z = 0; z < absPositions.Length; z++)
            {
                if (!rowsToClear.Contains(absPositions[z].YPos))
                {
                    bool isClearable = true;

                    Position[] posRow = boardPositions.Positions[absPositions[z].YPos];

                    for (var x = 0; x < posRow.Length; x++)
                    {
                        //if there are any empty spaces in the row
                        if (Color.empty.Equals(posRow[x].Type))
                        {
                            isClearable = false;
                        }
                    }

                    if (isClearable)
                    {
                        rowsToClear.Add(absPositions[z].YPos);
                    }
                }
            }

            if (rowsToClear.Count > 0)
            {
                //clear lines at the end so the row indexes do not change while testing clearable rows
                foreach (int rowIndex in rowsToClear)
                {
                    ClearLine(boardPositions, rowIndex);
                }
            }

            return rowsToClear.Count;
        }

        public static void ClearLine(BoardPositions boardPositions, int rowIndex)
        {
            List<Position[]> tempPositions = boardPositions.Positions.ToList();
            tempPositions.RemoveAt(rowIndex);
            tempPositions.Insert(0, GetEmptyRow(0));
            boardPositions.Positions = tempPositions.ToArray();
        }

        public static Position[] GetEmptyRow(int rowIndex)
        {
            Position[] cols = new Position[10];

            for (int x = 0; x < cols.Length; x++)
            {
                cols[x] = new Position(x, rowIndex);
            }

            return cols;
        }

    }
}
