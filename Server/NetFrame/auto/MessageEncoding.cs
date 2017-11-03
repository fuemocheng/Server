using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetFrame.auto
{
    /// <summary>
    /// 消息体的序列化和反序列化
    /// </summary>
    public class MessageEncoding
    {
        /// <summary>
        /// 消息体序列化
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] Encode(object value)
        {
            SocketModel socketModel = value as SocketModel;
            ByteArray ba = new ByteArray();
            ba.Write(socketModel.type);
            ba.Write(socketModel.area);
            ba.Write(socketModel.command);
            //判断消息体是否为空  不为空则序列化后写入
            if (null != socketModel.message)
            {
                ba.Write(SerializeUtil.Encode(socketModel.message));
            }
            byte[] result = ba.GetBuffer();
            ba.Close();
            return result;
        }

        /// <summary>
        /// 消息体反序列化
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static object Decode(byte[] value)
        {
            SocketModel socketModel = new SocketModel();
            ByteArray ba = new ByteArray(value);

            //从数据中读取 三层协议  读取数据顺序必须和写入顺序保持一致
            byte type;
            int area;
            int command;
            ba.Read(out type);
            ba.Read(out area);
            ba.Read(out command);

            //判断读取完协议后 是否还有数据需要读取 是则说明有消息体 进行消息体读取
            if (ba.Readable)
            {
                byte[] message;
                //将剩余数据全部读取出来
                ba.Read(out message, ba.Length - ba.Position);
                //反序列化剩余数据为消息体
                socketModel.message = SerializeUtil.Decode(message);
            }
            ba.Close();
            return socketModel;
        }
    }
}
