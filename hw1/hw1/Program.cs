using System;
using System.Threading;
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
        private long _startTime;
        private long _needTime;
        private bool _isRunning;
        private readonly object lockObject = new object();
        private long elapsed;
        public LongProgressByTime()
        {
            _isRunning = false;
            _startTime = 0;
            _needTime = 0;
        }

        public bool Start(long needTime)
        {
            lock(lockObject)
            {
                if (!_isRunning)
                {
                    _startTime = Environment.TickCount64;
                    _needTime = needTime;
                    _isRunning = true;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool TrySet0()
        {
            lock (lockObject)
            {
                if (_isRunning && Environment.TickCount64 < _startTime + _needTime)
                {
                    _isRunning = false;
                    _startTime = 0;
                    _needTime = 0;
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
            lock(lockObject) 
            {
                _isRunning = false;
                _startTime = 0;
                _needTime = 0;
            }   
        }

        public (long ElapsedTime, long NeedTime) GetProgress()
        {
            lock(lockObject)
            {
                if (_isRunning)
                {
                    elapsed = Environment.TickCount64 - _startTime;
                    if (elapsed >= _needTime)
                    {
                        _isRunning = false;
                        return (_needTime, _needTime);
                    }
                    else
                    {
                        return (elapsed, _needTime);
                    }
                }
                else
                {
                    return (0, 0);
                }
            }
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