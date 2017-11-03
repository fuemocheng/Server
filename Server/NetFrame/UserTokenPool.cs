using System;
using System.Collections.Generic;
using System.Text;

namespace NetFrame
{
    public class UserTokenPool
    {
        private Stack<UserToken> m_stackUTPool;

        public UserTokenPool(int maxClient)
        {
            m_stackUTPool = new Stack<UserToken>(maxClient);
        }

        /// <summary>
        /// 取出一个连接对象
        /// </summary>
        public UserToken Pop()
        {
            return m_stackUTPool.Pop();
        }

        /// <summary>
        /// 插入一个连接对象
        /// </summary>
        public void Push(UserToken token)
        {
            if (null != token)
                m_stackUTPool.Push(token);
        }

        public int Size
        {
            get { return m_stackUTPool.Count; }
        }
    }
}
