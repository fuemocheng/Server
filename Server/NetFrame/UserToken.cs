using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetFrame
{
    /// <summary>
    /// 用户连接对象
    /// </summary>
    public class UserToken
    {
        /// <summary>
        /// Socket 套接字 连接
        /// </summary>
        public Socket m_socket;
        // 异步接受网络数据的对象
        public SocketAsyncEventArgs receiveSAEA;
        // 异步发送网络数据的对象
        public SocketAsyncEventArgs sendSAEA;

        public LengthEncode lengthEncode;
        public LengthDecode lengthDecode;
        public ObjectEncode messageEncode;
        public ObjectDecode messageDecode;

        private bool m_bIsReading = false;
        private bool m_bIsWriting = false;

        //接收消息的缓存
        List<byte> cache = new List<byte>();
        //发送消息的队列
        Queue<byte[]> writeQueue = new Queue<byte[]>();

        public delegate void SendProcess(SocketAsyncEventArgs e);
        public SendProcess sendProcess;

        public delegate void CloseProcess(UserToken token, string error);
        public CloseProcess closeProcess;

        public AbsHandlerCenter handlerCenter;

        public UserToken()
        {
            receiveSAEA = new SocketAsyncEventArgs();
            sendSAEA = new SocketAsyncEventArgs();
            receiveSAEA.UserToken = this;
            sendSAEA.UserToken = this;
            //设置接收消息对象的缓冲区大小
            receiveSAEA.SetBuffer(new byte[1024], 0, 1024);
        }

        //网络消息到达
        public void Receive(byte[] buff)
        {
            //消息写入缓存
            cache.AddRange(buff);

            if (false == m_bIsReading)
            {
                m_bIsReading = true;
                OnData();
            }
        }

        //缓存中有数据进行处理
        private void OnData()
        {
            //解码消息存储对象
            byte[] buff = null;

            //当粘包解码器存在的时候，进行粘包处理
            if (null != lengthDecode)
            {
                buff = lengthDecode(ref cache);
                //消息未接收全 退出数据处理 等待下次消息到达
                if (null == buff)
                {
                    m_bIsReading = false;
                    return;
                }
            }
            else
            {
                //不用处理粘包
                //缓存区中没有数据 直接跳出数据处理 等待消息到达
                if (cache.Count == 0)
                {
                    m_bIsReading = false;
                    return;
                }
            }
            //反序列化方法是否存在 此方法必须存在
            if (null == messageDecode) { throw new Exception("Message decode process is null"); }
            //进行消息反序列化
            object message = messageDecode(buff);

            //TODO 通知应用层 有消息到达
            handlerCenter.MessageReceive(this, message);

            //尾递归 防止在消息存储过程中 有其他消息到达而没有经过处理
            OnData();
        }

        public void Write(byte[] value)
        {
            if (null == m_socket)
            {
                //此连接已经断开
                closeProcess(this, "调用已经断开的Socket连接");
                return;
            }
            writeQueue.Enqueue(value);
            if (false == m_bIsWriting)
            {
                m_bIsWriting = true;
                OnWrite();
            }
        }

        public void OnWrite()
        {
            //判断发送队列是否有信息
            if (writeQueue.Count == 0)
            {
                m_bIsWriting = false;
                return;
            }
            //取出第一条代发消息
            byte[] buff = writeQueue.Dequeue();
            //设置消息发送异步对象的发送数据缓冲区数据
            sendSAEA.SetBuffer(buff, 0, buff.Length);
            //开启异步发送
            bool willRaiseEvent = m_socket.SendAsync(sendSAEA);
            //是否挂起
            if (false == willRaiseEvent)
            {
                sendProcess(sendSAEA);
            }
        }

        public void SendCallback()
        {
            //与OnData尾递归同理
            OnWrite();
        }

        public void Close()
        {
            try
            {
                writeQueue.Clear();
                cache.Clear();
                m_bIsReading = false;
                m_bIsWriting = false;
                m_socket.Shutdown(SocketShutdown.Both);
                m_socket.Close();
                m_socket = null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
