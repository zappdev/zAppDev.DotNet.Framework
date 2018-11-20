using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace CLMS.Framework.Sockets
{
    public class BufferManager
    {
        Int32 m_numBytes;
        byte[] m_buffer;
        Stack<int> m_freeIndexPool;
        Int32 m_currentIndex;
        Int32 m_bufferSize;

        public BufferManager(int totalBytes, Int32 bufferSize)
        {
            m_numBytes = totalBytes;
            m_currentIndex = 0;
            m_bufferSize = bufferSize;
            m_freeIndexPool = new Stack<int>();
        }
                
        internal void InitBuffer()
        {
            m_buffer = new byte[m_numBytes];
        }
        
        internal bool SetBuffer(SocketAsyncEventArgs args)
        {
            if (m_freeIndexPool.Count > 0)
            {
                args.SetBuffer(m_buffer, m_freeIndexPool.Pop(), m_bufferSize);
            }
            else
            {
                if ((m_numBytes - m_bufferSize) < m_currentIndex)
                {
                    return false;
                }
                args.SetBuffer(m_buffer, m_currentIndex, m_bufferSize);
                m_currentIndex += m_bufferSize;
            }
            return true;
        }

        internal void FreeBuffer(SocketAsyncEventArgs args)
        {
            m_freeIndexPool.Push(args.Offset);
            args.SetBuffer(null, 0, 0);
        }
    }
}
