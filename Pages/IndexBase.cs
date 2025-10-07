using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Threading.Tasks;
using TetrisApp.Models;

namespace TetrisApp.Pages
{
    public class IndexBase : ComponentBase
    {

        protected Board Board = new Board();

        protected ElementReference keyboardRef;

        protected bool Paused { get; set; } = true;

        protected bool IsAI { get; set; }
        
        private bool IsPieceDropping { get; set; }

        private bool IsPieceLocking { get; set; }

        private int LockCounter { get; set; }

        private int LockSpeed {
            get
            {
                if (Board.Level >= 32)
                {
                    return 15;
                }
                return 15 + ((15 * (32 - Board.Level)) / 32);
            }
        }

        private bool SlidingLeft { get; set; }
        private bool SlidingRight { get; set; }

        private int FrameDuration { get; set; } = 1000 / 60;

        //Game execution loop
        private async void StartGameLoop()
        {
            IsPieceDropping = true;
            int dropCounter = 0;
            int slideLeftCounter = 0;
            int slideRightCounter = 0;

            IsPieceLocking = false;
            LockCounter = 0;

            while (true)
            {
                var timer = Task.Delay(FrameDuration);

                if (!Paused)
                {

                    //every time the active piece moves/rotates
                    if (Board.ResetLock)
                    {
                        Board.ResetLock = false;
                        LockCounter = 0;
                        IsPieceLocking = false;

                        //test the next drop position
                        Piece testPiece = Board.ActivePiece.Clone();
                        testPiece.BoardPosition.YPos += 1;
                        if (!Move.none.Equals(Utilities.GetCollision(Board.BoardPositions.Positions, testPiece)))
                        {
                            //cannot drop again, start lock counter
                            IsPieceLocking = true;

                        }

                    }

                    if (IsPieceLocking)
                    {
                        LockCounter++;
                        if (LockCounter >= LockSpeed)
                        {
                            LockCounter = 0;
                            ProcessLockPiece(false);

                        }

                    }
                    else if (IsPieceDropping)
                    {
                        dropCounter++;
                        if (dropCounter >= Board.Speed)
                        {
                            dropCounter = 0;

                            //drop piece
                            Move collision = Board.TryMove(0, 1);
                            
                        }
                    }

                    if (SlidingLeft)
                    {
                        slideLeftCounter++;
                        if (slideLeftCounter >= 3)
                        {
                            slideLeftCounter = 0;
                            MoveLeftCommand();
                        }
                    }
                    else if (SlidingRight)
                    {
                        slideRightCounter++;
                        if (slideRightCounter >= 3)
                        {
                            slideRightCounter = 0;
                            MoveRightCommand();
                        }
                    }

                    if (IsAI)
                    {
                        if (IsPieceDropping && !IsPieceLocking && !SlidingLeft && !SlidingRight)
                        {
                            MovePath aiMoves = AI.GetOptimalMoveForCurrentPiece(Board.BoardPositions, Board.ActivePiece, Board.NextPieces);

                            for(int m = 0; m < aiMoves.Count; m++)
                            {
                                if (Move.left.Equals(aiMoves[m]))
                                {
                                    MoveLeftCommand();
                                }
                                else if (Move.right.Equals(aiMoves[m]))
                                {
                                    MoveRightCommand();
                                }
                                else if (Move.rotateclockwise.Equals(aiMoves[m]))
                                {
                                    RotateClockwiseCommand();
                                }
                            }

                            MoveDownCommand();
                            
                        }
                    }

                    StateHasChanged();
                }

                await timer;

            }
        }

