using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace SafeVault.Transport.Models
{
    public class QueryMessage
    {
        public string Command { get; set; }
        public string XsfrToken { get; set; }

        public Dictionary<string, string> Params { get; set; }

        public QueryMessage()
        {
            Params = new Dictionary<string, string>();
            XsfrToken = Guid.NewGuid().ToString();
        }
    }
}