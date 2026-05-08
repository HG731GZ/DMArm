using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMArmDLL

{
    public class CANBuffer  //缓存CAN总线上收到的数据，用于均值滤波等信号处理
    {
        public UInt16 buffer_size;
        public byte[][] buffer;
        public CANBuffer(uint size)
        {
            buffer_size = (UInt16)size;
            buffer = new byte[buffer_size][];
            for (int i = 0; i < buffer_size; i++)
            {
                buffer[i] = new byte[8];
                for (int j = 0; j < 8; j++)
                {
                    buffer[i][j] = 0;
                }
            }
        }
        public void add_canframe(byte[] candata)
        {
            move_buffer();
            buffer[0] = candata;
        }
        private void move_buffer()//目前数组所有内容向下移动一行
        {
            for (int i = buffer_size - 1; i >= 1; i--)
            {
                buffer[i] = buffer[i - 1];
            }
        }
    }
}
