using System;
using System.Collections.Generic;
using System.Linq;

namespace TetrisApp.Models
{
    public static class AI
    {
        public static MovePath GetOptimalMoveForCurrentPiece(BoardPositions boardPositions, Piece activePiece, List<Piece> nextPieces)
        {
            //get all possible positions for the active piece
            List<Tuple<MovePath, BoardPositions>> allVisibleOutcomes = new List<Tuple<MovePath, BoardPositions>>()
            {
                new Tuple<MovePath, BoardPositions>(null, boardPositions)
            };
            allVisibleOutcomes = GetAllPieceOutcomes(allVisibleOutcomes, activePiece, false);

            //loop through each of the next pieces and get every possible board state for each piece, by looping through each board state created from the previous piece
            /*foreach (Piece piece in nextPieces)
            {
                allVisibleOutcomes = GetAllPieceOutcomes(allVisibleOutcomes, piece, true);
            }*/
            //only use the first next piece
            allVisibleOutcomes = GetAllPieceOutcomes(allVisibleOutcomes, nextPieces[0], true);

            if (allVisibleOutcomes.Any())
            {
                return allVisibleOutcomes.LastOrDefault().Item1;
            }

            return null;
        }

        private static List<Tuple<MovePath, BoardPositions>> GetAllPieceOutcomes(List<Tuple<MovePath, BoardPositions>> pathBoardStates, Piece piece, bool isFiltering)
        {
            List<MovePath> basicMoves = GetColorMoves(piece.Type);

            List<Tuple<MovePath, BoardPositions>> filteredResult = new List<Tuple<MovePath, BoardPositions>>();
            int minFilledCount = 1000;
            int minCoveredCount = 1000;
            int minChasmCount = 1000;
            int maxScore = 0;

            //loop over each of the possible starting board states
            for (int pbs=0; pbs < pathBoardStates.Count; pbs++)
            {
                BoardPositions boardState = pathBoardStates[pbs].Item2;

                //build a list of board states from each valid lock position for the current piece
                for (int i = 0; i < basicMoves.Count; i++)
                {
                    MovePath movePath = basicMoves[i];

                    //copy current piece from starting position
                    Piece tempPiece = piece.Clone();

                    //rotate and move the test piece into position
                    for (int m=0; m < movePath.Count; m++)
                    {
                        if (Move.rotateclockwise.Equals(movePath[m]))
                        {
                            Utilities.RotateTestPiece(tempPiece, 1);
                        }
                        else if (Move.left.Equals(movePath[m]))
                        {
                            tempPiece.BoardPosition.XPos -= 1;
                        }
                        else if (Move.right.Equals(movePath[m]))
                        {
                            tempPiece.BoardPosition.XPos += 1;
                        }
                    }

                    bool isMoveValid = true;

                    //move the piece down until there is a collision
                    for (int x = 0; x < boardState.Positions.Length - 1; x++)
                    {
                        if (x == 0)
                        {
                            //check if start position is invalid
                            Move col = Utilities.GetCollision(boardState.Positions, tempPiece);
                            if (!Move.none.Equals(col))
                            {
                                isMoveValid = false;
                                break;
                            }
                        }

                        tempPiece.BoardPosition.YPos += 1;

                        Move collision = Utilities.GetCollision(boardState.Positions, tempPiece);
                        if (!Move.none.Equals(collision))
                        {
                            //if there is a collision, get the previous valid position
                            tempPiece.BoardPosition.YPos -= 1;
                            break;
                        }
                    }

                    if (isMoveValid)
                    {

                        BoardPositions tempBoard = new BoardPositions()
                        {
                            Positions = boardState.ClonePositions()
                        };

                        //only include if lock does not cause Game Over
                        Tuple<int, Position[]> linesClearedAbsPositions = Utilities.LockPieceToBoard(tempBoard, tempPiece);
                        if (!Utilities.CheckForGameOver(linesClearedAbsPositions.Item2))
                        {
                            if (!isFiltering)
                            {

                                filteredResult.Add(new Tuple<MovePath, BoardPositions>(pathBoardStates[pbs].Item1 == null ? movePath : pathBoardStates[pbs].Item1, tempBoard));

                            }
                            else
                            {
                                int filledCount = 0;
                                int coveredCount = 0;
                                int chasmCount = 0;
                                int score = 0;

                                for (var y = 0; y < tempBoard.Positions.Length; y++)
                                {
                                    int fillScore = 0;
                                    for (var x = 0; x < tempBoard.Positions[y].Length; x++)
                                    {
                                        if (!Color.empty.Equals(tempBoard.Positions[y][x].Type))
                                        {
                                            //moves with the most cleared rows
                                            filledCount++;

                                            //moves with the fewest chasm spaces
                                            if (x == 1)
                                            {
                                                //chasm on the left wall
                                                Position emptyPos = tempBoard.Positions[y][0];
                                                if (Color.empty.Equals(emptyPos.Type))
                                                {
                                                    chasmCount++;
                                                }
                                            }
                                            else if (x == tempBoard.Positions[y].Length - 2)
                                            {
                                                //chasm on the right wall
                                                Position emptyPos = tempBoard.Positions[y][tempBoard.Positions[y].Length - 1];
                                                if (Color.empty.Equals(emptyPos.Type))
                                                {
                                                    chasmCount++;
                                                }
                                            }
                                            else if (x != tempBoard.Positions[y].Length - 1)
                                            {
                                                //chasm somewhere between pieces, but not the right edge
                                                Position emptyPos = tempBoard.Positions[y][x + 1];
                                                if (Color.empty.Equals(emptyPos.Type))
                                                {
                                                    Position filledPos = tempBoard.Positions[y][x + 2];
                                                    if (!Color.empty.Equals(filledPos.Type))
                                                    {
                                                        chasmCount++;
                                                    }
                                                }

                                            }

                                            //moves with the lowest row
                                            fillScore++;

                                        }
                                        else
                                        {
                                            //moves with the fewest covered spaces
                                            for (var z = y - 1; z >= 0; z--)
                                            {
                                                if (!Color.empty.Equals(tempBoard.Positions[z][x].Type))
                                                {
                                                    coveredCount++;
                                                    break;
                                                }
                                            }
                                        }

                                    }

                                    score += fillScore * y;

                                }

                                //moves with the most cleared rows
                                if (filledCount < minFilledCount)
                                {
                                    minFilledCount = filledCount;
                                    filteredResult = new List<Tuple<MovePath, BoardPositions>>();
                                    minCoveredCount = coveredCount;
                                    minChasmCount = chasmCount;
                                    maxScore = score;
                                }

                                if (filledCount == minFilledCount)
                                {
                                    //moves with the fewest covered spaces
                                    if (coveredCount < minCoveredCount)
                                    {
                                        minCoveredCount = coveredCount;
                                        filteredResult = new List<Tuple<MovePath, BoardPositions>>();
                                        minChasmCount = chasmCount;
                                        maxScore = score;
                                    }

                                    if (coveredCount == minCoveredCount)
                                    {
                                        //moves with the fewest chasm spaces
                                        if (chasmCount < minChasmCount)
                                        {
                                            minChasmCount = chasmCount;
                                            filteredResult = new List<Tuple<MovePath, BoardPositions>>();
                                            maxScore = score;
                                        }

                                        if (chasmCount == minChasmCount)
                                        {
                                            //moves with the lowest row
                                            if (score > maxScore)
                                            {
                                                maxScore = score;
                                                filteredResult = new List<Tuple<MovePath, BoardPositions>>();
                                            }

                                            if (score == maxScore)
                                            {
                                                filteredResult.Add(new Tuple<MovePath, BoardPositions>((pathBoardStates[pbs].Item1 == null ? movePath : pathBoardStates[pbs].Item1), tempBoard));
                                            }

                                        }

                                    }

                                }

                            }

                        }

                    }

                }

            }

            return filteredResult;
        }