        //Handle page events
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await keyboardRef.FocusAsync();
            }
        }

        protected void HandleKeyUp(KeyboardEventArgs e)
        {
            if (e.Key == "ArrowLeft" || e.Key == "A" || e.Key == "a")
            {
                SlidingLeft = false;
            }
            else if (e.Key == "ArrowRight" || e.Key == "D" || e.Key == "d")
            {
                SlidingRight = false;
            }
        }

        protected void HandleKeyDown(KeyboardEventArgs e)
        {
            if (e.Key == "Enter")
            {
                IsAI = false;
                StartCommand();
            }
            else if (e.Key == "ArrowUp" || e.Key == "W" || e.Key == "w")
            {
                //Rotate
                RotateClockwiseCommand();
            }
            else if (e.Key == "ArrowDown" || e.Key == "S" || e.Key == "s")
            {
                //ArrowDown
                MoveDownCommand();
            }
            else if (e.Key == "ArrowLeft" || e.Key == "A" || e.Key == "a")
            {
                //ArrowLeft
                //MoveLeftCommand();
                if (!SlidingRight)
                {
                    SlidingLeft = true;
                }
            }
            else if (e.Key == "ArrowRight" || e.Key == "D" || e.Key == "d")
            {
                //ArrowRight
                //MoveRightCommand();
                if (!SlidingLeft)
                {
                    SlidingRight = true;
                }
            }
        }

        protected void HandleBlur(FocusEventArgs e)
        {
            Paused = true;
        }

        protected void HandleAIClick(MouseEventArgs e)
        {
            IsAI = true;
            StartCommand();
        }

        protected void HandleStartClick(MouseEventArgs e)
        {
            IsAI = false;
            StartCommand();
        }

        protected void HandleEnterClick(MouseEventArgs e)
        {
            IsAI = false;
            StartCommand();
        }

        protected void HandleUpClick(MouseEventArgs e)
        {
            RotateClockwiseCommand();
        }

        protected void HandleLeftClick(MouseEventArgs e)
        {
            MoveLeftCommand();
        }

        protected void HandleDownClick(MouseEventArgs e)
        {
            MoveDownCommand();
        }

        protected void HandleRightClick(MouseEventArgs e)
        {
            MoveRightCommand();
        }
        //END Handle page events

        //Commands
        private void StartCommand()
        {
            if (Paused)
            {
                //Resume
                Paused = false;
                keyboardRef.FocusAsync();

                if (Board.StartMessage == "Start Game")
                {
                    StartGameLoop();
                    Board.StartMessage = "Resume";
                }
                else if (Board.StartMessage == "Restart Game")
                {
                    Board = new Board();
                    Board.StartMessage = "Resume";
                }
                
                IsPieceDropping = true;
            }
            else
            {
                //Pause
                Paused = true;
            }

            StateHasChanged();

        }

        private void ShiftDownCommand()
        {
            if (!Paused)
            {
                Move collision = Board.TryMove(0, 1);
            }
        }

        private void MoveDownCommand()
        {
            if (!Paused)
            {
                for (var x = 0; x < 30; x++)
                {
                    if (!Move.none.Equals(Board.TryMove(0, 1)))
                    {
                        break;
                    }
                }

                ProcessLockPiece(true);

                StateHasChanged();

            }
        }

        private void MoveRightCommand()
        {
            if (!Paused)
            {
                Move collision = Board.TryMove(1, 0);
            }
        }

        private void MoveLeftCommand()
        {
            if (!Paused)
            {
                Move collision = Board.TryMove(-1, 0);
            }
        }

        private void RotateClockwiseCommand()
        {
            if (!Paused)
            {
                int dir = 1;

                Move collision = Board.TryRotate(dir);

                if (!Move.none.Equals(collision))
                {

                    if (Move.down.Equals(collision))
                    {
                        if (!Board.TryRotateOffset(0, -1, dir))
                        {
                            if (!Board.TryRotateOffset(-1, 0, dir))
                            {
                                if (!Board.TryRotateOffset(1, 0, dir))
                                {
                                    if (!Board.TryRotateOffset(0, -2, dir))
                                    {
                                        if (!Board.TryRotateOffset(-2, 0, dir))
                                        {
                                            if (!Board.TryRotateOffset(2, 0, dir))
                                            {
                                                if (!Board.TryRotateOffset(-1, -1, dir))
                                                {
                                                    if (!Board.TryRotateOffset(-1, 1, dir))
                                                    {
                                                        if (!Board.TryRotateOffset(1, -1, dir))
                                                        {
                                                            if (!Board.TryRotateOffset(1, 1, dir))
                                                            {

                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else if (Move.left.Equals(collision))
                    {
                        if (!Board.TryRotateOffset(1, 0, dir))
                        {
                            if (!Board.TryRotateOffset(0, -1, dir))
                            {
                                if(!Board.TryRotateOffset(0, 1, dir))
                                {
                                    if (!Board.TryRotateOffset(2, 0, dir))
                                    {
                                        if (!Board.TryRotateOffset(0, -2, dir))
                                        {
                                            if (!Board.TryRotateOffset(0, 2, dir))
                                            {
                                                if (!Board.TryRotateOffset(-1, -1, dir))
                                                {
                                                    if (!Board.TryRotateOffset(-1, 1, dir))
                                                    {
                                                        if (!Board.TryRotateOffset(1, -1, dir))
                                                        {
                                                            if (!Board.TryRotateOffset(1, 1, dir))
                                                            {

                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else if (Move.right.Equals(collision))
                    {
                        if (!Board.TryRotateOffset(-1, 0, dir))
                        {
                            if (!Board.TryRotateOffset(0, -1, dir))
                            {
                                if(!Board.TryRotateOffset(0, 1, dir))
                                {
                                    if (!Board.TryRotateOffset(-2, 0, dir))
                                    {
                                        if (!Board.TryRotateOffset(0, -2, dir))
                                        {
                                            if (!Board.TryRotateOffset(0, 2, dir))
                                            {
                                                if (!Board.TryRotateOffset(-1, -1, dir))
                                                {
                                                    if (!Board.TryRotateOffset(-1, 1, dir))
                                                    {
                                                        if (!Board.TryRotateOffset(1, -1, dir))
                                                        {
                                                            if (!Board.TryRotateOffset(1, 1, dir))
                                                            {

                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else if (Move.up.Equals(collision))
                    {
                        if (!Board.TryRotateOffset(0, 1, dir))
                        {
                            if (!Board.TryRotateOffset(-1, 0, dir))
                            {
                                if(!Board.TryRotateOffset(1, 0, dir))
                                {
                                    if (!Board.TryRotateOffset(0, 2, dir))
                                    {
                                        if (!Board.TryRotateOffset(-2, 0, dir))
                                        {
                                            if (!Board.TryRotateOffset(2, 0, dir))
                                            {
                                                if (!Board.TryRotateOffset(-1, -1, dir))
                                                {
                                                    if (!Board.TryRotateOffset(-1, 1, dir))
                                                    {
                                                        if (!Board.TryRotateOffset(1, -1, dir))
                                                        {
                                                            if (!Board.TryRotateOffset(1, 1, dir))
                                                            {

                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                }
            }
        }
        //END Commands

        public void ProcessLockPiece(bool isHardDrop)
        {
            Tuple<int, Position[]> linesClearedAbsPositions = Utilities.LockPieceToBoard(Board.BoardPositions, Board.ActivePiece);

            Board.UpdateScoreLevelSpeed(linesClearedAbsPositions.Item1, isHardDrop);

            if (Utilities.CheckForGameOver(linesClearedAbsPositions.Item2))
            {
                //if piece resulted in game over
                Board.StartMessage = "Restart Game";
                Paused = true;
            }
            else
            {
                Board.SetupNextPiece();
            }
        }

    }
}
