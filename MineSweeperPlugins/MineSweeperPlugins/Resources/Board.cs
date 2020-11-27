using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MineSweepersPlugins
{
    /// <summary>
    /// Board represents playing field and manages data that board contains.
    /// </summary>
    public class Board
    {
        /// <summary>
        /// Board width
        /// </summary>
        private int width = 10;
        /// <summary>
        /// Board hight
        /// </summary>
        private int hight = 10;
        /// <summary>
        /// number of mines on the board
        /// </summary>
        private int mineCount = 10;
        /// <summary>
        /// number of mines that have been detonated
        /// </summary>
        private int explodedCount = 0;
        /// <summary>
        /// Number of fileds that have been reveald
        /// </summary>
        private int revealdCount = 0;
        /// <summary>
        /// Is first opened tile always safe
        /// </summary>
        private bool firstSafe = false;

        /// <summary>
        /// Board width
        /// </summary>
        public int Width { get { return width; } }
        /// <summary>
        /// Board hight
        /// </summary>
        public int Hight { get { return hight; } }
        /// <summary>
        /// Number of mines
        /// </summary>
        public int MineCount { get { return mineCount; } }
        /// <summary>
        /// number of mines that have been detonated
        /// </summary>
        public int ExplodedCount { get { return explodedCount; } }
        /// <summary>
        /// Number of fileds that have been reveald
        /// </summary>
        public int RevealdCount { get { return revealdCount; } }

        /// <summary>
        /// Field of tiles represneting the board
        /// </summary>
        private int[,] mines;
        /// <summary>
        /// Filed of reveald mines
        /// </summary>
        private bool[,] reveald;
        /// <summary>
        /// Field of flags
        /// </summary>
        private bool[,] flags;
        
        public Board(int hight, int width, int mineCount)
        {
            this.hight = hight;
            this.width = width;
            this.mineCount = mineCount;

            mines = new int[hight, width];
            reveald = new bool[hight, width];
            flags = new bool[hight, width];

            PlaceMines();
            CalculateTiles();
        }
        /// <summary>
        /// Place mines on the board
        /// </summary>
        private void PlaceMines()
        {
            int count = 0;
            Random r = new Random();
            while (count < mineCount)
            {
                
                int x = r.Next(width);
                int y = r.Next(hight);
                if (mines[y, x] != 10)
                {
                    mines[y, x] = 10;
                    count++;
                }
            }
        }
        /// <summary>
        /// Calculate tiles depending on surrounding mines
        /// </summary>
        private void CalculateTiles()
        {
            for (int i = 0; i < hight; i++)
                for (int j = 0; j < width; j++)
                {
                    if (mines[i, j] != 10)
                        mines[i, j] = NumOfMines(i, j);
                }
        }
        /// <summary>
        /// count number of mines around given tile
        /// </summary>
        /// <param name="y"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        public int NumOfMines(int y, int x)
        {
            int count = 0;
            for (int i = GetStartY(y); i <= GetEndY(y); i++)
                for (int j = GetStartX(x); j <= GetEndX(x); j++)
                {

                    if (mines[i, j] == 10) count++;
                }
            return count;
        }
        /// <summary>
        /// clamp and give start cordinate for neighbour tiles
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public int GetStartX(int x) => (x - 1) < 0 ? 0 : x - 1;
        /// <summary>
        /// clamp and give start cordinate for neighbour tiles
        /// </summary>
        /// <param name="y"></param>
        /// <returns></returns>
        public int GetStartY(int y) => (y - 1) < 0 ? 0 : y - 1;
        /// <summary>
        /// clamp and give start cordinate for neighbour tiles
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public int GetEndX(int x) => (x + 1) >= width ? width - 1 : x + 1;
        /// <summary>
        /// clamp and give start cordinate for neighbour tiles
        /// </summary>
        /// <param name="y"></param>
        /// <returns></returns>
        public int GetEndY(int y) => (y + 1) >= hight ? hight - 1 : y + 1;

        /// <summary>
        /// Get given tile on given cordinates
        /// </summary>
        /// <param name="y"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        public int GetTile(int y, int x) => mines[y, x];
        /// <summary>
        /// Check if tile is reveald
        /// </summary>
        /// <param name="y"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        public bool IsReveald(int y, int x) => reveald[y, x];
        /// <summary>
        /// check if tile is a flag
        /// </summary>
        /// <param name="y"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        public bool IsFlag(int y, int x) => flags[y, x];
        /// <summary>
        /// check if tile is a mine
        /// </summary>
        /// <param name="y"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        public bool IsMine(int y, int x) => mines[y, x] == 10;
        /// <summary>
        /// Reveal a tile
        /// </summary>
        /// <param name="y"></param>
        /// <param name="x"></param>
        public void Reveal(int y , int x)
        {
            if (!reveald[y, x])
                revealdCount++;
            if (mines[y, x] == 10) explodedCount++;
            reveald[y, x] = true;
        }

        /// <summary>
        /// Toggle the flag state 
        /// </summary>
        /// <param name="y"></param>
        /// <param name="x"></param>
        public void ToggleFlag(int y, int x)
        {
            flags[y, x] = !flags[y, x];
        }
        /// <summary>
        /// count number of flags
        /// </summary>
        /// <param name="y"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        public int NumOfFlags(int y, int x)
        {
            int count = 0;

            for (int i = GetStartY(y); i <= GetEndY(y); i++)
                for (int j = GetStartX(x); j <= GetEndX(x); j++)
                    if (flags[i, j])
                        count++;

            return count;
        }
        /// <summary>
        /// open neighbour tiles if not flaged or al ready opened
        /// </summary>
        /// <param name="y"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        public List<Dictionary<byte, object>> OpenRest(int y, int x)
        {
            
            List<Dictionary<byte, object>> tiles = new List<Dictionary<byte, object>>();
            tiles.Add( new Dictionary<byte, object>());
            int current = 0;
            byte index = 0;


            for (int i = GetStartY(y); i <= GetEndY(y); i++)
                for (int j = GetStartX(x); j <= GetEndX(x); j++)
                    if (!IsReveald(i, j) && !IsFlag(i, j))
                    {
                        Reveal(i, j);
                        tiles[current].Add(index++, i);
                        tiles[current].Add(index++, j);
                        tiles[current].Add(index++, mines[i, j]);
                       

                        if (mines[i, j] == 0)
                            tiles.AddRange(Expand(i, j,ref tiles,ref current,ref index));
                    }

            return tiles;
        }
        /// <summary>
        /// Expand all Empty tiles
        /// </summary>
        /// <param name="y"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        public List<Dictionary<byte, object>> Expand(int y,int x)
        {
            List<Dictionary<byte, object>> tiles = new List<Dictionary<byte, object>>();
            tiles.Add(new Dictionary<byte, object>());
            int current = 0;
            byte index = 0;

            Expand(y, x, ref tiles, ref current, ref index);            

            return tiles;
        }
        private List<Dictionary<byte, object>> Expand(int y, int x, ref List<Dictionary<byte, object>> tiles, ref int current, ref byte index)
        {
            for (int i = GetStartY(y); i <= GetEndY(y); i++)
                for (int j = GetStartX(x); j <= GetEndX(x); j++)
                    if (!IsReveald(i, j))
                    {
                        Reveal(i, j);
                        tiles[current].Add(index++, i);
                        tiles[current].Add(index++, j);
                        tiles[current].Add(index++, mines[i, j]);
                        //tiles.Add((i, j,mines[i,j]));

                        if (index >= 255)
                        {
                            tiles.Add(new Dictionary<byte, object>());
                            current++;
                            index = 0;
                        }

                        if (mines[i, j] == 0)
                            Expand(i, j, ref tiles, ref current, ref index);
                    }

            return tiles;
        }
        /// <summary>
        /// Get all reveald tiles
        /// </summary>
        /// <returns></returns>
        public List<Dictionary<byte,object>> AllReveald()
        {
            List<Dictionary<byte, object>> tiles = new List<Dictionary<byte, object>>();
            tiles.Add(new Dictionary<byte, object>());
            int current = 0;
            byte index = 0;

            for (int i = 0; i < hight; i++)
                for (int j = 0; j < width; j++)
                    if (IsReveald(i, j))
                    {
                        tiles[current].Add(index++, i);
                        tiles[current].Add(index++, j);
                        tiles[current].Add(index++, mines[i, j]);

                        if (index >= 255)
                        {
                            tiles.Add(new Dictionary<byte, object>());
                            current++;
                            index = 0;
                        }
                    }

                        return tiles;
        }
        /// <summary>
        /// Get all flags
        /// </summary>
        /// <returns></returns>
        public List<Dictionary<byte, object>> AllFlags()
        {
            List<Dictionary<byte, object>> tiles = new List<Dictionary<byte, object>>();
            tiles.Add(new Dictionary<byte, object>());
            int current = 0;
            byte index = 0;

            for (int i = 0; i < hight; i++)
                for (int j = 0; j < width; j++)
                    if (IsFlag(i, j))
                    {
                        tiles[current].Add(index++, i);
                        tiles[current].Add(index++, j);

                        if (index >= 254)
                        {
                            tiles.Add(new Dictionary<byte, object>());
                            current++;
                            index = 0;
                        }
                    }

            return tiles;
        }
        /// <summary>
        /// swapMine and place it elsewhere 
        /// </summary>
        /// <param name="y"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        public int SwapMine(int y,int x)
        {
            Random r = new Random();
            while (true)
            {
                int newX = r.Next(width);
                int newY = r.Next(hight);
                if (mines[newY, newX] != 10 && !reveald[newY,newX])
                {
                    mines[newY, newX] = 10;
                    UpdateNeighbourTiles(newY, newX);

                    mines[y, x] = NumOfMines(y, x);
                    UpdateNeighbourTiles(y, x);

                    return mines[y, x];
                }
            }
        }
        /// <summary>
        /// Update neighbour tiles
        /// </summary>
        /// <param name="y"></param>
        /// <param name="x"></param>
        private void UpdateNeighbourTiles(int y,int x)
        {
            for (int i = GetStartY(y); i <= GetEndY(y); i++)
                for (int j = GetStartX(x); j <= GetEndX(x); j++)
                    if (mines[i, j] != 10)
                        mines[i, j] = NumOfMines(i, j);
        }
        /// <summary>
        /// Check if board is complete
        /// </summary>
        /// <returns></returns>
        public bool CheckIfComplete()
        {
            if ((revealdCount + mineCount-explodedCount )== width * hight)
                return true;
            else
                return false;
            
        }
    }
}
