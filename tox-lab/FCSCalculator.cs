using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;

namespace tox_lab
{
    public static class FCSCalculator
    {

        public static int Generator { get => 0x11D; }
        public static byte ComputeCRC8(string msg)
        {
            byte[] bytes = new byte[msg.Length];
            for(int i = 0; i < msg.Length; i++)
            {
                bytes[i] = (byte)msg[i];
            }

            int generator = Generator;
            byte crc = 0;

            foreach (byte currByte in bytes)
            {
                crc ^= currByte;
                for (int i = 0; i < 8; i++)
                {
                    if ((crc & 0x80) != 0)
                    {
                        crc = (byte)((crc << 1) ^ generator);
                    }
                    else
                    {
                        crc <<= 1;
                    }
                }
            }

            return crc;
        }
    }
}
