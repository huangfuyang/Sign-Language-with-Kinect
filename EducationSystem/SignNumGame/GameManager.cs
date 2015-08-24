using System;
using System.Collections.Generic;
using System.Linq;

namespace EducationSystem.SignNumGame
{
    public class GameManager
    {

        private const int INITIAL_TILE_COUNT = 2;

        public Grid Board { get; private set; }
        public int Score { get; private set; }
        public int BestScore { get; private set; }

        public enum Direction { UP, DOWN, LEFT, RIGHT }

        public GameManager(Grid grid)
        {
            this.Board = grid;
            Reset();
        }

        private void Reset()
        {
            Score = BestScore = 0;

            // Remove previous tiles and Add initial tiles
            Board.Empty();
            AddRandomTile(INITIAL_TILE_COUNT);
        }

        private void AddRandomTile(int maxSize)
        {
            List<Point> selectedCells = Board.AvailableCells(maxSize, true);
            Random random = new Random();

            foreach (Point point in selectedCells)
            {
                int value = (random.NextDouble() < 0.9 ? 0 : 1);

                Board.InsertTile(new Tile(point.X, point.Y, value));
            }
        }

        public void UpdateGame(bool isMoved)
        {
            if (isMoved)
            {
                this.AddRandomTile(1);
            }

            for (int i = 0; i < Board.BoardSize; i++)
            {
                for (int j = 0; j < Board.BoardSize; j++)
                {
                    Tile tile = Board.Tiles[i, j];
                    if (tile != null)
                    {
                        tile.MergeFrom = null;
                    }
                }
            }

            if (BestScore < Score)
            {
                BestScore = Score;
            }
        }

        private Point getDirectionDelta(Direction direction)
        {
            switch (direction)
            {
                case Direction.UP:
                    return new Point(0, -1);
                case Direction.DOWN:
                    return new Point(0, 1);
                case Direction.LEFT:
                    return new Point(-1, 0);
                case Direction.RIGHT:
                    return new Point(1, 0);
                default:
                    return new Point(0, 0);
            }
        }

        private IEnumerable<int> CreateTransversalOrder(int axisDelta)
        {
            IEnumerable<int> transversalOrder = Enumerable.Range(0, Board.BoardSize);

            // Right -> Search along x-axis using Reverse order
            // Down  -> Search along y-axis using Reverse order
            if (axisDelta > 0)
            {
                transversalOrder = transversalOrder.Reverse();
            }

            return transversalOrder;
        }

        public void MergeTile(Tile movingTile, Tile mergedTile)
        {
            int mergedValue = GetNextValue(movingTile.Value);
            Tile newMergedTile = new Tile(mergedTile.Position.X, mergedTile.Position.Y, mergedValue);
            newMergedTile.MergeFrom = movingTile;

            Board.RemoveTile(movingTile);
            Board.RemoveTile(mergedTile);
            movingTile.UpdatePosition(mergedTile.Position);
            Board.InsertTile(newMergedTile);
            Score += mergedValue;
        }

        public void Move(Tile tile, Point newPosition)
        {
            Board.RemoveTile(tile);
            tile.UpdatePosition(newPosition);
            Board.InsertTile(tile);
        }

        public bool Move(Direction direction)
        {
            Point delta = getDirectionDelta(direction);
            IEnumerable<int> xTransversalOrder = CreateTransversalOrder(delta.X);
            IEnumerable<int> yTransversalOrder = CreateTransversalOrder(delta.Y);
            bool isAnyTileMoved = false;

            foreach (int x in xTransversalOrder)
            {
                foreach (int y in yTransversalOrder)
                {
                    Tile movingTile = Board.Tiles[x, y];

                    if (movingTile != null)
                    {
                        Point targetPosition = GetFarthestPosition(movingTile.Position, delta);
                        Point mergePosition = targetPosition.Add(delta);

                        if (IsTilesMergeable(movingTile, mergePosition))
                        {
                            Tile mergedTile = Board.Tiles[mergePosition.X, mergePosition.Y];
                            MergeTile(movingTile, mergedTile);
                            isAnyTileMoved = true;
                        }
                        else if (!targetPosition.Equals(movingTile.Position))
                        {
                            Move(movingTile, targetPosition);
                            isAnyTileMoved = true;
                        }
                    }
                }
            }

            return isAnyTileMoved;
        }

        private int GetNextValue(int value)
        {
            if (value < 0)
            {
                return 0;
            }
            else
            {
                return value + 1;
            }
        }

        private bool IsTilesMergeable(Tile movingTile, Point mergedPosition)
        {
            if ((movingTile != null) && (mergedPosition != null)
                && Board.IsCellOccupied(mergedPosition.X, mergedPosition.Y))
            {
                Tile mergedTile = Board.Tiles[mergedPosition.X, mergedPosition.Y];
                return (movingTile.Value == mergedTile.Value) && (mergedTile.MergeFrom == null);
            }

            return false;
        }

        private Point GetFarthestPosition(Point position, Point delta)
        {
            Point previousPosition = new Point(-1, -1);
            Point currentPosition = new Point(position.X, position.Y);

            do
            {
                previousPosition.X = currentPosition.X;
                previousPosition.Y = currentPosition.Y;
                currentPosition.X += delta.X;
                currentPosition.Y += delta.Y;
            } while (Board.IsCellAvailable(currentPosition.X, currentPosition.Y));

            return previousPosition;
        }
    }
}
