using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetFrame;
using NetFrame.auto;
using GameProtocol;
using GameProtocol.dto;

namespace Server.Logic.Login
{
    public class LoginHandler : HandlerInterface
    {
        public void ClientClose(UserToken token, string error)
        {
            
        }

        public void ClientConnect(UserToken token)
        {
            
        }

        public void MessageReceive(UserToken token, SocketModel message)
        {
            switch (message.command)
            {
                case LoginProtocol.LOGIN_CRES:
                    Logined(token, message.GetMessage<AccoutInfoDTO>());
                    break;
                case LoginProtocol.RES_CREQ:
                    Register(token, message.GetMessage<AccoutInfoDTO>());
                    break;
                default:
                    break;
            }
        }

        public void Logined(UserToken token, AccoutInfoDTO value)
        {

        }

        public void Register(UserToken token, AccoutInfoDTO value)
        {

        }
    }
}
