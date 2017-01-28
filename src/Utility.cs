using System;
using System.Collections.Generic;
using System.Net;
using System.Web;
using System.IO;

namespace Web{
    
    class Utility{
        
        //20170125 tkli
        //取得 Post 的資料
        //參考：
        //http://stackoverflow.com/questions/19031438/parse-post-parameters-from-httplistener
        //http://stackoverflow.com/questions/8637856/httplistener-with-post-data
        //
        //編譯時 需加 參考，如 ：
        // mcs -r:System.Web *.cs /out:WebServer.exe
        public static Dictionary<string, string> GetPostData(HttpListenerContext context){
            
            //var context = listener.GetContext();
            var request = context.Request;
            string rawData;
            using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
            {
                rawData = reader.ReadToEnd();
            }            
            
            //using System.Web and Add a Reference to System.Web
            Dictionary<string, string> postParams = new Dictionary<string, string>();
            string[] rawParams = rawData.Split('&');
            foreach (string param in rawParams)
            {
                string[] kvPair = param.Split('=');
                string key = kvPair[0];
                string value = System.Web.HttpUtility.UrlDecode(kvPair[1]);
                postParams.Add(key, value);
            }
            
            return postParams;
        }
    }
}