using System;
using System.Net;
using System.Collections.Generic;
using System.Data;

namespace Web {

//-----------------------------------------------------------------------------------
// 20170125 tkli
// 測試 Handler 架構的設計
//
// 這裡實作 handler
// 另外，必須在 主程式那邊 設定 路由的 path 
//-----------------------------------------------------------------------------------
class LoginVerify : PathHandler
{
    private bool dbAuth(string uid, string upass){
        
        //string sql = "select userid, username from users where userid = ? and pass=? ";
        string sql = "select userid, username from users where userid = '" +uid+ "' and pass= '" + upass +"'";
        //Console.WriteLine(sql);

        IDbConnection dbcon = Db.MysqlDB.getMysqlConn();
        
        DataTable table = Db.MysqlDB.GetDataTable(dbcon, sql);
        Console.WriteLine(table.Rows.Count);
        if (table.Rows.Count>0) return true;
        else return false;
    }
    
    public void handler(HttpListenerContext context){
        
        HttpListenerResponse response = context.Response;
        
        Dictionary<string, string> post = Utility.GetPostData(context);
        
        string uid = post["uid"];
        string upass = post["upass"];
        Console.WriteLine(uid);
        
        string responseString = "";
        if (dbAuth(uid, upass)){
            responseString = @"{""success"":""ok"", ""msg"":""none""}";
        } else {
            responseString = @"{""success"":""fail"", ""msg"":""帳密錯誤！！！""}";
            
        }
        
        // Construct a response.
        //string responseString = "<HTML><BODY> Hi, " + uid + "</BODY></HTML>";
        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
        // Get a response stream and write the response to it.
        response.ContentLength64 = buffer.Length;
        System.IO.Stream output = response.OutputStream;
        output.Write(buffer,0,buffer.Length);
        // You must close the output stream.
        output.Close();
        
    }

}

}