using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;

namespace SafeVault.Transport.Models
{
    public class ResponseMessage
    {
        public string StatusText { get; set; }
        public int StatusCode { get; set; }
        public DateTime Timestamp { get; set; }

        public Dictionary<string, string> Header { get; set; }

        public ResponseMessage()
        {
            Header = new Dictionary<string, string>();
            StatusCode = 200;
            Timestamp = DateTime.UtcNow;
        }
    }
}