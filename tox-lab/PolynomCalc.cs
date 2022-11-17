using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tox_lab
{
    public static class PolynomCalc
    {
        private static string MakeLenEqualLen(string bits, int len)
        {
            if (bits.Length != len)
            {
                bits = new string('0', len - bits.Length) + bits;
            }
            return bits;
        }

        private static string CicleLeftShift(string bits)
        {
            int len = bits.Length;
            var firstChar = bits[0];
            var newBits = (string)bits.Remove(0, 1) + firstChar;
            return newBits;
        }

        private static string XorStrings(string bits, string poly)
        {
            var tempStr = "";
            for (int i = 0; i < bits.Length; i++)
            {
                tempStr += (bits[i] == poly[i]) ? '0' : '1';
            }
            return tempStr;
        }

        public static int PolynomMul(byte xored, string poly = "100011101")
        {
            var bits = Convert.ToString(xored, 2);
            int countOf0 = 0;
            int len = poly.Length;
            bits = MakeLenEqualLen(bits, len);


            while (bits.Count(f => f == '1') > 1)
            {
                for (int i = len - 1; i >= 0; i--)
                {
                    if (bits[i] == '0')
                    {
                        bits = (string)bits.Remove(bits.Length - 1, 1);
                        countOf0++;
                    }
                    else break;
                }
                bits = MakeLenEqualLen(bits, len);
                if (countOf0 > byte.MaxValue)
                {
                    break;
                }
                bits = XorStrings(bits,poly);
                bits = MakeLenEqualLen(bits, len);
            }

            //for (int i = len - 1; i >= 0; i--)
            //{
            //    if (bits[i] == '0')
            //    {
            //        bits = (string)bits.Remove(bits.Length - 1, 1);
            //        countOf0++;
            //    }
            //    else break;
            //}

            return countOf0;
        }

        
    }
}