        private static Dictionary<Color, List<MovePath>> _Moves { get; set; }

        private static List<MovePath> GetColorMoves(Color moveColor)
        {
            if (_Moves == null)
            {
                //direction[0-1], rotations[0-1,2,4], lateral distance 2,3,4,5,6,7
                Dictionary<Color, int[][]> colors = new Dictionary<Color, int[][]>()
                {
                    { Color.blue, new int[2][]{ new int[4] { 5, 5, 5, 5 }, new int[4] { 3, 3, 3, 4 } } },
                    { Color.green, new int[2][]{ new int[2] { 5, 6 }, new int[2] { 3, 3 } } },
                    { Color.orange, new int[2][]{ new int[4] { 5, 6, 5, 5 }, new int[4] { 3, 3, 3, 4} } },
                    { Color.purple, new int[2][]{ new int[4] { 5, 6, 5, 5 }, new int[4] { 3, 3, 3, 4 } } },
                    { Color.red, new int[2][]{ new int[2] { 5, 6 }, new int[2] { 3, 3 } } },
                    { Color.turquoise, new int[2][]{ new int[2] { 5, 7 }, new int[2] { 2, 3 } } },
                    { Color.yellow, new int[2][]{ new int[1] { 5 }, new int[1] { 4 } } }
                };

                _Moves = new Dictionary<Color, List<MovePath>>();

                foreach (KeyValuePair<Color, int[][]> colorMoves in colors)
                {
                    List<MovePath> allLockMoves = new List<MovePath>();

                    //rotations
                    for (int r = 0; r < colorMoves.Value[0].Length; r++)
                    {
                        //2 branches for each direction
                        for (int d = 0; d < 2; d++)
                        {
                            //5 branches for each lateral position
                            for (int m = 0; m < colorMoves.Value[d][r]; m++)
                            {
                                MovePath movePath = new MovePath();

                                //add 0-r rotations
                                for (int y = 0; y < r; y++)
                                {
                                    movePath.Add(Move.rotateclockwise);
                                }

                                //add 0-m + (0-1) moves to the side
                                for (int z = 0; z < m+d; z++)
                                {
                                    if (d == 0)
                                    {
                                        movePath.Add(Move.left);
                                    }
                                    else
                                    {
                                        movePath.Add(Move.right);
                                    }
                                }

                                allLockMoves.Add(movePath);
                            }
                        }
                    }

                    _Moves.Add(colorMoves.Key, allLockMoves);
                }
            }

            return _Moves[moveColor];
        }

    }
}
