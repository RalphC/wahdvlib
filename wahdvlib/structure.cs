using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WAHDV.structure
{
    class dumpfile
    {
        public List<dumpInfo> files {get; set;}
    }

    class dumpInfo
    {
        public string url { get; set; }
        public UInt64 lastModified { get; set; }
    }



    class realmInfo
    {
        public string name { get; set; }
        public string slug { get; set; }
    }

    class auctionData
    {
        public realmInfo realm { get; set; }
        public auction alliance { get; set; }
        public auction horde { get; set; }
        public auction neutral { get; set; }
    }

    class auction
    {
        public List<auctionItem> auctions { get; set; }
    }

    class auctionItem
    {
        public UInt64 auc { get; set; }
        public UInt32 item { get; set; }
        public string owner { get; set; }
        public UInt64 bid { get; set; }
        public UInt64 buyout { get; set; }
        public UInt32 quantity { get; set; }
        public string timeLeft { get; set; }
        public Int32 rand { get; set; }
        public UInt64 seed { get; set; }
        public UInt32 petSpeciesId { get; set; }
        public UInt32 petBreedId { get; set; }
        public UInt16 petLevel { get; set; }
        public UInt16 petQualityId { get; set; }
        public bool inbid { get; set; }
    }
}
