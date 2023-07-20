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
    
    public class LongProgressByTime : ILongProgressByTime
    {
        private long startTime;
        private long needTime;
        private long elapsed;
        private bool isRunning;
        private readonly object lockObject = new object();
        public bool Start(long needTime)
        {
            lock (lockObject)
            {
                if (isRunning)
                {
                    return false;
                }

                this.needTime = needTime;
                startTime = Environment.TickCount64;
                elapsed = 0;
                isRunning = true;

                return true;
            }
        }
        public bool TrySet0()
        {
            lock (lockObject)
            {
                if (!isRunning)
                {
                    return false;
                }

                elapsed = 0;
                isRunning = false;

                return true;
            }
        }
        public void Set0()
        {
            lock (lockObject)
            {
                elapsed = 0;
                isRunning = false;
            }
        }
        public (long ElapsedTime, long NeedTime) GetProgress()
        {
            lock (lockObject)
            {
                return (elapsed, needTime);
            }
        }
        public LongProgressByTime()
        {
            var t = new Thread(()=>
            {
                while (isRunning)
                {
                    Thread.Sleep(1);
                    lock (lockObject)
                    {
                        if (!isRunning)
                        {
                            break;
                        }
                        elapsed = Environment.TickCount64 - startTime;
                        if (elapsed >= needTime)
                        {
                            elapsed = needTime;
                            isRunning = false;
                        }
                    }
                }
            }
            );
            t.IsBackground = true;
            t.Start();
        }
    }

    /*输出示例：
    A Start: False
    B Start: True
    A TrySet0: True
    B Start: True Now: 14536562
    A Start: False Now: 14536578
    B Progress: (516, 1000) Now: 14537078
    A Progress: (516, 1000) Now: 14537078
    A TrySet0: False
    */
}
