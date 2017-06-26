using System;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;


namespace GYAsync
{
    /// <summary>
    /// Pinging result is delivered via two different approaches: 
    /// Event, or Async
    /// </summary>
    public static class GYPinger
    {
        /// <summary>
        /// Relatively big, just for increasing await time
        /// </summary>
        public static readonly int GY_PINGER_DEFAULT_TIMEOUT = 8000;
        /// <summary>
        /// The data can go through 180 gateways/routers
        /// before it is destroyed.
        /// The data packet cannot be fragmented.
        /// </summary>
        private static PingOptions _opt { get; } =
                                        new PingOptions(180, true);
        /// <summary>
        /// 32 Bytes buffer
        /// </summary>
        private static byte[] _buff { get; } =
            Encoding.ASCII.GetBytes("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");



        /// <summary>
        /// Ping via `Task`
        /// </summary>
        /// <param name="inTgt">Hostname or IP. Not verified</param>
        /// <returns>Task</returns>
        public static async Task<GYPingResult> PingAsync(string inTgt)
        {
            Task<PingReply> aPromise;
            lock (_buff)
            {
                //"...
                //cannot use the same instance of the Ping class
                //to generate multiple simultaneous ICMP Echo requests"
                //TO_DO *0* : IDisposable ping
                Ping doer = new Ping();
                aPromise = doer.SendPingAsync(
                                    inTgt,
                                    GY_PINGER_DEFAULT_TIMEOUT,
                                    _buff,
                                    _opt);
            }//lock, safe of exceptions since no task is executed
            try
            {
                //here is where the task is executed:
                return new GYPingResult(inTgt, await aPromise);
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Ping was aborted :   {e.Message}");
            }
            return new GYPingResult(inTgt, null);
        }


        /// <summary>
        /// Event is raised on ping result
        /// </summary>
        /// <param name="inTgt">Hostname or IP. Not verified</param>
        /// <param name="inH">Do NOT invoke with null</param>
        /// <exception cref="ArgumentException"></exception>
        public static void PingWithEvent(
                                string inTgt,
                                PingCompletedEventHandler inH)
        {
            if (inH == null)
                throw new ArgumentException("null event handler");
                //exception <-> not setting empty delegate
                //https://stackoverflow.com/questions/4303343/is-it-a-good-practice-to-define-an-empty-delegate-body-for-a-event
            lock (_buff)
            {
                //... cannot use the same instance of the Ping class...
                Ping doer = new Ping();//doers do
                doer.PingCompleted += inH;
                Task.Run(() =>
                {
                    //https://msdn.microsoft.com/en-us/library/system.net.networkinformation.ping(v=vs.110).aspx
                    //NOTICE:
                    //`UserState` token is set with `AutoResetEvent`
                    //Here (see last arg),
                    //the `UserState` is set with the supplied string
                    doer.SendAsync(
                                inTgt,
                                GY_PINGER_DEFAULT_TIMEOUT,
                                _buff,
                                _opt,
                                inTgt);
                });
            }//lock
        }
    }
}
