using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WAHDV.DAL;

namespace WAHDVUnitTest
{
    [TestClass]
    public class DALUnitTest
    {
        [TestMethod]
        public void TestDBConnection()
        {
            WAHDataAccessLayer dal = new WAHDataAccessLayer();
            System.Data.SqlClient.SqlConnection conn = dal.GetDBConnection();
            Assert.IsTrue(conn.State == System.Data.ConnectionState.Open);
        }

        [TestMethod]
        public void TestGetLastModified()
        {
            WAHDataAccessLayer dal = new WAHDataAccessLayer();
            UInt64 lastModified = (UInt64)dal.GetLastModified("Moonglade");
            Assert.AreEqual(lastModified, (UInt64)1367302826000);
        }
    }
}
