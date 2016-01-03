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
        const double MineRate = 0.2;

        static Random seed = new Random();

        private MineEntities context;

        private Stack<MineInfo> stack;
        private ArrayList changed_list;

        public static double randomDouble
        {
            get { return seed.NextDouble(); }
        }

        public MineGroupUtils()
        {
            context = new MineEntities();

            stack = new Stack<MineInfo>();
            changed_list = new ArrayList();
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

        public int getOffset(int m)
        {
            m = m % 20;
            if (m < 0) m += 20;
            return m;
        }

        public MineGroup insertIfNotExist(int group_x, int group_y)
        {
            if (group_x % 20 != 0 || group_y % 20 != 0)
            {
                Debug.WriteLine("trying get group: {0}, {1}", group_x, group_y);
                return null;
            }

            var mine = context.MineGroups.FirstOrDefault(m => m.x == group_x && m.y == group_y);

            if (null == mine)
            {
                mine = genMineGroup(group_x, group_y);
                context.MineGroups.Add(mine);
                context.SaveChanges();
            }

            return mine;
        }

        /// <summary>
        /// get data for given group, if not exists, insert default data into db
        /// </summary>
        /// <param name="group_x"></param>
        /// <param name="group_y"></param>
        public byte[] getOrInsert(int group_x, int group_y)
        {
            insertIfNotExist(group_x, group_y);

            return getBin(group_x, group_y);
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
                for (int _y = 0; _y < 20; _y++)
                {
                    for (int _x = 0; _x < 20; _x++)
                    {
                        res.Add(bytesToFlag(bs, _x, _y));
                    }
                }
            }
            return JsonConvert.SerializeObject(res);
        }

        public int open(int mine_x, int mine_y)
        {
            byte[] opened = { 1 };
            update(mine_x, mine_y, opened);
            return getFlag(mine_x, mine_y);
        }

        public void print(byte[] bs)
        {
            for (int i = 0; i < bs.Count(); i++)
            {
                Debug.Write(bs[i]);
            }
            Debug.WriteLine("");
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
            stack.Clear();
            stack.Push(new MineInfo(mine_x, mine_y));

            for (int i = 0; stack.Count() > 0; i++)
            {
                clickLeft(i);
            };

            ArrayList res = new ArrayList();
            foreach (MineInfo mine in changed_list)
            {
                calculateAndUpdateMineCount(mine.mine_x, mine.mine_x);
                if (!isMine(mine.mine_x, mine.mine_y) || (mine.mine_x == mine_x && mine.mine_y == mine_y))
                {
                    mine.val = open(mine.mine_x, mine.mine_y);
                    if (mine.val == 0)
                    {
                        Debug.WriteLine("{0}", JsonConvert.SerializeObject(mine));
                        print(getBytes(mine_x, mine_y));
                    }
                    res.Add(mine);
                }
            }
            Debug.WriteLine(JsonConvert.SerializeObject(res));

            changed_list.Clear();

            return JsonConvert.SerializeObject(res);
        }

        public int calculateAndUpdateMineCount(int mine_x, int mine_y)
        {
            if (!isDetermined(mine_x, mine_y)) return 0;
            if (isMine(mine_x, mine_y)) return 0;
            int res = 1; // bin start with 0001
            for (int _x = -1; _x <= 1; _x++)
            {
                for (int _y = 0; _y <= 1; _y++)
                {
                    if (0 == _x && 0 == _y) continue;
                    if (isMine(mine_x + _x, mine_y + _y)) { res += 1; }
                }
            }
            byte[] bs = { 1, 0, 0, 0, 0 };
            for (int i = 0; i < 4; i++)
            {
                bs[4 - i] = (byte)((res >> i) % 2);
            }
            update(mine_x, mine_y, bs);
            return res;
        }

        /// <summary>
        /// real logic implement for clickLeft(int, int)
        /// use this.stack
        /// </summary>
        public void clickLeft(int depth)
        {
            MineInfo todo = stack.Pop();

            int mine_offset_x = getOffset(todo.mine_x), mine_offset_y = getOffset(todo.mine_y);
            int group_x = todo.mine_x - mine_offset_x, group_y = todo.mine_y - mine_offset_y;

            int res = genMine(todo.mine_x, todo.mine_y, depth > 100);
            if (ALREADY_DETERMINED == res) { return; }
            changed_list.Add(todo);
            if (NOT_MINE == res)
            {
                for (int _x = -1; _x <= 1; _x++)
                {
                    for (int _y = -1; _y <= 1; _y++)
                    {
                        if (1 == _x && 1 == _y) { continue; }
                        stack.Push(new MineInfo(todo.mine_x + _x, todo.mine_y + _y));
                    }
                }
            }
        }

        /// <summary>
        /// perform right click on given position
        /// </summary>
        /// <param name="mine_x"></param>
        /// <param name="mine_y"></param>
        /// <returns>
        /// return changes that need to be done in front end
        /// </returns>
        public string clickRight(int mine_x, int mine_y)
        {
            Debug.WriteLine("clickRight called with: {0}, {1}", mine_x, mine_y);

            string res = JsonConvert.SerializeObject(ArrayList.Repeat(0, 0));
            if (isOpened(mine_x, mine_y))
            {
                Debug.WriteLine("mine: {0}, {1} is already opened", mine_x, mine_y);
                return res;
            }
            int status = genMine(mine_x, mine_y);
            MineInfo m = new MineInfo(mine_x, mine_y);
            if (MINE == status || isMine(mine_x, mine_y))
            {
                Debug.WriteLine("right click to: {0}, {1} is mine", mine_x, mine_y);
                m.val = -1;
            }
            else if (NOT_MINE == status)
            {
                Debug.WriteLine("right click to: {0}, {1} is not mine", mine_x, mine_y);
                m.val = 1;
            }
            res = JsonConvert.SerializeObject(ArrayList.Repeat(m, 1));

            return res;
        }

        /// <summary>
        /// update one mine with given bytes
        /// </summary>
        /// <param name="mine_x"></param>
        /// <param name="mine_y"></param>
        /// <param name="bs"></param>
        public void update(int mine_x, int mine_y, byte[] bs)
        {
            Debug.WriteLine("update: ({0}, {1}), {2}", mine_x, mine_y, bs);

            int offset_x = getOffset(mine_x), offset_y = getOffset(mine_y);
            int group_x = mine_x - offset_x, group_y = mine_y - offset_y;

            MineGroup mg = insertIfNotExist(group_x, group_y);

            for (int i = 0; i < 5 && i < bs.Count(); i++)
            {
                if (bs[i] <= 1)
                    mg.bin[offset_x + offset_y * 20 + i] = bs[i];
            }

            context.MineGroups.Attach(mg);
            context.Entry(mg).Property(m => m.bin).IsModified = true;
            context.SaveChanges();
        }

        const int ALREADY_DETERMINED = 0;
        const int MINE = 1;
        const int NOT_MINE = 2;
        /// <summary>
        /// generate a not decided mine
        /// </summary>
        /// <param name="mine_x"></param>
        /// <param name="mine_y"></param>
        /// <returns></returns>
        /// <seealso cref="ALREADY_DETERMINED"/>
        /// <seealso cref="MINE"/>
        /// <seealso cref="NOT_MINE"/>
        public int genMine(int mine_x, int mine_y, bool isMine = false)
        {
            int offset_x = getOffset(mine_x), offset_y = getOffset(mine_y);
            int group_x = mine_x - offset_x, group_y = mine_y - offset_y;
            var bs = getOrInsert(group_x, group_y);
            bs = bs.Skip(offset_x + offset_y * 20).Take(5).Cast<byte>().ToArray();
            if (isDetermined(bs))
            {
                Debug.WriteLine("mine {0}, {1} is already determined", mine_x, mine_y);
                return ALREADY_DETERMINED;
            }
            if (randomDouble < MineRate || isMine)
            {
                update(mine_x, mine_y, NOT_OPENED_MINE);
                return MINE;
            }
            else
            {
                update(mine_x, mine_y, NOT_DETERMINED_BLANK);
                return NOT_MINE;
            }
        }

        public int bytesToInt(byte[] bs)
        {
            int res = 0;
            for (int i = 1; i < bs.Count(); i++) // skip the first
            {
                res <<= 1;
                res += bs[i];
            }
            return res;
        }

        public byte[] getBytes(int mine_x, int mine_y)
        {
            int offset_x = getOffset(mine_x), offset_y = getOffset(mine_y);
            int group_x = mine_x - offset_x, group_y = mine_y - offset_y;
            var bs = getOrInsert(group_x, group_y);
            return bs.Skip(offset_x + offset_y * 20).Take(5).Cast<byte>().ToArray();
        }

        public bool isMine(byte[] bs)
        {
            if (1 == bs[1] && 1 == bs[3]) return true;
            return false;
        }


        public bool isMine(int mine_x, int mine_y)
        {
            return isMine(getBytes(mine_x, mine_y));
        }

        public bool isOpened(byte[] bs)
        {
            return 1 == bs[0];
        }

        public bool isOpened(int mine_x, int mine_y)
        {
            return isOpened(getBytes(mine_x, mine_y));
        }

        public bool isDetermined(byte[] bs)
        {
            for (int i = 0; i < 5; i++)
            {
                if (1 == bs[i]) return true;
            }
            return false;
        }

        public bool isDetermined(int mine_x, int mine_y)
        {
            return isDetermined(getBytes(mine_x, mine_y));
        }

        public readonly static byte[] NOT_OPENED_MINE = { 0, 1, 0, 1, 0 };
        public readonly static byte[] NOT_DETERMINED_BLANK = { 1, 0, 0, 0, 0 };
        /// <summary>
        /// convert bytes of one mine to its meaning
        /// </summary>
        /// <remarks>
        /// 
        /// 1 ~ 9 for non-mine, and for number of mines surronded: 0 ~ 8
        /// 0 for not-opened
        /// -1 for flagged
        /// -2 for exploded
        /// 
        /// x for if opened
        /// 00000 for not determined
        /// 0xxxx for not opened
        /// 10000 for not mine but number is still unknown
        /// x0001 - x1001 for number of mines around (0 - 8)
        /// x101x for mine
        /// 11010 for flagged
        /// 11011 for boom
        /// 
        /// </remarks>
        /// <param name="bs"></param>
        /// <returns></returns>
        public int bytesToFlag(byte[] bs)
        {
            if (0 == bs[0])
            {
                return 0;
            }
            int res = bytesToInt(bs);
            if (res < 10) { return res; }
            if (10 == res) { return -1; }
            if (11 == res) { return -2; }

            return 0;
        }

        public int getFlag(int mine_x, int mine_y)
        {
            return bytesToFlag(getBytes(mine_x, mine_y));
        }

        public int bytesToFlag(IEnumerable<byte> enumerable)
        {
            return bytesToFlag(enumerable.Cast<byte>().ToArray());
        }

        public int bytesToFlag(byte[] bs, int mine_x, int mine_y)
        {
            Debug.Assert(bs.Count() == 8000, "number of bytes is not correct.");

            mine_x %= 20;
            mine_y %= 20;

            return bytesToFlag(bs.Skip(mine_x * 20 + mine_y).Take(5));
        }

        public static MineGroup genMineGroup(int group_x, int group_y)
        {
            var m = new MineGroup();
            m.x = group_x;
            m.y = group_y;
            m.bin = new byte[8000];
            return m;
        }
    }

    public class MineInfo
    {
        public int mine_x;
        public int mine_y;

        /// <summary>
        /// current status
        /// </summary>
        public int val;

        public MineInfo(int mine_x, int mine_y)
        {
            this.mine_x = mine_x;
            this.mine_y = mine_y;
        }
    }
}
