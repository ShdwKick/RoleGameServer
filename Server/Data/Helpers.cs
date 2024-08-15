using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Data.Helpers
{
    public class Helpers
    {
        public static async Task<List<HexBlockData>> CallculateBlocksCoordByRadius(int radius)
        {
            List<HexBlockData> hexs = new List<HexBlockData>();
            int startIndex = radius * -1;
            for (int i = startIndex; i < radius; i++)
            {
                int r1 = Math.Max(startIndex, i * -1 - radius);
                int r2 = Math.Min(i * -1 + radius, radius);

                for (int j = r1; j < r2; j++)
                {
                    var hex = new HexBlockData
                    {
                        Q = i,
                        R = j,
                        S = i * -1 - j,
                    };
                    hexs.Add(hex);
                }
            }
            return await Task.FromResult(hexs);
        }

        public static async Task<List<HexBlockData>> AddBlocksForNextRadius(int currentRadius)
        {
            int nextRadius = currentRadius + 1;
            List<HexBlockData> newHexs = new List<HexBlockData>();

            for (int q = -nextRadius; q <= nextRadius; q++)
            {
                for (int r = Math.Max(-nextRadius, -q - nextRadius); r <= Math.Min(nextRadius, -q + nextRadius); r++)
                {
                    int s = -q - r;
                    if (Math.Abs(q) == nextRadius || Math.Abs(r) == nextRadius || Math.Abs(s) == nextRadius)
                    {
                        newHexs.Add(new HexBlockData { Q = q, R = r, S = s });
                    }
                }
            }

            return await Task.FromResult(newHexs);
        }

        public static bool CheckIsPathValid(List<Block> blocks)
        {
            if (blocks == null || blocks.Count < 2)
            {
                return true;
            }

            for (int i = 0; i < blocks.Count - 1; i++)
            {
                if (!AreNeighbors(blocks[i], blocks[i + 1]))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool AreNeighbors(Block a, Block b)
        {
            int distance = (Math.Abs(a.n_q - b.n_q) + Math.Abs(a.n_r - b.n_r) + Math.Abs(a.n_s - b.n_s)) / 2;
            return distance == 1;
        }
    }
}
