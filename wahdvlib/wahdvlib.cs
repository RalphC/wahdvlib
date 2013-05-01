using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Data;
using System.Net;
using System.Net.Http;

using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using log4net;

using WAHDV.structure;
using WAHDV.DAL;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]
namespace WAHDV
{
    public class wahdvlib
    {
        private static wahdvlib _instance = null;
        private auctionData currentData;
        //private UInt64 lngLastModified = 0;
        //private string strAuctionData = "";
        private string strRealm;

        private WAHDataAccessLayer wahdal;

        private Dictionary<UInt64, auctionItem> CurrentAllianceAuction;
        private Dictionary<UInt64, auctionItem> CurrentHordeAuction;
        private Dictionary<UInt64, auctionItem> CurrentNeutralAuction;

        private JsonSerializerSettings jsSettings;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("WAHDV");

        protected wahdvlib(string strRealmName)
        {
            strRealm = strRealmName;

            wahdal = new WAHDataAccessLayer();

            CurrentAllianceAuction = new Dictionary<UInt64, auctionItem>();
            CurrentHordeAuction = new Dictionary<UInt64, auctionItem>();
            CurrentNeutralAuction = new Dictionary<UInt64, auctionItem>();

            jsSettings = new JsonSerializerSettings();
            jsSettings.MissingMemberHandling = MissingMemberHandling.Ignore;
            jsSettings.NullValueHandling = NullValueHandling.Ignore;
        }

        public static wahdvlib getInstance(string Realm)
        {
            if (_instance == null)
            {
                _instance = new wahdvlib(Realm);
            }
            return _instance;
        }

        public void check()
        {
            HttpWebRequest rqstLastModified = (HttpWebRequest)WebRequest.Create("http://www.battlenet.com.cn/api/wow/auction/data/" + strRealm);
            HttpWebResponse rspLastModified = (HttpWebResponse)rqstLastModified.GetResponse();
            if (rspLastModified.StatusCode == HttpStatusCode.OK)
            {
                log.Info("get OK reply from " + rqstLastModified.RequestUri);
                Console.WriteLine("get OK reply from " + rqstLastModified.RequestUri);
                using (Stream streamResponse = rspLastModified.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(streamResponse, Encoding.GetEncoding("UTF-8")))
                    {
                        string strLastModified = reader.ReadToEnd();
                        try
                        {
                            dumpfile CurrentRealm = JsonConvert.DeserializeObject<dumpfile>(strLastModified, jsSettings);
                            if (CurrentRealm.files[0].url == "" || CurrentRealm.files[0].lastModified == 0)
                            {
                                log.Error("deserialize error");
                                Console.WriteLine("deserialize error");
                                return;
                            }

                            log.Debug("deserialized url=" + CurrentRealm.files[0].url);
                            log.Debug("deserialized lastModified=" + CurrentRealm.files[0].lastModified);
                            Console.WriteLine("deserialize result: url= " + CurrentRealm.files[0].url);
                            Console.WriteLine("deserialize result: lastModified= " + CurrentRealm.files[0].lastModified);

                            UInt64 preLastModified = wahdal.GetLastModified(strRealm);
                            if (CurrentRealm.files[0].lastModified != preLastModified)
                            {
                                log.Info("auction data modified, pre = " + preLastModified + ", curr = " + CurrentRealm.files[0].lastModified + ". start processing");
                                Console.WriteLine("auction data modified, pre = " + preLastModified + ", curr = " + CurrentRealm.files[0].lastModified + ". start processing");
                                wahdal.UpdateLastModified(strRealm, CurrentRealm.files[0].lastModified);
                                download(CurrentRealm.files[0].url);
                            }
                            else
                            {
                                log.Info("auction data signature unchanged, thread sleep");
                                Console.WriteLine("auction data signature unchanged, thread sleep");
                            }
                        }
                        catch (JsonException JsEx)
                        {
                            log.Error(JsEx.Message);
                            Console.WriteLine(JsEx.Message);
                        }
                        catch (Exception Ex)
                        {
                            log.Error(Ex.Message);
                            Console.WriteLine(Ex.Message);
                        }
                    }
                }
            }
            else
            {
                log.Warn("get " + rspLastModified.StatusCode.ToString() + " reply from " + rqstLastModified.RequestUri);
            }
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
                        log.Debug("data deserialized");
                        Console.WriteLine("data deserialized");

