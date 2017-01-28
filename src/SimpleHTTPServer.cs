// 20170124 tkli
// 參考：https://gist.github.com/aksakalli/9191056
//
// MIT License - Copyright (c) 2016 Can Güney Aksakalli

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;
using System.Diagnostics;

namespace Web {

class SimpleHTTPServer
{
    private readonly string[] _indexFiles = { 
        "index.html", 
        "index.htm", 
        "default.html", 
        "default.htm" 
    };
    
    private static IDictionary<string, string> _mimeTypeMappings = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase) {
        #region extension to MIME type list
        {".asf", "video/x-ms-asf"},
        {".asx", "video/x-ms-asf"},
        {".avi", "video/x-msvideo"},
        {".bin", "application/octet-stream"},
        {".cco", "application/x-cocoa"},
        {".crt", "application/x-x509-ca-cert"},
        {".css", "text/css"},
        {".deb", "application/octet-stream"},
        {".der", "application/x-x509-ca-cert"},
        {".dll", "application/octet-stream"},
        {".dmg", "application/octet-stream"},
        {".ear", "application/java-archive"},
        {".eot", "application/octet-stream"},
        {".exe", "application/octet-stream"},
        {".flv", "video/x-flv"},
        {".gif", "image/gif"},
        {".hqx", "application/mac-binhex40"},
        {".htc", "text/x-component"},
        {".htm", "text/html"},
        {".html", "text/html"},
        {".ico", "image/x-icon"},
        {".img", "application/octet-stream"},
        {".iso", "application/octet-stream"},
        {".jar", "application/java-archive"},
        {".jardiff", "application/x-java-archive-diff"},
        {".jng", "image/x-jng"},
        {".jnlp", "application/x-java-jnlp-file"},
        {".jpeg", "image/jpeg"},
        {".jpg", "image/jpeg"},
        {".js", "application/x-javascript"},
        {".mml", "text/mathml"},
        {".mng", "video/x-mng"},
        {".mov", "video/quicktime"},
        {".mp3", "audio/mpeg"},
        {".mpeg", "video/mpeg"},
        {".mpg", "video/mpeg"},
        {".msi", "application/octet-stream"},
        {".msm", "application/octet-stream"},
        {".msp", "application/octet-stream"},
        {".pdb", "application/x-pilot"},
        {".pdf", "application/pdf"},
        {".pem", "application/x-x509-ca-cert"},
        {".pl", "application/x-perl"},
        {".pm", "application/x-perl"},
        {".png", "image/png"},
        {".prc", "application/x-pilot"},
        {".ra", "audio/x-realaudio"},
        {".rar", "application/x-rar-compressed"},
        {".rpm", "application/x-redhat-package-manager"},
        {".rss", "text/xml"},
        {".run", "application/x-makeself"},
        {".sea", "application/x-sea"},
        {".shtml", "text/html"},
        {".sit", "application/x-stuffit"},
        {".swf", "application/x-shockwave-flash"},
        {".tcl", "application/x-tcl"},
        {".tk", "application/x-tcl"},
        {".txt", "text/plain"},
        {".war", "application/java-archive"},
        {".wbmp", "image/vnd.wap.wbmp"},
        {".wmv", "video/x-ms-wmv"},
        {".xml", "text/xml"},
        {".xpi", "application/x-xpinstall"},
        {".zip", "application/zip"},
        #endregion
    };
    private Thread _serverThread;
    private string _rootDirectory;
    private HttpListener _listener;
    private int _port;
 
    public int Port
    {
        get { return _port; }
        private set { }
    }
 
    /// <summary>
    /// Construct server with given port.
    /// </summary>
    /// <param name="path">Directory path to serve.</param>
    /// <param name="port">Port of the server.</param>
    public SimpleHTTPServer(string path, int port)
    {
        this.Initialize(path, port);
    }
 
    /// <summary>
    /// Construct server with suitable port.
    /// </summary>
    /// <param name="path">Directory path to serve.</param>
    public SimpleHTTPServer(string path)
    {
        //get an empty port
        TcpListener l = new TcpListener(IPAddress.Loopback, 0);
        l.Start();
        int port = ((IPEndPoint)l.LocalEndpoint).Port;
        l.Stop();
        this.Initialize(path, port);
    }
 
