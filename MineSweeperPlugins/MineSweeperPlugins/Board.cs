using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MineSweepersPlugins
{
    public class Board
    {
        private int width = 10;
        private int hight = 10;
        private int mineCount = 10;
        private int explodedCount = 0;
        private int revealdCount = 0;
        private bool firstSafe = false;

        public int Width { get { return width; } }
        public int Hight { get { return hight; } }
        public int MineCount { get { return mineCount; } }
        public int ExplodedCount { get { return explodedCount; } }
        public int RevealdCount { get { return revealdCount; } }

        private int[,] mines;
        private bool[,] reveald;
        private bool[,] flags;
        
        public Board(int hight, int width, int mineCount)
        {
            this.hight = hight;
            this.width = width;
            this.mineCount = mineCount;

            mines = new int[hight, width];
            reveald = new bool[hight, width];
            flags = new bool[hight, width];

            placeMines();
            calculateTiles();
        }

        private void placeMines()
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

        private void calculateTiles()
        {
            for (int i = 0; i < hight; i++)
                for (int j = 0; j < width; j++)
                {
                    if (mines[i, j] != 10)
                        mines[i, j] = NumOfMines(i, j);
                }
        }

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

        public int GetStartX(int x) => (x - 1) < 0 ? 0 : x - 1;
        public int GetStartY(int y) => (y - 1) < 0 ? 0 : y - 1;
        public int GetEndX(int x) => (x + 1) >= width ? width - 1 : x + 1;
        public int GetEndY(int y) => (y + 1) >= hight ? hight - 1 : y + 1;


        public int GetTile(int y, int x) => mines[y, x];

        public bool IsReveald(int y, int x) => reveald[y, x];

        public bool IsFlag(int y, int x) => flags[y, x];

        public bool IsMine(int y, int x) => mines[y, x] == 10;

        public void Reveal(int y , int x)
        {
            if (!reveald[y, x])
                revealdCount++;
            if (mines[y, x] == 10) explodedCount++;
            reveald[y, x] = true;
        }


        public void ToggleFlag(int y, int x)
        {
            flags[y, x] = !flags[y, x];
        }

        public int NumOfFlags(int y, int x)
        {
            int count = 0;

            for (int i = GetStartY(y); i <= GetEndY(y); i++)
                for (int j = GetStartX(x); j <= GetEndX(x); j++)
                    if (flags[i, j])
                        count++;

            return count;
        }

        public List<Dictionary<byte, object>> OpenRest(int y, int x)
        {
            
            List<Dictionary<byte, object>> tiles = new List<Dictionary<byte, object>>();
            tiles.Add( new Dictionary<byte, object>());
            int current = 0;
            byte index = 0;

            //List<(int, int, int)> tiles = new List<(int, int, int)>();

            for (int i = GetStartY(y); i <= GetEndY(y); i++)
                for (int j = GetStartX(x); j <= GetEndX(x); j++)
                    if (!IsReveald(i, j) && !IsFlag(i, j))
                    {
                        Reveal(i, j);
                        tiles[current].Add(index++, i);
                        tiles[current].Add(index++, j);
                        tiles[current].Add(index++, mines[i, j]);
                        //tiles.Add((i, j, mines[i, j]));
                       

                        if (mines[i, j] == 0)
                            tiles.AddRange(Expand(i, j,ref tiles,ref current,ref index));
                    }

            return tiles;
        }
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
        
        public List<Dictionary<byte,object>> AllReveald()
        {
            List<Dictionary<byte, object>> tiles = new List<Dictionary<byte, object>>();
            tiles.Add(new Dictionary<byte, object>());
            int current = 0;
            byte index = 0;

            for (int i = 0; i <= hight; i++)
                for (int j = 0; j <= width; j++)
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

        public List<Dictionary<byte, object>> AllFlags()
        {
            List<Dictionary<byte, object>> tiles = new List<Dictionary<byte, object>>();
            tiles.Add(new Dictionary<byte, object>());
            int current = 0;
            byte index = 0;

            for (int i = 0; i <= hight; i++)
                for (int j = 0; j <= width; j++)
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
        private void UpdateNeighbourTiles(int y,int x)
        {
            for (int i = GetStartY(y); i <= GetEndY(y); i++)
                for (int j = GetStartX(x); j <= GetEndX(x); j++)
                    if (mines[i, j] != 10)
                        mines[i, j] = NumOfMines(i, j);
        }
        public bool CheckIfComplete()
        {
            if ((revealdCount + mineCount-explodedCount )== width * hight)
                return true;
            else
                return false;
            
        }
    }
}
