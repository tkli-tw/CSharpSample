using System;
using System.Net;

namespace Web {

//-----------------------------------------------------------------------------------
// 20170125 tkli
// 設定一個 陣列，用來 route，基本上就是 一個 path 就對應到 一個 handler
// handler 就是一個 繼承的體系，
// PathHandler: 抽象的 類別，供 開發者繼承 來 實作 handler，
// 裡頭 主要 宣告一個 abstract 的方法 handler(HttpListenerContext context)，
// 另外一個 String path。
//-----------------------------------------------------------------------------------
interface PathHandler
{
    void handler(HttpListenerContext context);
}

}