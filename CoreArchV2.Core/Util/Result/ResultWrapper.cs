using System.Net;

namespace CoreArchV2.Core.Util.Result
{
    public class ResultWrapper
    {
        public bool Success { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public dynamic Data { get; set; }
    }
}