using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace NetFrame
{
    public class SerializeUtil
    {
        /// <summary>
        /// 对象序列化
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] Encode(object value)
        {
            //创建内存流对象
            MemoryStream ms = new MemoryStream();
            //二进制流序列化对象
            BinaryFormatter bf = new BinaryFormatter();
            //将 obj对象 序列化成二进制数据，写入 内存流
            bf.Serialize(ms, value);
            byte[] result = new byte[ms.Length];
            //将流数据 拷贝到要返回的数组
            Buffer.BlockCopy(ms.GetBuffer(), 0, result, 0, (int)ms.Length);
            ms.Close();
            return result;
        }

        /// <summary>
        /// 对象反序列化
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static object Decode(byte[] value)
        {
            //创建编码解码的内存流对象 并将需要反序列化的数据写入其中
            MemoryStream ms = new MemoryStream(value);
            //二进制流序列化对象
            BinaryFormatter bf = new BinaryFormatter();
            //将流数据反序列化为obj对象
            object result = bf.Deserialize(ms);
            ms.Close();
            return result;
        }
    }
}
