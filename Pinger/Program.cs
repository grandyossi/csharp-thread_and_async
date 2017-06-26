using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using GYAsync;


namespace GYConsole01
{
    class Program
    {
        /// <summary>
        /// Map of tasks responsible for ping results output
        /// </summary>
        private static Dictionary<int, Task> _map = null;



        /// <summary>
        /// Invoked on event : `PingCompleted`
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void _OnPingCompleted(
                                    object sender,
                                    PingCompletedEventArgs e)
        {
            if (e.Error != null)
                Debug.WriteLine(
                $"Error : {e.Error.InnerException.Message} - {e.UserState}");
            else
                Debug.WriteLine(
                $"Reply : {e.Reply.Address.ToString()} - {e.UserState} - {e.Reply.Status}");
        }


        //GUI and ASP.NET applications 
        //have a SynchronizationContext that permits only one chunk of code
        //to run at a time
        //https://msdn.microsoft.com/en-us/magazine/jj991977.aspx

        
        /// <summary>
        /// It is `async` all the way down:
        /// Outputs the taskID and its full result (including the ip string)
        /// </summary>
        /// <param name="id">Pinger's task id</param>
        /// <param name="inRes">A comprehensive result object</param>
        private static async void _setTaskToOutput(
                                            int inId,
                                            GYPingResult inRes)
        {
            //                                         null-conditional      null-coalescing
            Debug.WriteLine(
            $"Completed :   {inId} - {inRes.Target} - {inRes.Reply?.Status.ToString() ?? "Error"}");
            //
            Task t = _map[inId];
            _map.Remove(inId);
            await t;//until its status becomes `RanToCompletion`
            //https://stackoverflow.com/questions/3734280/is-it-considered-acceptable-to-not-call-dispose-on-a-tpl-task-object
            //no actual need:
            //t.Dispose();
        }


        /// <summary>
        /// This loop enables to eventually output
        /// BOTH the relevant string AND the ping result
        /// </summary>
        /// <returns>The last task to wait for (Notice: NO `return`)
        /// </returns>
        private static async Task _inputLoopAsync()
        {
            string aLine = "";
            Task tOut = null;
            while (true)
            {
                aLine = Console.ReadLine();//blocks
                if (aLine == null)
                    break;//<-> ctrl+Z+Enter
                Task<GYPingResult> tPing = GYPinger.PingAsync(aLine);
                tOut = Task.Run(
                        //`Action` via thread pool:
                        async () =>
                        {
                            //`await` is not blocking since via a thread:
                            _setTaskToOutput(tPing.Id, await tPing);
                        });
                //tOut.Id != tPing.Id
                //`tPing` is always being awaited, while tOut not yet...
                _map.Add(tPing.Id, tOut);//NOTICE: ID of inner pinging task
            }
            //notice: THE last pending output may have completed,
            //while others are still pending -
            //i.e: the last ping is successful, while others are not
            await tOut;//this method is `async`...
            //LINQ to array: wait all pendingz at `_setTaskToOutput`
            Task.WaitAll(_map.Values.ToArray());
        }


        private static void _inputLoopEvent()
        {
            var dlgt = new PingCompletedEventHandler(_OnPingCompleted);
            string aLine = null;
            while (true)
            {
                aLine = Console.ReadLine();//blocks
                if (aLine == null)
                    break;//<-> ctrl+Z+Enter
                GYPinger.PingWithEvent(aLine, dlgt);
            }
        }



        /// <summary>
        /// Designed to be initialized with one argument "async"
        /// when intention is to test its `Task`/async functionality
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Console.WriteLine(
                "Input IP address OR Hostname - Enter to ping:\n\n");
            if (args.Length == 1)
                if (args[0] == "async")
                {
                    _map = new Dictionary<int, Task>(10);
                    _inputLoopAsync().Wait();//OK, avoid aggregate exception: .GetAwaiter().GetResult();
                    return;
                }
            _inputLoopEvent();
        }
    }
}
