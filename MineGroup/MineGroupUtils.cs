using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MineGroup
{
    /// <summary>
    /// MineGroupUtils is the API to control mine database.
    /// <para>With clickLeft and clickRight</para>
    /// <seealso cref="MineGroupUtils.clickLeft(int, int)"/>
    /// <seealso cref="MineGroupUtils.clickRight(int, int)"/>
    /// </summary>
    public class MineGroupUtils : IDisposable
    {
        private MineEntities context;

        public MineGroupUtils()
        {
            context = new MineEntities();
        }

        public void Dispose()
        {
            return;
        }

        /// <summary>
        /// get data for given group, if not exists, return default data
        /// </summary>
        /// <param name="group_x"></param>
        /// <param name="group_y"></param>
        /// <returns></returns>
        public byte[] getBin(int group_x, int group_y)
        {
            var mine = context.MineGroups.FirstOrDefault(m => m.x == group_x && m.y == group_y);

            if (null == mine)
                return null;
            else
                return mine.bin;
        }

        /// <summary>
        /// get data for given group, if not exists, insert default data into db
        /// </summary>
        /// <param name="group_x"></param>
        /// <param name="group_y"></param>
        public void getAndInsert(int group_x, int group_y)
        {
            return;
        }

        /// <summary>
        /// get init data for front end
        /// </summary>
        /// <param name="group_x"></param>
        /// <param name="group_y"></param>
        /// <returns>
        /// json data for front end init
        /// </returns>
        public string getInit(int group_x, int group_y)
        {
            byte[] bs = getBin(group_x, group_y);
            ArrayList res;
            if (null == bs)
            {
                res = ArrayList.Repeat(0, 400);
            }
            else
            {
                res = new ArrayList();
                for (int i = 0; i < 400; i++)
                {
                    res.Add(bytesToFlag(bs.Skip(i * 5).Take(5)));
                }
            }
            return JsonConvert.SerializeObject(res);
        }

        /// <summary>
        /// perform left click on given position
        /// </summary>
        /// <param name="mine_x"></param>
        /// <param name="mine_y"></param>
        /// <returns>
        /// return changes that need to be done in front end
        /// </returns>
        public string clickLeft(int mine_x, int mine_y)
        {
            return "";
        }

        /// <summary>
        /// convert bytes of one mine to its meaning
        /// </summary>
        /// <remarks>
        /// 1 ~ 9 for non-mine, and for number of mines surronded: 0 ~ 8
        /// 0 for not-opened
        /// -1 for flagged
        /// -2 for exploded
        /// </remarks>
        /// <param name="bs"></param>
        /// <returns></returns>
        public int bytesToFlag(byte[] bs)
        {
            return 0;
        }

        public int bytesToFlag(IEnumerable<byte> enumerable)
        {
            return bytesToFlag(enumerable.Cast<byte>().ToArray());
        }
    }
}
