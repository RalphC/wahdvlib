using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;
using WAHDV.structure;

namespace WAHDV.DAL
{
    public class WAHDataAccessLayer
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("WAHDV.dao");
        private MySqlConnection connection;
        private MySqlDataAdapter adapter;
        private MySqlCommand command;
        private MySqlCommandBuilder builder;
        private string connString;
        private string SQL_SELECT_ITEM = "SELECT * FROM auctiondb.TBL_Moonglade_AuctionItem";
        private string SQL_SELECT_TRANS = "SELECT * FROM auctiondb.TBL_Moonglade_Transaction";
        private string SQL_SELECT_LASTMOD = "SELECT LastModified FROM auctiondb.TBL_RealmLastModified WHERE RealmName = '";
        private string SQL_SELECT_CREATEDATE = "SELECT createTime FROM auctiondb.TBL_Moonglade_AuctionItem WHERE auc = ";

        public WAHDataAccessLayer()
        {
            connString = ConfigurationManager.ConnectionStrings["CurrentDB"].ConnectionString;
            connection = new MySqlConnection(connString);
        }

        public UInt64 GetLastModified(string RealmName)
        {
            command = new MySqlCommand(SQL_SELECT_LASTMOD + RealmName + "'", GetDBConnection());
            return Convert.ToUInt64(command.ExecuteScalar());
        }

        public DateTime GetCreateDateTime(UInt64 auc)
        {
            command = new MySqlCommand(SQL_SELECT_CREATEDATE + auc, GetDBConnection());
            return Convert.ToDateTime(command.ExecuteScalar());
        }

        public bool UpdateLastModified(string RealmName, UInt64 LastModified)
        {
            string SQL_UPDATE_REALM = "UPDATE auctiondb.TBL_RealmLastModified SET LastModified = " + LastModified + " , LastUpdate = '" + DateTime.Now.ToString() + "' WHERE RealmName = '" + RealmName + "'";
            command = new MySqlCommand(SQL_UPDATE_REALM, GetDBConnection());
            if (1 == command.ExecuteNonQuery()) return true;
            else return false;
        }

        public int SaveNewTransaction(ref DataTable NewTransaction)
        {
            command = new MySqlCommand(SQL_SELECT_TRANS, GetDBConnection());
            adapter = new MySqlDataAdapter();
            adapter.SelectCommand = command;
            builder = new MySqlCommandBuilder(adapter);
            adapter.InsertCommand = builder.GetInsertCommand();
            adapter.ContinueUpdateOnError = true;
            return adapter.Update(NewTransaction);
        }

        public int SaveNewItems(ref DataTable NewItemTBL)
        {
            command = new MySqlCommand(SQL_SELECT_ITEM, GetDBConnection());
            adapter = new MySqlDataAdapter();
            adapter.SelectCommand = command;
            builder = new MySqlCommandBuilder(adapter);
            adapter.InsertCommand = builder.GetInsertCommand();
            adapter.ContinueUpdateOnError = true;
            return adapter.Update(NewItemTBL);
        }

        //public bool SaveNewItem(auctionItem item)
        //{
        //    string strInsert = "INSERT INTO dbo.TBL_Moonglade_AuctionItem SET auc = " + item.auc + "";
        //    command = new MySqlCommand(strInsert, connection);
        //    if (command.ExecuteNonQuery() == 1) return true;
        //    else return false;
        //}

        public MySqlConnection GetDBConnection()
        {
            if (connection.State == ConnectionState.Closed)
            {
                try
                {
                    connection.Open();
                }
                catch (MySqlException sqlEx)
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
                catch (MySqlException sqlEx)
                {
                    log.Error(sqlEx.Message);
                }
            }
            connection.Dispose();
        }
        
    }
}
