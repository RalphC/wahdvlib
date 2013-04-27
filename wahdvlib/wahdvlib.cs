using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;

using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;

using log4net;

namespace wahdvlib
{
    public class wahdvlib
    {
        private static wahdvlib _instance = null;
        private UInt64 lngLastModified = 0;
        private string strRealm;
        private LogManager logger;

        protected wahdvlib(string strRealmName)
        {
            strRealm = strRealmName;
        }

        public static wahdvlib getInstance(string Realm)
        {
            if (_instance == null)
            {
                _instance = new wahdvlib(Realm);
            }
            return _instance;
        }

        public bool isModified()
        {
            HttpWebRequest rqstLastModified = (HttpWebRequest)WebRequest.Create("http://us.battle.net/api/wow/auction/data/" + strRealm);
            HttpWebResponse rspLastModified = (HttpWebResponse)rqstLastModified.GetResponse();
            if (rspLastModified.StatusCode == HttpStatusCode.OK)
            {
                try
                {
                    Stream streamResponse = rspLastModified.GetResponseStream();
                    StreamReader reader = new StreamReader(streamResponse, Encoding.GetEncoding(rspLastModified.ContentEncoding));
                    string strLastModified = reader.ReadToEnd();
                    realm CurrentRealm = JsonConvert.DeserializeObject<realm>(strLastModified);
                    if (CurrentRealm.LastModified != lngLastModified)
                    {
                        lngLastModified = CurrentRealm.LastModified;
                        return true;
                    }
                }
                catch (JsonException JsonEx)
                {
                	
                }
                return false;
            }
            else return false;
        }

    }


    public class realm
    {
        public UInt64 LastModified { get; set; }
        public string url { get; set; }
    }

}
