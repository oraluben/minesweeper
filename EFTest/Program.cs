using MineGroup;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFTest
{
    class Program
    {
        static void Main(string[] args)
        {
            using (MineGroupUtils context = new MineGroupUtils())
            {
                byte[] bs = { 0, 1, 0, 1, 1 };
                int res = 0;
                for (int i = 1; i < 5; i++)
                {
                    res <<= 1;
                    res += bs[i];
                }
                Console.WriteLine(res);
            }
        }
    }
}
