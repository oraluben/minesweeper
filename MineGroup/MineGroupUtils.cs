using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MineGroup
{
    public class MineGroupUtils : IDisposable
    {
        public void Dispose()
        {
            return;
        }

        public byte[] getBin(int group_x, int group_y)
        {
            using (MineEntities context = new MineEntities())
            {
                var mine = context.MineGroups.FirstOrDefault(m => m.x == group_x && m.y == group_y);

                if (null == mine)
                    return null;
                else
                    return mine.bin;
            }
        }
    }
}
