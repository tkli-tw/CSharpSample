using System;
using System.Data;
using MySql.Data.MySqlClient;

namespace Web.Db {

//-----------------------------------------------------------------------------------
//20170126 tkli
//編譯範例：
// dmcs *.cs db/*.cs /out:WebServer.exe -r:System.Web -r:/home/ubuntu/workspace/c#/mysqlDB/v4.5/MySql.Data.dll -r:System.Data
//
// System.Data 需要 額外加 -r:System.Data
// Mysql.Data.dll 需要 把 dll 的路徑都寫進去，才抓得到，要注意的是 這邊 只是給 編譯器 看的，
// 執行時，是透過 mono，所以還要註冊給 mono ，程式 才能正常執行，註冊命令如下：
//  $ sudo gacutil /i MySql.Data.dll
// 
// 檔案請自官網下載，並解壓縮好：
//  https://dev.mysql.com/downloads/connector/net/
//
//-----------------------------------------------------------------------------------
class MysqlDB {

    //-----------------------------------------------------------------------------------
    // 20170126 tkli
    // 取得mysql 連線
    //-----------------------------------------------------------------------------------
    public static IDbConnection getMysqlConn(){
       string connectionString =
          "Server=localhost;" +
          "Database=test2;" +
          "User ID=tkli;" +
          "Password=;" +
          "Pooling=false";
       IDbConnection dbcon;
       dbcon = new MySqlConnection(connectionString);
       if (dbcon.State == ConnectionState.Open) dbcon.Close();
       dbcon.Open();
       
       return dbcon;
   
    }

    //-----------------------------------------------------------------------------------
    // 20170126 tkli
    // 傳入 sql ，取得 dataTable
    //-----------------------------------------------------------------------------------
    public static DataTable GetDataTable(IDbConnection dbcon, string sql)
    {
        MySqlCommand dbcmd = ((MySqlConnection)dbcon).CreateCommand();
        dbcmd.CommandText = sql;
        //dbcmd.CommandTimeout = 300;
        //Console.WriteLine(sql);

        MySqlDataAdapter adapter = new MySqlDataAdapter(dbcmd);
        DataSet ds = new DataSet();
        adapter.Fill(ds);
        return ds.Tables[0];
    }
     
    //-----------------------------------------------------------------------------------
    // 20170126 tkli
    // 傳入 sql ，執行命令
    //-----------------------------------------------------------------------------------
    public static void ExecSqlCmd(IDbConnection dbcon, string sql)
    {
        MySqlConnection conn = (MySqlConnection)dbcon;
        MySqlCommand cmd = new MySqlCommand(sql, conn);
        MySqlTransaction mySqlTransaction = conn.BeginTransaction();
        try
        {
            cmd.Transaction = mySqlTransaction;
            cmd.ExecuteNonQuery();
            mySqlTransaction.Commit();
        }
        catch (Exception ex)
        {
            mySqlTransaction.Rollback();
            throw (ex);
        }
    }    
}
    
}