using System;
using System.Collections.Generic;
using System.Linq;

namespace TetrisApp.Models
{
    public class Board
    {
        public Board()
        {
            SetupEmptyBoard();

            NextPieces.Add(GetRandomPiece());
            NextPieces.Add(GetRandomPiece());
            NextPieces.Add(GetRandomPiece());

            SetupNextPiece();

        }

        public string StartMessage { get; set; } = "Start Game";

        public int Score { get; set; } = 0;
        public int Level { get; set; } = 0;
        public int Lines { get; set; } = 0;

        //Speed out of 60 frames per second per row drop
        public int Speed { get; set; } = 48;

        public bool ResetLock { get; set; }

        public Piece ActivePiece { get; set; }
        public Piece ActiveShadow
        {
            get
            {
                Piece p = ActivePiece.Clone();
                p.BoardPosition.YPos = GetDropRowIndex();
                return p;
            }
        }

        public List<Piece> NextPieces { get; set; } = new List<Piece>();
        
        public BoardPositions BoardPositions { get; set; }

        private int GetDropRowIndex()
        {
            int dropRow = 0;

            Piece p = ActivePiece.Clone();
            
            for (var x=0; x<30; x++)
            {
                p.BoardPosition.YPos += 1;
                if (!Move.none.Equals(Utilities.GetCollision(BoardPositions.Positions, p)))
                {
                    dropRow = p.BoardPosition.YPos - 1;
                    break;
                }
            }
            return dropRow;
        }

        public void SetupNextPiece()
        {
            List<Piece> tempNextPieces = new List<Piece>(NextPieces);
            Piece tempActivePiece = tempNextPieces.First();
            tempActivePiece.IsActive = true;
            tempNextPieces.RemoveAt(0);
            tempNextPieces.Add(GetRandomPiece());

            NextPieces = tempNextPieces;
            ActivePiece = tempActivePiece;

            ResetLock = true;

        }

        public Move TryMove(int xVector, int yVector)
        {
            Piece testPiece = ActivePiece.Clone();

            testPiece.BoardPosition.XPos += xVector;
            testPiece.BoardPosition.YPos += yVector;

            Move collision = Utilities.GetCollision(BoardPositions.Positions, testPiece);

            if (Move.none.Equals(collision))
            {
                ActivePiece.BoardPosition.XPos = ActivePiece.BoardPosition.XPos + xVector;
                ActivePiece.BoardPosition.YPos = ActivePiece.BoardPosition.YPos + yVector;
                ResetLock = true;
            }

            return collision;
        }

        public Move TryRotate(int direction)
        {
            Piece testPiece = ActivePiece.Clone();
            Utilities.RotateTestPiece(testPiece, direction);

            Move collision = Utilities.GetCollision(BoardPositions.Positions, testPiece);
            if (Move.none.Equals(collision))
            {
                ActivePiece.Matrix = testPiece.Matrix;
                ResetLock = true;
            }

            return collision;
        }

        public bool TryRotateOffset(int xVector, int yVector, int dir)
        {
            //retry rotation offset one space
            Piece testPiece = ActivePiece.Clone();
            testPiece.BoardPosition.XPos += xVector;
            testPiece.BoardPosition.YPos += yVector;
            Utilities.RotateTestPiece(testPiece, dir);

            Move collision = Utilities.GetCollision(BoardPositions.Positions, testPiece);
            if (Move.none.Equals(collision))
            {
                //if there are no collisions, move the actual piece
                ActivePiece.BoardPosition.XPos += xVector;
                ActivePiece.BoardPosition.YPos += yVector;
                TryRotate(dir);
            }

            return Move.none.Equals(collision);
        }

        public void UpdateScoreLevelSpeed(int rowsToClear, bool isHardDrop)
        {
            UpdateScore(rowsToClear, isHardDrop);

            for (int x = 0; x < rowsToClear; x++)
            {
                UpdateLevelSpeed();
            }

        }

        private void UpdateLevelSpeed()
        {
            Lines++;
            if (Lines % 10 == 0)
            {
                Level++;

                if (Level == 1)
                {
                    Speed = 45;
                }
                else if (Level == 2)
                {
                    Speed = 38;
                }
                else if (Level == 3)
                {
                    Speed = 33;
                }
                else if (Level == 4)
                {
                    Speed = 28;
                }
                else if (Level == 5)
                {
                    Speed = 23;
                }
                else if (Level == 6)
                {
                    Speed = 18;
                }
                else if (Level == 7)
                {
                    Speed = 13;
                }
                else if (Level == 8)
                {
                    Speed = 8;
                }
                else if (Level == 9)
                {
                    Speed = 6;
                }
                else if (Level == 10 || Level == 11 || Level == 12)
                {
                    Speed = 5;
                }
                else if (Level == 13 || Level == 14 || Level == 15)
                {
                    Speed = 4;
                }
                else if (Level == 16 || Level == 17 || Level == 18)
                {
                    Speed = 3;
                }
                else if (Level >= 19 && Level <= 28)
                {
                    Speed = 2;
                }
                else if (Level > 28)
                {
                    Speed = 1;
                }
            }
        }

        private void UpdateScore(int rowsToClear, bool isHardDrop)
        {
            //calculate line clear score
            int multiplier = 0;
            if (rowsToClear == 1)
            {
                multiplier = 40;
            }
            else if (rowsToClear == 2)
            {
                multiplier = 100;
            }
            else if (rowsToClear == 3)
            {
                multiplier = 300;
            }
            else if (rowsToClear == 4)
            {
                multiplier = 1200;
            }

            Score = Score + ((Level + 1) * multiplier);

            //add piece score
            if (isHardDrop)
            {
                Score = Score + 8;
            }
            else
            {
                Score = Score + 4;
            }

        }

        private Piece GetRandomPiece()
        {
            Array values = Enum.GetValues(typeof(Models.Color));
            Random random = new Random();
            Color randomColor = (Color)values.GetValue(random.Next(1, values.Length));

            return new Piece(randomColor);
        }

        private void SetupEmptyBoard()
        {
            Position[][] rows = new Position[22][];

            for (int y = 0; y < rows.Length; y++)
            {
                rows[y] = Utilities.GetEmptyRow(y);
            }

            BoardPositions = new BoardPositions()
            {
                Positions = rows
            };
        }


    }
}
