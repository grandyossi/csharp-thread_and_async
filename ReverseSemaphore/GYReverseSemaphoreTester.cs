using System;
using static System.Diagnostics.Debug;
using System.Threading;


namespace GYThreading.Testers
{
    public sealed class GYReverseSemaphoreTester
    {
        private GYReverseSemaphore _sync { get; }

        /// <summary>
        /// 
        /// </summary>
        public event Action Done = delegate { };


        /// <summary>
        /// CTOR
        /// </summary>
        /// <param name="threshold">for GYReverseSemaphore</param>
        /// <exception cref="System.ArgumentException"></exception>
        /// <see cref="GYLib1.GYReverseSemaphore.GYReverseSemaphore(int)"/>
        public GYReverseSemaphoreTester(int threshold)
        {
            _sync = new GYReverseSemaphore(threshold);
        }


        /// <summary>
        /// Executes a test
        /// </summary>
        public void Execute()
        {
            int count = _sync.Threshold;
            Random gen = new Random();//random zleep time ms
            Thread[] t = new Thread[_sync.Threshold];
            for (int i = 0; i < count; ++i)
            {
                t[i] = new Thread(delegate()
                {
                    _threadProc(gen.Next(100, 4000), _sync);
                });
                t[i].Name = $"{i + 1}";
                t[i].Start();
            }
            foreach (Thread th in t)
                th.Join();
            Done.Invoke();
        }


        /// <summary>
        /// Thread proc, which is supplied with the relevant wait object
        /// </summary>
        /// <param name="zleep">Test purpose sleep in ms</param>
        /// <param name="deSync">Sync/Wait object</param>
        private static void _threadProc(int zleep, GYReverseSemaphore deSync)
        {
            string n = Thread.CurrentThread.Name;
            Write($"{DateTime.Now} - {n} - zzzleeps ({zleep}ms)\n");
            Thread.Sleep(zleep);
            Write($"{DateTime.Now} - {n} - waits for threshold\n");
            //
            //
            deSync.WaitAll();
            //
            //
            Write($"{DateTime.Now} - {n} - Released!\n");
        }
    }
}
