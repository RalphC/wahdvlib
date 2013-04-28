using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Http;

using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;

using log4net;

namespace wahdvlib
{
    private class dumpInfo
    {
        public UInt64 LastModified { get; set; }
        public string url { get; set; }
    }

    private class auctionData
    {
        public realmInfo realm {get; set;}
        public auction alliance {get; set;}
        public auction horde {get; set;}
        public auction neutral {get; set;}
    }

    private class realmInfo
    {
        public string name {get; set;}
        public string slug {get; set;}
    }

    private class auction
    {
        public List<auctionItem> auctions {get; set;}
    }

    private class auctionItem
    {
        public UInt64 auc {get; set;}
        public UInt32 item {get; set;}
        public string owner {get; set;}
        public UInt64 bid {get; set;}
        public UInt64 buyout {get; set;}
        public UInt32 quality {get; set;}
        public string timeleft {get; set;}
        public Int32 rand {get; set;}
        public UInt64 seed {get; set;}
        public UInt32 petSpeciesId {get; set;}
        public UInt32 petBreedId {get; set;}
        public UInt16 petLevel {get; set;}
        public UInt16 petQualityId {get; set;}
    }

    public class wahdvlib
    {
        private static wahdvlib _instance = null;
        private auctionData currentData;
        private UInt64 lngLastModified = 0;
        private string strAuctionData = "";
        private string strRealm;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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
            HttpWebRequest rqstLastModified = (HttpWebRequest)WebRequest.Create("http://www.battlenet.com.cn/api/wow/auction/data/" + strRealm);
            HttpWebResponse rspLastModified = (HttpWebResponse)rqstLastModified.GetResponse();
            if (rspLastModified.StatusCode == HttpStatusCode.OK)
            {
                try
                {
                    Stream streamResponse = rspLastModified.GetResponseStream();
                    StreamReader reader = new StreamReader(streamResponse, Encoding.GetEncoding(rspLastModified.ContentEncoding));
                    string strLastModified = reader.ReadToEnd();
                    dumpInfo CurrentRealm = JsonConvert.DeserializeObject<dumpInfo>(strLastModified);
                    if (CurrentRealm.LastModified != lngLastModified)
                    {
                        lngLastModified = CurrentRealm.LastModified;
                        strAuctionData = CurrentRealm.url;
                        download(strAuctionData);
                        return true;
                    }
                }
                catch (JsonException JsonEx)
                {
                    log.Error(JsonEx.Message);
                }
                return false;
            }
            else return false;
        }

        private void download(string auctionDataUrl)
        {
            HttpClient downClient = new HttpClient();
            using (Stream dataStream = downClient.GetStreamAsync(auctionDataUrl).Result)
            {
                using (StreamReader streamReader = new StreamReader(dataStream))
                {
                    using (JsonReader jsReader = new JsonTextReader(streamReader))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        currentData = serializer.Deserialize<auctionData>(jsReader);
                    }
                }
            }
        }

    }




}
