using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace tox_lab
{
    internal class ByteStaffingConverter
    {
        private byte flagByte;
        private char flagChar;
        private int packetSize;

        public string StringMask { get; private set; }

        public ByteStaffingConverter(char flag, int size)
        {
            flagByte = (byte)flag;
            flagChar = flag;
            packetSize = size;
        }

        public string EncodeToString(string msg, int sourceAddres, int destinationAddres = 0, int FCS = 0)
        {
            StringMask = "";
            var ret = flagChar + destinationAddres.ToString() + sourceAddres.ToString();

            StringMask = new string('1', ret.Length);
            int index;
            index = msg.IndexOf(flagChar);
            ret += ((index = msg.IndexOf(flagChar)) == -1) ? EncodeToStringWithFlag(msg, msg.Length) : EncodeToStringWithFlag(msg, index);
            StringMask += '2';
            StringMask += new string('4', FCS.ToString().Length);
            return ret + '0' + FCS.ToString();
        }

        private string EncodeToStringWithFlag(string msg, int index)
        {
            string ret = "";
            StringMask += new string('2', (1 + index).ToString().Length);
            ret += (++index).ToString();
            for (int i = 0; i < msg.Length; i++)
            {
                if (msg[i] == flagChar)
                {
                    var pos = msg.Substring(i + 1).IndexOf(flagChar);
                    var t = (pos == -1) ? (msg.Length - i).ToString() : (pos + 1).ToString();
                    ret += t;
                    StringMask += new string('3', t.Length);
                    continue;
                }
                var temp = SpecialSymbolsCheck(msg[i]);
                StringMask += new string('0', temp.Length);
                ret += temp;
            }
            return ret;
        }

        private string SpecialSymbolsCheck(char c)
        {
            if (c == '\r')
            {
                return @"\r";
            }
            if (c == '\n')
            {
                return @"\n";
            }
            return c.ToString();
        }

        public byte[] EncodeToByteArray(string msg, int sourceAddres, int destinationAddres = 0, byte FCS = 0)
        {
            int index = 0;
            byte[] toReturn = new byte[msg.Length + 6];
            toReturn[index++] = flagByte;
            toReturn[index++] = (byte)destinationAddres;
            toReturn[index++] = (byte)sourceAddres;
            int indexOfFoundFlag = msg.IndexOf(flagChar);
            toReturn[index++] = ((indexOfFoundFlag) == -1) ? (byte)(1 + msg.Length) : (byte)(++indexOfFoundFlag);
            for (int i = 0; i < msg.Length; i++)
            {
                if (msg[i] == flagChar)
                {
                    var pos = msg.Substring(i + 1).IndexOf(flagChar);
                    toReturn[index++] = (pos == -1) ? (byte)(msg.Length - i) : (byte)(pos + 1);
                    continue;
                }
                toReturn[index++] = (byte)msg[i];
            }
            toReturn[index++] = 0;
            toReturn[index++] = FCS;
            return toReturn;
        }

        public string DecodeToString(byte[] message)
        {
            string toReturn = "";
            char flag = (char)message[0];
            int index = 3;
            byte[] bytes = new byte[message.Length - (index + 1)];
            Array.Copy(message, index + 1, bytes, 0, message.Length - (index + 1));
            index = message[index];
            for (int i = 0; i < bytes.Length - 2; i++)
            {
                if (i == (index - 1))
                {
                    toReturn += flag;
                    index += bytes[i];
                }
                else
                {
                    toReturn += (char)bytes[i];
                }
            }

            return toReturn;
        }
    }
}
