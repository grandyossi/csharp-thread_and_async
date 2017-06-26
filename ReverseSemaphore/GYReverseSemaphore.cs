using System;
using System.Threading;

[assembly: CLSCompliant(true)]


namespace GYThreading
{
    /// <summary>
    /// Blocks until wating threads count reaches the desired threshold
    /// </summary>
    [CLSCompliant(true)]
    public sealed class GYReverseSemaphore
    {
        /// <summary>
        /// The desired amount of working threads
        /// </summary>
        public int Threshold { get; }
        /// <summary>
        /// Counts waiting threads
        /// </summary>
        private int _countThreads = 0;
        /// <summary>
        /// Once signaled (true) it allows all waiting threads to pass
        /// </summary>
        private ManualResetEvent _releaser { get; } = new ManualResetEvent(false);


        /// <summary>
        /// CTOR.
        /// default/parameterless will not be created
        /// </summary>
        /// <param name="threshold">Threads count threshold.
        /// signed int, to make it CLS-Compliant</param>
        /// <exception cref="System.ArgumentException"></exception>
        public GYReverseSemaphore(int threshold)
        {
            if (0 >= threshold)
                throw new ArgumentException("Threshold must be bigger than zero");
            Threshold = threshold;
        }
        /// <summary>
        /// Finalizer
        /// </summary>
        ~GYReverseSemaphore()
        {
            _releaser.Dispose();
        }



        public void WaitAll()
        {
            Interlocked.Increment(ref _countThreads);
            //lock (_releaser)
            //{
            //reading `_countThreads` is atomic
            if (Threshold == _countThreads)
                _releaser.Set();//THE last thread will trigger...
            //}//not a must...
            _releaser.WaitOne();
            Interlocked.Decrement(ref _countThreads);//on each passing thread
            if (0 == _countThreads)
                _releaser.Reset();
        }
    }
}
