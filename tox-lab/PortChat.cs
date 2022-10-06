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

namespace WpfApp1
{
    internal class PortChat
    {

        SerialPort writePort;
        SerialPort readPort;
        string currentPortToConnect = "com";
        int currentWritePortNum = 0;
        int swapFlag = 0;

        public string InitializeRead(int bytesSize)
        {
            bool continueTrying = true;
            readPort = new SerialPort();
            readPort.BaudRate = readPort.BaudRate;
            readPort.Parity = readPort.Parity;
            readPort.DataBits = bytesSize;
            readPort.StopBits = readPort.StopBits;
            readPort.Handshake = readPort.Handshake;
            readPort.ReadTimeout = 500;
            readPort.WriteTimeout = 500;

            while (continueTrying)
            {
                readPort.PortName = SetPortName(currentWritePortNum + 1);
                try
                {
                    readPort.Open();
                    continueTrying = false;
                }
                catch (Exception e)
                {
                    swapFlag++;
                    continueTrying = true;
                }
            }

            if (swapFlag % 2 == 1 && swapFlag != 0)
            {
                var temp = new SerialPort();
                temp = writePort;
                writePort = readPort;
                readPort = temp;
            }

            return "opened port " + currentPortToConnect + "\n";
        }


        public string InitializeWrite(int bytesSize)
        {
            bool continueTrying = true;
            writePort = new SerialPort();

            writePort.BaudRate = writePort.BaudRate;
            writePort.Parity = writePort.Parity;
            writePort.DataBits = bytesSize;
            writePort.StopBits = writePort.StopBits;
            writePort.Handshake = writePort.Handshake;
            writePort.ReadTimeout = 500;
            writePort.WriteTimeout = 500;

            while (continueTrying)
            {
                writePort.PortName = SetPortName(currentWritePortNum);
                try
                {
                    writePort.Open();
                    continueTrying = false;
                }
                catch (Exception e)
                {
                    swapFlag++;
                    continueTrying = true;
                }
            }
            //if (swapFlag%2 == 1 && swapFlag != 0)
            return "opened port " + currentPortToConnect + "\n";
        }

        public void Close()
        {
            writePort.Close();
            readPort.Close();
        }

        public string Read()
        {
            string message = "";
            byte[] bytes = new byte[1];
            try
            {
                readPort.Read(bytes, 0, 1);
                message += Convert.ToChar(bytes[0], CultureInfo.CurrentCulture);
                return message;
            }
            catch (TimeoutException) { return String.Empty; }
        }

        public string SetPortName(int index = 0)
        {
            var ports = SerialPort.GetPortNames();
            for (int i = index; i < ports.Length; i++)
            {
                if (!currentPortToConnect.Equals(ports[i].ToString()))
                {
                    currentPortToConnect = ports[i].ToString();
                    currentWritePortNum = i + 1;
                    break;
                }
            }
            return currentPortToConnect;
        }


        public void Write(byte val)
        {
            byte[] array = new byte[1];
            array[0] = val;
            writePort.Write(array, 0, 1);
        }


        public string[] GetPortName()
        {
            return SerialPort.GetPortNames();
        }
    }
}
