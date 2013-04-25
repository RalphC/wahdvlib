using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;

using Newtonsoft.Json;

namespace wahdvlib
{
    public class wahdvlib
    {
        private static wahdvlib _instance = null;
        //protected static bool isInitialized = false;

        protected wahdvlib()
        {

        }

        public static wahdvlib getInstance()
        {
            if (_instance == null)
            {
                _instance = new wahdvlib();
            }
            return _instance;
        }

    }


    public class downloader
    {
        HttpWebRequest request;
        HttpWebResponse response;
        Stream requestStream;
        Stream responseStream;

        public void run()
        {
            
        }
    } 
}
