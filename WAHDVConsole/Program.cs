using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace WAHDV
{
    class Program
    {
        private static wahdvlib wahlib;
        static void Main(string[] args)
        {
            wahlib = wahdvlib.getInstance("Moonglade");
            Thread ServerRun = new Thread(run);
            ServerRun.Start();
        }

        static void run()
        {
            while (true)
            {
                wahlib.check();
                Thread.Sleep(1000 * 1800);
            }
        }
    }
}
