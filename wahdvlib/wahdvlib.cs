using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;

using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;

namespace wahdvlib
{
    public class wahdvlib
    {
        private static wahdvlib _instance = null;
        private long lngLastModified = 0;
        private string strRealm; 

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

        public bool checkModified()
        {
            HttpWebRequest rqstLastModified = (HttpWebRequest)WebRequest.Create("http://us.battle.net/api/wow/auction/data/" + strRealm);
            HttpWebResponse rspLastModified = (HttpWebResponse)rqstLastModified.GetResponse();
            if (rspLastModified.StatusCode == HttpStatusCode.OK)
            {
                Stream streamResponse = rspLastModified.GetResponseStream();
                StreamReader reader = new StreamReader(streamResponse, Encoding.GetEncoding(rspLastModified.ContentEncoding));
                string strLastModified = reader.ReadToEnd();
                
            }
            else return false;
        }

    }

}
