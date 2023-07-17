﻿using System;
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
        /// 如果之前进度条处于就绪态，则将进度开始下一次加载，返回true
        /// 如果之前进度条不处于就绪态，返回false
        /// </summary>
        public bool Start(long needTime);

        /// <summary>
        /// 使未完成的进度条清零并终止变为就绪态，返回值代表是否成功终止
        /// </summary>
        public bool TrySet0();

        /// <summary>
        /// 使进度条强制清零并终止变为就绪态
        /// </summary>
        public void Set0();

        /// <summary>
        ///     ElapsedTime指其中已过去的时间，NeedTime指当前Progress完成所需时间，单位毫秒
        /// </summary>
        public (long ElapsedTime, long NeedTime) GetProgress();
    }

    public class LongProgressByTime : ILongProgressByTime
    {
        // 根据时间推算Start后完成多少进度的进度条（long）。
        public long startTime;//从0开始加载的时间
        public long needTime;//可能需要记一下总时间
        public long elapsedTime;//已经经过的时间,当进度条使
        public bool isReady;//是否处于就绪态
        public LongProgressByTime()//构造函数
        {
            startTime = Environment.TickCount64;
            isReady = true;
        }
        // 只允许修改LongProgressByTime类中的代码
        // 要求实现ILongProgressByTime中的要求
        // 可利用Environment.TickCount64获取当前时间（单位ms）

        //挑战：利用原子操作
        //long.MaxValue非常久
        public (long ElapsedTime, long NeedTime) GetProgress()
        {
            return (Environment.TickCount64-startTime,needTime);
        }

        public void Set0()
        {
            elapsedTime = 0;
            startTime = Environment.TickCount64;
            isReady = true;
        }

        public bool Start(long NeedTime)
        {
            if(isReady)
            {
                needTime = NeedTime;
                isReady = false;
                return true;
            }
            else
            {
                needTime = NeedTime;
                return false;
            }
        }

        public bool TrySet0()
        {
            if(Environment.TickCount64-startTime>=needTime) //完成了,不动它
            {
                return false;
            }
            else
            {
                startTime = Environment.TickCount64;//重置时间
                elapsedTime = 0;
                isReady = true;
                return true;
            }
        }
    }

    /*输出示例（仅供参考）：
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