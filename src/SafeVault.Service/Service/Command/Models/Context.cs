using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using SafeVault.Exceptions;
using SafeVault.Logger;
using SafeVault.Service.Command.Exceptions;
using SafeVault.Transport;
using SafeVault.Transport.Models;
using ArgumentException = SafeVault.Exceptions.ArgumentException;

namespace SafeVault.Service.Command.Models
{
    public class Context : Hashtable
    {
        private static ILog Logger = Log.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public string UserDataPath
        {
            get { return Get<string>("UserDataPath"); }
            set { this["UserDataPath"] = value; }
        }

        public string Username 
        {
            get
            {
                return GetOrAdd<string>("Username", () =>
                {
                    var username = QueryParam("username");
                    if (username.Length > 32 || username != Regex.Replace(username, @"[^a-zA-Z0-9\-]", ""))
                        throw new ArgumentException("Bad Username");

                    return username;
                });
            }
        }

        public Userprofile Userprofile
        {
            get
            {
                return GetOrAdd<Userprofile>("Userprofile", () =>
                {
                    Userprofile userprofile = new Userprofile();
                    userprofile.Username = Username;

                    var path = $"{UserDataPath}/{Username}";

                    if (!Directory.Exists(path))
                        throw new ArgumentException("Unknown Username");

                    userprofile.Path = path;
                    userprofile.LoadProfile();
                    return userprofile;
                });
            }
        }
        public QueryMessage Query
        {
            get { return Get<QueryMessage>("QueryMessage"); }
            set { this["QueryMessage"] = value; }
        }
        public ServiceChannel Channel
        {
            get { return Get<ServiceChannel>("ServiceChannel"); }
            set { this["ServiceChannel"] = value; }
        }
        public IPAddress ClientIP
        {
            get { return Get<IPAddress>("ClientIP"); }
            set { this["ClientIP"] = value; }
        }

        public string QueryUUID
        {
            get
            {
                return GetOrAdd("QueryUUID", () => 
                {
                    var value = QueryParam("uuid");
                    if (value.Length > 64 || value != Regex.Replace(value, @"[^0-9a-zA-Z\-]", ""))
                        throw new ArgumentException("Bad UUID");

                    return value;
                });
            }
        }

        private T Get<T>(string key)
        {
            return GetOrAdd<T>(key, null);
        }

        private T GetOrAdd<T>(string key, Func<T> getvalueCallback = null)
        {
            if (HasKey(key))
                return (T) this[key];

            if (getvalueCallback == null)
                throw new ContextException($"context[\"{key}\"] is not defined.");

            this[key] = getvalueCallback();
            return (T)this[key];
        }


        public DateTime QueryDateTimeParam(string name, bool optional=false)
        {
            var value = QueryParam(name, optional);
            DateTime lastModified;
            if (DateTime.TryParse(value, out lastModified) == false)
                throw new BadRequestException($"Bad {name}");

            return lastModified.ToUniversalTime();
        }

        public string QueryParam(string name, bool optional=false)
        {
            if (!Query.Params.ContainsKey(name))
            {
                if (!optional)
                    throw new BadRequestException($"Argument not Found: {name}");

                return null;
            }

            //Logger.Info("QueryParam[{0}]={1}", name, Query.Params[name]);
            return Query.Params[name];
        }

        public bool HasKey(string name)
        {
            return ContainsKey(name);
        }
    }
}