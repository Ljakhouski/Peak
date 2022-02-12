using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace RuntimeEnvironment
{
    class Data { public string S = ""; int type = 0; public Data(int y) { S = y.ToString(); } public Data() { } }
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            var d = new Data[9999990];
            for (int i = 0; i < 9999990; i++)
                d[i] = new Data() { S = "beb" + i.ToString() };
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
            Console.WriteLine("RunTime " + elapsedTime);
            GC.Collect();
            Stopwatch stopWatch2 = new Stopwatch();
            stopWatch2.Start();
            var d2 = new Data[9999990];
            for (int i = 0; i < 9999990; i++)
            {
                d2[i] = new Data() { S = "beb" + i.ToString() +/*new Random().Next(255,34534)*/ new Data(i).S };

                /*if (d2[i].S.Length > 3)
                    d2[i].S += "43534fghgf534";
                else*/
                if (i > 3 && i % 3 == 4)
                    d2[i].S += "435345fghfg534";
                else if (i > 3 && i % 3 == 2)
                    d2[i].S += "43fghf4534534";
                else if (i > 3 && i % 3 == 8)
                    d2[i].S += "435345fghfgh534";
                else if (i > 3 && i - 3 == 9765)
                    d2[i].S += "43fghfghfgh34534";
                else if (i > 567 && i * 3 == 2567)
                    d2[i].S += "43fghf4534534";
                else if (i > 567 && i / 3 == 8567)
                    d2[i].S += "435345fghfgh534";
                else if (i > 567 && i * 33 == 9567)
                    d2[i].S += "43fghfghfgh34534";
                else if (i > 3 && i * 3 == 2567)
                    d2[i].S += "43fghf4534534";
                else if (i < 367)
                    d2[i].S += "435345fghfgh534";
                else if (i > 3 && i % 3 == 9)
                    d2[i].S += "43fghfghfgh34534";
            }

            stopWatch2.Stop();
            TimeSpan ts2 = stopWatch2.Elapsed;
            string elapsedTime2 = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts2.Hours, ts2.Minutes, ts2.Seconds,
            ts2.Milliseconds / 10);
            Console.WriteLine("RunTime " + elapsedTime2);
        }

    }
}

