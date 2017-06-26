using System.Net.NetworkInformation;


namespace GYAsync
{
    /// <summary>
    /// Wrapping a `Reply` together with the Hostname / IP string
    /// </summary>
    public sealed class GYPingResult
    {
        /// <summary>
        /// The string value used for the ping operation
        /// </summary>
        public string Target { get; }
        /// <summary>
        /// The ping's reply
        /// </summary>
        public PingReply Reply { get; }


        /// <summary>
        /// Can be instantiated only within this assembly
        /// </summary>
        /// <param name="inReq">Hostname or IP</param>
        /// <param name="inRep">The obtained reply</param>
        internal GYPingResult(string inReq, PingReply inRep)
        {
            Target = inReq;
            Reply = inRep;
        }
    }
}
