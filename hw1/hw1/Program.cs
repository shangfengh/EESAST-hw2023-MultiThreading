using System;
using System.Diagnostics;

namespace Homework
{
    public class Program
    {
        public static void Main(string[] args) 
        {
            ILongProgressByTime a = new LongProgressByTime();
            new Thread
                (
                    () =>
                    {
                        Console.WriteLine("A Start: "+(a.Start(2000)).ToString());
                        Thread.Sleep(1000);
                        Console.WriteLine("A TrySet0: "+(a.TrySet0()).ToString());
                        Thread.Sleep(500);
                        Console.WriteLine("A Start: "+(a.Start(1000)).ToString() +" Now: "+Environment.TickCount64);
                        Thread.Sleep(500);
                        Console.WriteLine("A Progress: "+(a.GetProgress()).ToString() +" Now: "+Environment.TickCount64);
                        Thread.Sleep(1003);
                        Console.WriteLine("A TrySet0: "+(a.TrySet0()).ToString());
                    }
                ).Start();

            new Thread
                (
                    () =>
                    {
                        Console.WriteLine("B Start: "+(a.Start(2000)).ToString());
                        Thread.Sleep(1500);
                        Console.WriteLine("B Start: " +(a.Start(1000)).ToString() + " Now: " + Environment.TickCount64);
                        Thread.Sleep(500);
                        Console.WriteLine("B Progress: " +(a.GetProgress()).ToString() + " Now: " + Environment.TickCount64);
                    }
                ).Start();
        }
    }

    public interface ILongProgressByTime
    {
        /// <summary>
        /// 尝试加载下一次进度条，needTime指再次加载进度条所需时间，单位毫秒
        /// 如果之前进度条已经终止，则将进度开始下一次加载，返回true
        /// 如果之前进度条尚未终止，返回false
        /// </summary>
        public bool Start(long needTime);

        /// <summary>
        /// 使未完成的进度条终止清零，返回值代表是否成功终止
        /// </summary>
        public bool TrySet0();

        /// <summary>
        /// 使进度条强制终止清零
        /// </summary>
        public void Set0();

        /// <summary>
        ///     ElapsedTime指其中已过去的时间，NeedTime指当前Progress完成所需时间，单位毫秒
        /// </summary>
        public (long ElapsedTime, long NeedTime) GetProgress();
    }

    public class LongProgressByTime: ILongProgressByTime
    {
        // 根据时间推算Start后完成多少进度的进度条（long）。
        // 只允许Start时修改needTime（确保较大）；
        // 支持TrySet0使未完成的进度条终止清零；

        // 只允许修改LongProgressByTime类中的代码
        // 要求实现ILongProgressByTime中的要求
        // 可利用Environment.TickCount64获取当前时间（单位ms）

        //挑战：利用原子操作
        //long.MaxValue非常久

        public long _isStart = 0;
        public long _needTime = 0;
        public long _startTime = 0;

        public void Check()
        {
            if (Interlocked.Read(ref _isStart) == 1)
            {
                long elapsedTime = Environment.TickCount64 - Interlocked.Read(ref _startTime);
                long needTime = Interlocked.Read(ref _needTime);
                if (elapsedTime >= needTime)
                {
                    Interlocked.Exchange(ref _isStart, 0);
                }
            }
        }

        public bool Start(long needTime)
        {
            Check();
            if (Interlocked.Read(ref _isStart) == 0)
            {
                Interlocked.Exchange(ref _needTime, needTime);
                Interlocked.Exchange(ref _isStart, 1);
                Interlocked.Exchange(ref _startTime, Environment.TickCount64);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool TrySet0()
        {
            Check();
            if (Interlocked.Read(ref _isStart) == 0)
            {
                return false;
            }
            else
            {
                Interlocked.Exchange(ref _isStart, 0);
                Interlocked.Exchange(ref _needTime, 0);
                Interlocked.Exchange(ref _startTime, 0);
                return true;
            }
        }

        public void Set0()
        {
            Check();
            Interlocked.Exchange(ref _isStart, 0);
            Interlocked.Exchange(ref _needTime, 0);
            Interlocked.Exchange(ref _startTime, 0);
        }

        public (long ElapsedTime, long NeedTime) GetProgress()
        {
            Check();
            long elapsedTime = Environment.TickCount64 - Interlocked.Read(ref _startTime);
            long needTime = Interlocked.Read(ref _needTime);
            return (elapsedTime, needTime);
        }
    }

/*输出示例：
 * A Start: False
B Start: True
A TrySet0: True
B Start: True Now: 14536562
A Start: False Now: 14536578
B Progress: (516, 1000) Now: 14537078
A Progress: (516, 1000) Now: 14537078
A TrySet0: False
*/
}