    /// <summary>
    /// Stop server and dispose all functions.
    /// </summary>
    public void Stop()
    {
        _serverThread.Abort();
        _listener.Stop();
    }
 
    private void Listen()
    {
        _listener = new HttpListener();
        _listener.Prefixes.Add("http://*:" + _port.ToString() + "/");
        _listener.Start();
        while (true)
        {
            try
            {
                HttpListenerContext context = _listener.GetContext();
                Process(context);
            }
            catch (Exception ex)
            {
 
            }
        }
    }
    
    //-----------------------------------------------------------------------------------
    // 20170125 tkli
    // 設定一個 陣列，用來 route，基本上就是 一個 path 就對應到 一個 handler
    // handler 就是一個 繼承的體系，
    // PathHandler: 抽象的 類別，供 開發者繼承 來 實作 handler，
    // 裡頭 主要 宣告一個 abstract 的方法 handler(HttpListenerContext context)，
    // 另外一個 String path。
    //
    // 後來改用 Dictionary<string, PathHandler> 來做，
    // PathHandler 改為 interface
    // 路由的 path 直接在 Dictionary 的 key 那邊設定
    // 而 繼承 的類別就是主要 去 實作 handler() 函數
    //
    // 最後在 Process() 裡頭，就用一個 foreach 迴圈 輪詢一次，看看 client端來的 request path 是否有被設定 handler，有則執行，沒有則看有沒有對應的其他處理。
    //-----------------------------------------------------------------------------------
    private Dictionary<string, PathHandler> _handlers;
    
    public void SetHandler(string path, PathHandler handler){
        if (_handlers != null) _handlers.Add(path, handler);
    }
 
    private void Process(HttpListenerContext context)
    {
        string requestPath = context.Request.Url.AbsolutePath;
        Console.WriteLine(DateTime.Now.ToString() + ": " + requestPath);
        
        // 20170125 tkli
        // 輪詢 request path 是否有被設定 handler 
        // (感覺用 繼承和多型的手法 來做這件事，好像有點 脫褲子放屁，有點 多做了一些 表面功夫)
        if (_handlers != null){
            foreach (var handler in _handlers)
            {
                if (requestPath.Equals(handler.Key)){
                    Console.WriteLine(handler.Key);
                    try{
                        handler.Value.handler(context);
                    } catch(Exception ex){
                        Console.WriteLine(ex.ToString());
                    }
                    //Console.WriteLine("end......");
                    return;
                }
            }
        }

        string filename = requestPath.Substring(1);
        
        if (string.IsNullOrEmpty(filename))
        {
            foreach (string indexFile in _indexFiles)
            {
                if (File.Exists(Path.Combine(_rootDirectory, indexFile)))
                {
                    filename = indexFile;
                    break;
                }
            }
        }
 
        filename = Path.Combine(_rootDirectory, filename);
 
        if (File.Exists(filename))
        {
            try
            {
                Stream input = new FileStream(filename, FileMode.Open);
                
                //Adding permanent http response headers
                string mime;
                context.Response.ContentType = _mimeTypeMappings.TryGetValue(Path.GetExtension(filename), out mime) ? mime : "application/octet-stream";
                context.Response.ContentLength64 = input.Length;
                context.Response.AddHeader("Date", DateTime.Now.ToString("r"));
                context.Response.AddHeader("Last-Modified", System.IO.File.GetLastWriteTime(filename).ToString("r"));
 
                byte[] buffer = new byte[1024 * 16];
                int nbytes;
                while ((nbytes = input.Read(buffer, 0, buffer.Length)) > 0)
                    context.Response.OutputStream.Write(buffer, 0, nbytes);
                input.Close();
                
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                context.Response.OutputStream.Flush();
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
 
        }
        else
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
        }
        
        context.Response.OutputStream.Close();
    }
 
    private void Initialize(string path, int port)
    {
        this._rootDirectory = path;
        this._port = port;
        //this._handlers = new List<PathHandler>();
        this._handlers = new Dictionary<string, PathHandler>();
        _serverThread = new Thread(this.Listen);
        _serverThread.Start();
    }
 
 
}

}