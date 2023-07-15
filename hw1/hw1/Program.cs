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
                        Console.WriteLine("A Start: " + (a.Start(2000)).ToString());
                        Thread.Sleep(1000);
                        Console.WriteLine("A TrySet0: " + (a.TrySet0()).ToString());
                        Thread.Sleep(500);
                        Console.WriteLine("A Start: " + (a.Start(1000)).ToString() + " Now: " + Environment.TickCount64);
                        Thread.Sleep(500);
                        Console.WriteLine("A Progress: " + (a.GetProgress()).ToString() + " Now: " + Environment.TickCount64);
                        Thread.Sleep(1003);
                        Console.WriteLine("A TrySet0: " + (a.TrySet0()).ToString());
                    }
                ).Start();

            new Thread
                (
                    () =>
                    {
                        Console.WriteLine("B Start: " + (a.Start(2000)).ToString());
                        Thread.Sleep(1500);
                        Console.WriteLine("B Start: " + (a.Start(1000)).ToString() + " Now: " + Environment.TickCount64);
                        Thread.Sleep(500);
                        Console.WriteLine("B Progress: " + (a.GetProgress()).ToString() + " Now: " + Environment.TickCount64);
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
    private long startTime;
    private long needTime;
    private readonly object locker = new object();

    public bool Start(long needTime)
    {
        lock (locker)
        {
            long currentTime = Environment.TickCount64;
            if (startTime + this.needTime > currentTime)
            {
                return false;
            }
            else
            {
                this.startTime = currentTime;
                this.needTime = needTime;
                return true;
            }
        }
    }

    public bool TrySet0()
    {
        lock (locker)
        {
            long currentTime = Environment.TickCount64;
            if (startTime + needTime > currentTime)
            {
                needTime = 0;
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public void Set0()
    {
        lock (locker)
        {
            startTime = Environment.TickCount64;
            needTime = 0;
        }
    }

    public (long ElapsedTime, long NeedTime) GetProgress()
    {
        lock (locker)
        {
            long currentTime = Environment.TickCount64;
            long elapsedTime = currentTime - startTime;

            if (elapsedTime > needTime)
            {
                elapsedTime = needTime;
            }

            return (elapsedTime, needTime);
        }
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