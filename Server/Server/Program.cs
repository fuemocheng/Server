using NetFrame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using NetFrame.auto;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            ServerStart server = new ServerStart(9000);
            server.handlerCenter = new HandlerCenter();
            server.messageEncode = MessageEncoding.Encode;
            server.messageDecode = MessageEncoding.Decode;
            server.lengthEncode = LengthEncoding.Encode;
            server.lengthDecode = LengthEncoding.Decode;         
            server.Start(8001);
            Console.WriteLine("Server starts success!");
            while (true){}
        }
    }
}
