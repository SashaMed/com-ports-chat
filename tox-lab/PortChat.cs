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
                if (readPort.Read(bytes, 0, readWriteSize) > -1)
                {
                    message += byteConverter.DecodeToString(bytes);
                }
                return message;
            }
            catch (TimeoutException) { return String.Empty; }
        }

        public string Write(char val)
        {
            writeStr += val;
            if (writeStr.Length == dataSize)
            {
                var array = byteConverter.EncodeToByteArray(writeStr, currentWritePortNum);
                var currentFrame = byteConverter.EncodeToString(writeStr, currentReadPortNum);
                writePort.Write(array, 0, readWriteSize);
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
