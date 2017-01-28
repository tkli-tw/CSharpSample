using System;

namespace Web{
    
    class Test{
        
        public static void Main(string[] args){
            Console.WriteLine("hi");
            
            //Db.MysqlDB.testDB();
            
            SimpleHTTPServer webServer = new SimpleHTTPServer("/home/ubuntu/workspace/webSiteSample/CSharpSample", 8080);
            webServer.SetHandler("/TestHandler2", new TestHandler());
            webServer.SetHandler("/TestHandler", new TestHandler());
            webServer.SetHandler("/TestHandlerWithPara", new TestHandlerWithPara());

            webServer.SetHandler("/loginVerify", new LoginVerify());
        }
    }
    
}