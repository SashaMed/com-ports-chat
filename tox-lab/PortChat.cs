using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Globalization;
using tox_lab;
using System.Collections;
using System.Reflection;

namespace WpfApp1
{
    internal class PortChat
    {
        public string WritePort
        {
            get { return $"sender port - {writePort.PortName}\n"; }
        }

        public string ReadPort
        {
            get { return $"receiver port - {readPort.PortName}\n"; }
        }

        public string StringMask { get => byteConverter.StringMask; }

        public string CurrentFrame { get; private set; }
        SerialPort writePort;
        SerialPort readPort;
        string currentPortToConnect = "com";
        int currentWritePortNum = 0;
        int currentReadPortNum = 1;
        int swapFlag = 0;
        int readWriteSize = 20;
        int dataSize = 15;
        string writeStr = "";
        ByteStaffingConverter byteConverter;

        public PortChat()
        {
            byteConverter = new ByteStaffingConverter((char)('a' + 15), readWriteSize);
        }

        public void InitializeRead(int bytesSize)
        {
            bool continueTrying = true;
            readPort = new SerialPort();
            readPort.DataBits = bytesSize;
            InitializePort(ref readPort);

            if (currentWritePortNum != 0)
            {
                currentReadPortNum = currentWritePortNum;
            }
            var portsCount = GetPortName().Length;
            int i = 0;
            while (continueTrying)
            {
                if (i > portsCount)
                {
                    throw new Exception("no available ports - try to connect later\n");
                }
                readPort.PortName = SetPortName(currentReadPortNum + 1);
                try
                {
                    readPort.Open();
                    continueTrying = false;
                }
                catch (UnauthorizedAccessException)
                {
                    swapFlag++;
                    continueTrying = true;
                }
                i++;
            }
            CheckPortSwap(ref readPort, ref writePort, swapFlag);
        }


        public void InitializeWrite(int bytesSize)
        {
            bool continueTrying = true;
            writePort = new SerialPort();
            writePort.DataBits = bytesSize;
            InitializePort(ref writePort);
            var portsCount = GetPortName().Length;
            int i = 0;
            while (continueTrying)
            {
                if (i > portsCount)
                {
                    throw new Exception("no available ports - try to connect later\n");
                }
                writePort.PortName = SetPortName(currentWritePortNum);
                try
                {
                    writePort.Open();
                    continueTrying = false;
                }
                catch (UnauthorizedAccessException)
                {
                    swapFlag++;
                    continueTrying = true;
                }
                i++;
            }
            CheckPortSwap(ref readPort, ref writePort, swapFlag);
        }


        public static void InitializePort(ref SerialPort port)
        {
            port.BaudRate = 9600;
            port.Parity = port.Parity;
            port.StopBits = port.StopBits;
            port.Handshake = port.Handshake;
            port.ReadTimeout = 500;
            port.WriteTimeout = 500;
        }

        public static void CheckPortSwap(ref SerialPort readPort, ref SerialPort writePort, int swapFlag)
        {
            if (swapFlag % 2 == 0 && swapFlag != 0)
            {
                if (readPort != null && writePort != null)
                {
                    var temp = new SerialPort();
                    temp = writePort;
                    writePort = readPort;
                    readPort = temp;
                }
            }
        }

        public void Close()
        {
            writePort.Close();
            readPort.Close();
        }

        public string SetPortName(int index = 0)
        {
            var ports = SerialPort.GetPortNames();
            for (int i = index; i < ports.Length; i++)
            {
                if (!currentPortToConnect.Equals(ports[i].ToString()))
                {
                    currentPortToConnect = ports[i].ToString();
                    break;
                }
            }
            return currentPortToConnect;
        }

        public string Read()
        {
            string message = "";
            byte[] bytes = new byte[readWriteSize + 1];
            try
            {
                if (readPort.Read(bytes, 0, readWriteSize + 1) > -1)
                {
                    var inputFCS = bytes[bytes.Length - 1];

                    message = byteConverter.DecodeToString(bytes);
                    byte calculatedFCS = FCSCalculator.ComputeCRC8(message);
                    if (inputFCS != calculatedFCS)
                    {

                        byte xored = (byte)(inputFCS ^ calculatedFCS);
                        var mul = PolynomCalc.PolynomMul(calculatedFCS);
                        var index = PolynomCalc.PolynomMul(xored);
                        int byteIndex = (int)Math.Ceiling((decimal)(120 - index) / 8);
                        return message = ErrorCorrection(inputFCS, bytes, byteIndex);
                    }
                }
                return message;
            }
            catch (TimeoutException) { return String.Empty; }
        }


        private string ErrorCorrection(int sFCS, byte[] inBytes, int byteIndex)
        {
            var changeIndex = byteIndex + 3;
            byte[] bytes = new byte[inBytes.Length];
            Array.Copy(inBytes, 0, bytes, 0, inBytes.Length);
            var message = byteConverter.DecodeToString(bytes);
            byte calculatedFCS = FCSCalculator.ComputeCRC8(message);
            if (calculatedFCS ==sFCS)
            {
                return message;
            }
            for (int i = 0; i < 8; i++)
            {
                var prevValue = bytes[changeIndex];
                var p = i;
                byte mask = (byte)(1 << p);
                bytes[changeIndex] = (byte)((bytes[changeIndex] & ~mask) 
                    | ((Convert.ToInt32(!((bytes[changeIndex] & (1 << p)) != 0)) << p) & mask));
                var newMessage = byteConverter.DecodeToString(bytes);
                calculatedFCS = FCSCalculator.ComputeCRC8(newMessage);
                if (calculatedFCS == sFCS)
                {
                    return newMessage;
                }
                else
                {
                    bytes[changeIndex] = prevValue;
                }
            }
            return null;
        }

        public string Write(char val)
        {
            writeStr += val;
            if (writeStr.Length == dataSize)
            {
                var FCS = FCSCalculator.ComputeCRC8(writeStr);
                var array = byteConverter.EncodeToByteArray(writeStr, currentWritePortNum, 0, FCS);
                var currentFrame = byteConverter.EncodeToString(writeStr, currentReadPortNum, 0, FCS);
                Random random = new Random();
                if (random.Next(0, 100) % 2 == 0)
                {
                    var t = random.Next(5, array.Length - 2);
                    while ((char)array[t] == '\r' && (char)array[t] == '\n' && (char)array[t] != 'p') t = random.Next(5, array.Length - 2);
                    var p = random.Next(0, 7);
                    byte mask = (byte)(1 << p);
                    var tempArrayT = (byte)((array[t] & ~mask) | ((Convert.ToInt32(!((array[t] & (1 << p)) != 0)) << p) & mask));
                    if (tempArrayT > 31)
                    {
                        var beforeChanges = Convert.ToString(array[t], 2);
                        currentFrame += $", {(char)array[t]}({array[t].ToString()}) to {(char)tempArrayT}({tempArrayT.ToString()})";
                        array[t] = tempArrayT;
                        var changedMessage = byteConverter.DecodeToString(array);
                        currentFrame = byteConverter.EncodeToString(changedMessage, currentReadPortNum, 0, FCS);
                    }
                }
                writePort.Write(array, 0, readWriteSize + 1);
                writeStr = "";
                return currentFrame;
            }
            return String.Empty;
        }

        public string[] GetPortName()
        {
            return SerialPort.GetPortNames();
        }
    }
}
