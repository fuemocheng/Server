using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace NetFrame
{
    /// <summary>
    /// 将数据写成二进制
    /// </summary>
    public class ByteArray
    {
        MemoryStream ms = new MemoryStream();
        BinaryReader br;
        BinaryWriter bw;

        /// <summary>
        /// 默认构造
        /// </summary>
        public ByteArray()
        {
            br = new BinaryReader(ms);
            bw = new BinaryWriter(ms);
        }

        /// <summary>
        /// 带参构造
        /// </summary>
        /// <param name="buff"></param>
        public ByteArray(byte[] buff)
        {
            ms = new MemoryStream(buff);
            br = new BinaryReader(ms);
            bw = new BinaryWriter(ms);
        }

        public void Close()
        {
            bw.Close();
            br.Close();
            ms.Close();
        }

        /// <summary>
        /// 获取当前数据 读取到下标位置
        /// </summary>
        public int Position
        {
            get { return (int)ms.Position; }
        }

        /// <summary>
        /// 获取当前数据长度
        /// </summary>
        public int Length
        {
            get { return (int)ms.Length; }
        }

        /// <summary>
        /// 当前是否还有数据可以读取
        /// </summary>
        public bool Readable
        {
            get { return ms.Length > ms.Position; }
        }

        #region 写入
        public void Write(int value)
        {
            bw.Write(value);
        }
        public void Write(byte value)
        {
            bw.Write(value);
        }
        public void Write(bool value)
        {
            bw.Write(value);
        }
        public void Write(string value)
        {
            bw.Write(value);
        }
        public void Write(byte[] value)
        {
            bw.Write(value);
        }

        public void Write(double value)
        {
            bw.Write(value);
        }
        public void Write(float value)
        {
            bw.Write(value);
        }
        public void Write(long value)
        {
            bw.Write(value);
        }
        #endregion


        #region 读取
        public void Read(out int value)
        {
            value = br.ReadInt32();
        }
        public void Read(out byte value)
        {
            value = br.ReadByte();
        }
        public void Read(out bool value)
        {
            value = br.ReadBoolean();
        }
        public void Read(out string value)
        {
            value = br.ReadString();
        }
        public void Read(out byte[] value, int length)
        {
            value = br.ReadBytes(length);
        }
        public void Read(out double value)
        {
            value = br.ReadDouble();
        }
        public void Read(out float value)
        {
            value = br.ReadSingle();
        }
        public void Read(out long value)
        {
            value = br.ReadInt64();
        }
        #endregion

        /// <summary>
        /// 重置读取位置
        /// </summary>
        public void ResetPosition()
        {
            ms.Position = 0;
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <returns></returns>
        public byte[] GetBuffer()
        {
            byte[] result = new byte[ms.Length];
            Buffer.BlockCopy(ms.GetBuffer(), 0, result, 0, (int)ms.Length);
            return result;
        }
    }
}
