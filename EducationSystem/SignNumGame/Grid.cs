using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace EducationSystem.SignNumGame
{
    public class Grid
    {
        public int BoardSize { get; private set; }

        public Tile[,] Tiles { get; private set; }

        public ObservableCollection<Tile> TileCollection { get; private set; }

        public delegate void ForEachCellCallback(Tile tile, int x, int y);

        public Grid(int boardSize)
        {
            BoardSize = boardSize;
            Tiles = new Tile[boardSize, boardSize];
            TileCollection = new ObservableCollection<Tile>();
        }

        private void ForEachCell(ForEachCellCallback callback)
        {
            for (int i = 0; i <= Tiles.GetUpperBound(0); i++)
            {
                for (int j = 0; j <= Tiles.GetUpperBound(1); j++)
                {
                    callback(Tiles[i, j], i, j);
                }
            }
        }

        public void Empty()
        {
            ForEachCell((Tile tile, int x, int y) =>
            {
                Tiles[x, y] = null;
            });
        }

        public List<Point> AvailableCells(int maxSize, bool isRandom)
        {
            List<Point> availableCells = new List<Point>();

            ForEachCell((Tile tile, int x, int y) =>
            {
                if (IsCellAvailable(x, y))
                {
                    availableCells.Add(new Point(x, y));
                }
            });

            if (isRandom)
            {
                Shuffle(availableCells);
            }

            return availableCells.GetRange(0, maxSize);
        }

        private void Shuffle<T>(List<T> list)
        {
            Random random = new Random();
            for (int i = list.Count - 1; i > 1; i--)
            {
                int k = random.Next(i + 1);
                T value = list[k];
                list[k] = list[i];
                list[i] = value;
            }
        }

        private bool IsPositionValid(int x, int y)
        {
            return (x >= 0 && x < BoardSize && y >= 0 && y < BoardSize);
        }

        public bool IsCellAvailable(int x, int y)
        {
            return IsPositionValid(x, y) && (Tiles[x, y] == null);
        }

        public bool IsCellOccupied(int x, int y)
        {
            return IsPositionValid(x, y) && (Tiles[x, y] != null);
        }

        public void InsertTile(Tile tile)
        {
            if (IsPositionValid(tile.Position.X, tile.Position.Y))
            {
                Tiles[tile.Position.X, tile.Position.Y] = tile;
                TileCollection.Add(tile);
            }
        }

        public void RemoveTile(Tile tile)
        {
            int x = tile.Position.X;
            int y = tile.Position.Y;

            if (tile != null && IsPositionValid(x, y))
            {
                Tiles[x, y] = null;
                TileCollection.Remove(tile);
            }
        }
    }
}
