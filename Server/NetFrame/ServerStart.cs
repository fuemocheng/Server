using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;

namespace NetFrame
{
    public class ServerStart
    {
        private Socket m_listenSocket;
        private int m_nMaxClient;
        private Semaphore m_semaphore;
        UserTokenPool m_userTokenPool;

        public LengthEncode lengthEncode;
        public LengthDecode lengthDecode;
        public ObjectEncode messageEncode;
        public ObjectDecode messageDecode;

        /// <summary>
        /// 消息处理中心，由外部应用传入
        /// </summary>
        public AbsHandlerCenter handlerCenter;

        /// <summary>
        /// 初始化通信监听
        /// </summary>
        /// <param name="port"></param>
        public ServerStart(int maxClient)
        {
            //实例化监听对象
            m_listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //设定最大连接数
            m_nMaxClient = maxClient;
        }

        public void Start(int port)
        {
            //创建连接池
            m_userTokenPool = new UserTokenPool(m_nMaxClient);
            //创建信号量
            m_semaphore = new Semaphore(m_nMaxClient, m_nMaxClient);

            for (int i = 0; i < m_nMaxClient; i++)
            {
                //初始化 token 信息
                UserToken token = new UserToken();
                token.receiveSAEA.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
                token.sendSAEA.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
                token.lengthEncode = lengthEncode;
                token.lengthDecode = lengthDecode;
                token.messageEncode = messageEncode;
                token.messageDecode = messageDecode;
                token.sendProcess = ProcessSend;
                token.closeProcess = ClientClose;
                token.handlerCenter = handlerCenter;
                m_userTokenPool.Push(token);
            }

            //监听当前服务器网络所有可用IP地址的port端口
            //外网IP，内网IP，本机IP127.0.0.1；
            try
            {
                m_listenSocket.Bind(new IPEndPoint(IPAddress.Any, port));
                //置于监听状态
                m_listenSocket.Listen(10);

                StartAccept(null);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void StartAccept(SocketAsyncEventArgs e)
        {
            if (null == e)
            {
                e = new SocketAsyncEventArgs();
                e.Completed += new EventHandler<SocketAsyncEventArgs>(Accept_Completed);
            }
            else
            {
                e.AcceptSocket = null;
            }

            //信号量-1
            m_semaphore.WaitOne();

            //false 表示没挂起，则立即执行， true 表示挂起，则等异步完成执行Accept_Completed
            bool willRaiseEvent = m_listenSocket.AcceptAsync(e);
            if (false == willRaiseEvent)
            {
                ProcessAccept(e);
            }
        }

        private void Accept_Completed(object sender, SocketAsyncEventArgs e)
        {
            ProcessAccept(e);
        }

        private void ProcessAccept(SocketAsyncEventArgs e)
        {
            UserToken token = m_userTokenPool.Pop();
            token.m_socket = e.AcceptSocket;
            //token.m_socket.NoDelay = false;

            //TODO 通知应用层，有客户端连接
            handlerCenter.ClientConnect(token);

            //开启消息到达监听
            StartReceive(token);
            //释放当前异步对象
            StartAccept(e);
        }


        private void StartReceive(UserToken token)
        {
            try
            {
                bool willRaiseEvent = token.m_socket.ReceiveAsync(token.receiveSAEA);
                if (false == willRaiseEvent)
                {
                    ProcessReceive(token.receiveSAEA);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }


        private void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    ProcessReceive(e);
                    break;
                case SocketAsyncOperation.Send:
                    ProcessSend(e);
                    break;
                default:
                    Console.WriteLine("SocketAsyncOperation is not receive or send.");
                    break;
            }

        }

        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            UserToken token = e.UserToken as UserToken;

            //判断网络消息接收是否成功
            if (token.receiveSAEA.BytesTransferred > 0 && token.receiveSAEA.SocketError == SocketError.Success)
            {
                //将网络消息拷贝到自定义数组
                byte[] message = new byte[token.receiveSAEA.BytesTransferred];
                Buffer.BlockCopy(token.receiveSAEA.Buffer, 0, message, 0, token.receiveSAEA.BytesTransferred);

                //处理接收的消息
                token.Receive(message);

                StartReceive(token);
            }
            else
            {
                if (token.receiveSAEA.SocketError != SocketError.Success)
                {
                    ClientClose(token, token.receiveSAEA.SocketError.ToString());
                }
                else
                {
                    ClientClose(token, "客户端主动断开连接");
                }
            }
        }


        private void ProcessSend(SocketAsyncEventArgs e)
        {
            UserToken token = e.UserToken as UserToken;
            if (e.SocketError != SocketError.Success)
            {
                ClientClose(token, e.SocketError.ToString());
            }
            else
            {
                //消息发送成功，回调成功
                token.SendCallback();
            }
        }

        /// <summary>
        /// 客户端断开连接
        /// </summary>
        /// <param name="token"></param>
        /// <param name="error"></param>
        private void ClientClose(UserToken token, string error)
        {
            if (token.m_socket != null)
            {
                lock (token)
                {
                    //通知应用层面 客户端断开连接了
                    handlerCenter.ClientClose(token, error);

                    token.Close();
                    m_userTokenPool.Push(token);
                    m_semaphore.Release();
                    Console.WriteLine(error);
                }
            }
        }
    }
}