                        log.Info("alliance auction item = " + currentData.alliance.auctions.Count);
                        Console.WriteLine("alliance auction item = " + currentData.alliance.auctions.Count);
                        log.Info("horde auction item = " + currentData.horde.auctions.Count);
                        Console.WriteLine("horde auction item = " + currentData.horde.auctions.Count);
                        log.Info("neutral auction item = " + currentData.neutral.auctions.Count);
                        Console.WriteLine("neutral auction item = " + currentData.neutral.auctions.Count);

                        AllianceAuctionUpdate(currentData.alliance.auctions);
                        HordeAuctionUpdate(currentData.horde.auctions);
                        NeutralAuctionUpdate(currentData.neutral.auctions);
                    }
                }
            }
        }

        private void AllianceAuctionUpdate(List<auctionItem> allianceList)
        {
            Dictionary<UInt64, auctionItem> alliance = ConvertList(allianceList);
            log.Info("start alliance auction data processing");
            Console.WriteLine("start alliance auction data processing");

            int newItem = AddNewItem(CurrentAllianceAuction, ref allianceList, "alliance");
            log.Info("new alliance auction item = " + newItem);
            Console.WriteLine("new alliance auction item = " + newItem);

            int newTrans = AddNewTransaction(CurrentAllianceAuction, alliance, "alliance");
            log.Info("new alliance transaction = " + newTrans);
            Console.WriteLine("new alliance transaction = " + newTrans);

            CurrentAllianceAuction = alliance;
        }

        private void HordeAuctionUpdate(List<auctionItem> hordeList)
        {
            Dictionary<UInt64, auctionItem> horde = ConvertList(hordeList);
            log.Info("start horde auction data processing");
            Console.WriteLine("start horde auction data processing");

            int newItem = AddNewItem(CurrentHordeAuction, ref hordeList, "horde");
            log.Info("new horde auction item = " + newItem);
            Console.WriteLine("new horde auction item = " + newItem);

            int newTrans = AddNewTransaction(CurrentHordeAuction, horde, "horde");
            log.Info("new horde transaction = " + newTrans);
            Console.WriteLine("new horde transaction = " + newTrans);

            CurrentHordeAuction = horde;
        }

        private void NeutralAuctionUpdate(List<auctionItem> neutralList)
        {
            Dictionary<UInt64, auctionItem> neutral = ConvertList(neutralList);
            log.Info("start neutral auction data processing");
            Console.WriteLine("start neutral auction data processing");

            int newItem = AddNewItem(CurrentNeutralAuction, ref neutralList, "neutral");
            log.Info("new neutral auction item = " + newItem);
            Console.WriteLine("new neutral auction item = " + newItem);

            int newTrans = AddNewTransaction(CurrentNeutralAuction, neutral, "neutral");
            log.Info("new neutral transaction = " + newTrans);
            Console.WriteLine("new neutral transaction = " + newTrans);

            CurrentNeutralAuction = neutral;
        }

        private Dictionary<UInt64, auctionItem> ConvertList(List<auctionItem> ItemList)
        {
            Dictionary<UInt64, auctionItem> Items = new Dictionary<UInt64, auctionItem>();
            foreach (auctionItem item in ItemList)
            {
                Items.Add(item.auc, item);
            }
            return Items;
        }

        private int AddNewItem(Dictionary<UInt64, auctionItem> CurrentAuction, ref List<auctionItem> NewAuctionList, string AuctionHouse)
        {
            DataTable ToAddItemTBL = new DataTable();
            auctionItem tmpItem;
            FormatItemTable(ref ToAddItemTBL);
            foreach (auctionItem item in NewAuctionList)
            {
                if ( !CurrentAuction.ContainsKey(item.auc))
                {
                    DataRow dr = ToAddItemTBL.NewRow();
                    AuctionItem2Row(item, AuctionHouse, ref dr);
                    ToAddItemTBL.Rows.Add(dr);
                }
                else
                {
                    if (CurrentAuction.TryGetValue(item.auc, out tmpItem))
                    {
                        if (item.bid > tmpItem.bid)
                        {
                            item.inbid = true;
                        }
                    }
                }
            }
            return wahdal.SaveNewItem(ref ToAddItemTBL);
        }

        private int AddNewTransaction(Dictionary<UInt64, auctionItem> PreAuction, Dictionary<UInt64, auctionItem> CurrentAuction, string AuctionHouse)
        {
            DataTable ToAddTransTBL = new DataTable();
            FormatAuctionTable(ref ToAddTransTBL);
            foreach (var auc in PreAuction)
            {
                if ( !CurrentAuction.ContainsKey(auc.Key))
                {
                    if (auc.Value.timeLeft != "SHORT" || auc.Value.inbid == true)
                    {
                        DataRow dr = ToAddTransTBL.NewRow();
                        TransactionItem2Row(auc.Value, AuctionHouse, ref dr);
                        ToAddTransTBL.Rows.Add(dr);
                    }
                }
            }
            return wahdal.SaveNewTransaction(ref ToAddTransTBL);
        }

        private void AuctionItem2Row(auctionItem item, string AuctionHouse, ref DataRow dr)
        {
            dr["auc"] = item.auc;
            dr["auctionHouse"] = AuctionHouse;
            dr["item"] = item.item;
            dr["owner"] = item.owner;
            dr["bid"] = item.bid;
            dr["buyout"] = item.buyout;
            dr["quantity"] = item.quantity;
            dr["timeLeft"] = item.timeLeft;
            dr["rand"] = item.rand;
            dr["seed"] = item.seed;
            dr["petSpeciesId"] = item.petSpeciesId;
            dr["petBreedId"] = item.petBreedId;
            dr["petLevel"] = item.petLevel;
            dr["petQualityId"] = item.petQualityId;
            dr["createTime"] = DateTime.Now;
            dr["lastUpdate"] = DateTime.Now;
        }

        private void TransactionItem2Row(auctionItem item, string AuctionHouse, ref DataRow dr)
        {
            dr["auc"] = item.auc;
            dr["auctionHouse"] = AuctionHouse;
            dr["item"] = item.item;
            dr["owner"] = item.owner;
            dr["bid"] = item.bid;
            dr["buyout"] = item.buyout;
            dr["quantity"] = item.quantity;
            dr["rand"] = item.rand;
            dr["seed"] = item.seed;
            dr["petSpeciesId"] = item.petSpeciesId;
            dr["petBreedId"] = item.petBreedId;
            dr["petLevel"] = item.petLevel;
            dr["petQualityId"] = item.petQualityId;
            dr["buyoutTime"] = DateTime.Now;
            dr["createTime"] = wahdal.GetCreateDateTime(item.auc);
            dr["inbid"] = item.inbid;
        }

        private void FormatItemTable(ref DataTable dt)
        {
            dt.Columns.Add("auc", typeof(UInt64));
            dt.Columns.Add("auctionHouse", typeof(string));
            dt.Columns.Add("item", typeof(UInt32));
            dt.Columns.Add("owner", typeof(string));
            dt.Columns.Add("bid", typeof(UInt64));
            dt.Columns.Add("buyout", typeof(UInt64));
            dt.Columns.Add("quantity", typeof(UInt32));
            dt.Columns.Add("timeLeft", typeof(string));
            dt.Columns.Add("rand", typeof(Int32));
            dt.Columns.Add("seed", typeof(UInt64));
            dt.Columns.Add("petSpeciesId", typeof(UInt32));
            dt.Columns.Add("petBreedId", typeof(UInt32));
            dt.Columns.Add("petLevel", typeof(UInt16));
            dt.Columns.Add("petQualityId", typeof(UInt16));
            dt.Columns.Add("createTime", typeof(DateTime));
            dt.Columns.Add("lastUpdate", typeof(DateTime));
        }

        private void FormatAuctionTable(ref DataTable dt)
        {
            dt.Columns.Add("auc", typeof(UInt64));
            dt.Columns.Add("auctionHouse", typeof(string));
            dt.Columns.Add("item", typeof(UInt32));
            dt.Columns.Add("owner", typeof(string));
            dt.Columns.Add("bid", typeof(UInt64));
            dt.Columns.Add("buyout", typeof(UInt64));
            dt.Columns.Add("quantity", typeof(UInt32));
            dt.Columns.Add("rand", typeof(Int32));
            dt.Columns.Add("seed", typeof(UInt64));
            dt.Columns.Add("petSpeciesId", typeof(UInt32));
            dt.Columns.Add("petBreedId", typeof(UInt32));
            dt.Columns.Add("petLevel", typeof(UInt16));
            dt.Columns.Add("petQualityId", typeof(UInt16));
            dt.Columns.Add("buyoutTime", typeof(DateTime));
            dt.Columns.Add("createTime", typeof(DateTime));
            dt.Columns.Add("inbid", typeof(bool));
        }
    }




}
