using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetFrame;
using Server.Logic;
using Server.Logic.Login;
using NetFrame.auto;
using GameProtocol;

namespace Server
{
    public class HandlerCenter : AbsHandlerCenter
    {
        HandlerInterface login;

        public HandlerCenter()
        {
            login = new LoginHandler();
        }

        public override void ClientClose(UserToken token, string error)
        {
            Console.WriteLine("[ {0} ] 断开连接,{1}",token.m_socket.ToString(),error);
        }

        public override void ClientConnect(UserToken token)
        {
            Console.WriteLine("[ {0} ] 连接",token.m_socket.RemoteEndPoint.ToString());
        }

        public override void MessageReceive(UserToken token, object message)
        {
            SocketModel model = message as SocketModel;
            switch (model.type)
            {
                case Protocol.TYPE_LOGIN:
                    login.MessageReceive(token, model);
                    break;

                default:
                    break;
            }
        }
    }
}
