using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;
using System.Data.SqlClient;
using WAHDV.structure;

namespace WAHDV.DAL
{
    public class WAHDataAccessLayer
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("WAHDV.dao");
        private SqlConnection connection;
        private SqlDataAdapter adapter;
        private SqlCommand command;
        private SqlCommandBuilder builder;
        private string connString = "Server=WORKSTATION;Database=AuctionDataDB;User Id=WAHDV_AP;Password=WAHDV_AP;";
        private string SQL_SELECT_ITEM = "SELECT * FROM dbo.TBL_Moonglade_AuctionItem";
        private string SQL_SELECT_TRANS = "SELECT * FROM dbo.TBL_Moonglade_Transaction";
        private string SQL_SELECT_LASTMOD = "SELECT LastModified FROM dbo.TBL_RealmLastModified WHERE RealmName = '";
        private string SQL_SELECT_CREATEDATE = "SELECT createTime FROM dbo.TBL_Moonglade_AuctionItem WHERE auc = ";

        public WAHDataAccessLayer()
        {
            connection = new SqlConnection(connString);
        }

        public UInt64 GetLastModified(string RealmName)
        {
            command = new SqlCommand(SQL_SELECT_LASTMOD + RealmName + "'", GetDBConnection());
            return Convert.ToUInt64(command.ExecuteScalar());
        }

        public DateTime GetCreateDateTime(UInt64 auc)
        {
            command = new SqlCommand(SQL_SELECT_CREATEDATE + auc, GetDBConnection());
            return Convert.ToDateTime(command.ExecuteScalar());
        }

        public bool UpdateLastModified(string RealmName, UInt64 LastModified)
        {
            string SQL_UPDATE_REALM = "UPDATE dbo.TBL_RealmLastModified SET LastModified = " + LastModified + " , LastUpdate = '" + DateTime.Now.ToString() + "' WHERE RealmName = '" + RealmName + "'";
            command = new SqlCommand(SQL_UPDATE_REALM, GetDBConnection());
            if (1 == command.ExecuteNonQuery()) return true;
            else return false;
        }

        public int SaveNewTransaction(ref DataTable NewTransaction)
        {
            command = new SqlCommand(SQL_SELECT_TRANS, GetDBConnection());
            adapter = new SqlDataAdapter();
            adapter.SelectCommand = command;
            builder = new SqlCommandBuilder(adapter);
            adapter.InsertCommand = builder.GetInsertCommand();
            adapter.ContinueUpdateOnError = true;
            return adapter.Update(NewTransaction);
        }

        public int SaveNewItems(ref DataTable NewItemTBL)
        {
            command = new SqlCommand(SQL_SELECT_ITEM, GetDBConnection());
            adapter = new SqlDataAdapter();
            adapter.SelectCommand = command;
            builder = new SqlCommandBuilder(adapter);
            adapter.InsertCommand = builder.GetInsertCommand();
            adapter.ContinueUpdateOnError = true;
            return adapter.Update(NewItemTBL);
        }

        public bool SaveNewItem(auctionItem item)
        {
            string strInsert = "INSERT INTO dbo.TBL_Moonglade_AuctionItem SET auc = " + item.auc + "";
            command = new SqlCommand(strInsert, connection);
            if (command.ExecuteNonQuery() == 1) return true;
            else return false;
        }

        public SqlConnection GetDBConnection()
        {
            if (connection.State == ConnectionState.Closed)
            {
                try
                {
                    connection.Open();
                }
                catch (SqlException sqlEx)
                {
                    log.Error(sqlEx.Message);
                }
                
            }
            return connection;
        }

        private void CloseDBConnection()
        {
            if (connection.State == ConnectionState.Open)
            {
                try
                {
                    connection.Close();
                }
                catch (SqlException sqlEx)
                {
                    log.Error(sqlEx.Message);
                }
            }
            connection.Dispose();
        }
        
    }
}
