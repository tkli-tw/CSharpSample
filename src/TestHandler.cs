using System;
using System.Net;

namespace Web {

//-----------------------------------------------------------------------------------
// 20170125 tkli
// 測試 Handler 架構的設計
//
// 這裡實作 handler
// 另外，必須在 主程式那邊 設定 路由的 path 
//-----------------------------------------------------------------------------------
class TestHandler : PathHandler
{
    public void handler(HttpListenerContext context){
        HttpListenerResponse response = context.Response;
        // Construct a response.
        string responseString = "<HTML><BODY> TestHandler!!!!</BODY></HTML>";
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