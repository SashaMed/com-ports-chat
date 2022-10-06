using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Windows.Input.Cursor;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        PortChat portChat;
        int inputTextIndex = 0;
        string hashStr = "";
        int hashVal = 0;
        string portRead;
        string portWrite;

        public MainWindow()
        {
            portChat = new PortChat();

            InitializeComponent();
            logTextBox.Text = "\n";
            var openPorts = portChat.GetPortName();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            e.Source.ToString();
            bool b = true;
            byte val = (byte)textBox.Text[inputTextIndex];
            var s = textBox1.Text;
            if (s.Length > 2)
            {
                var tempStr = hashStr;
                s = s.Substring(0, s.Length - 1);
                var t = s.GetHashCode();
                var tt = hashStr.Substring(0, hashStr.Length - 1).GetHashCode();
                if (hashVal != t && val != '\r')
                {
                    b = false;
                    try
                    {
                        textBox.Text = hashStr;
                    }
                    catch { }
                    return;
                }
            }

            if (b)
            {
                if (textBox.Text[inputTextIndex] == '\r')
                {
                    portChat.Write(val);
                    inputTextIndex++;
                    portChat.Write((byte)'\n');
                }
                else
                {
                    portChat.Write(val);
                }
                logTextBox.Text = "\n" + portWrite + "\n" + portRead + "\n";
                logTextBox.Text += "bytes sent: " + inputTextIndex.ToString() + "\n";
            }
            inputTextIndex++;
            hashStr = textBox.Text;
            hashVal = textBox.Text.GetHashCode();
        }

        private void StartInput(object sender, RoutedEventArgs e)
        {
            RadioButton pressed = (RadioButton)sender;
            textBox1.IsReadOnly = false;
            portWrite = portChat.InitializeWrite(pressed.Uid[0] - '0');
            logTextBox.Text += portWrite;
            sent1.IsEnabled = false;
            sent2.IsEnabled = false;
            sent3.IsEnabled = false;
            sent4.IsEnabled = false;
        }

        private void StartOutput(object sender, RoutedEventArgs e)
        {
            RadioButton pressed = (RadioButton)sender;
            portRead = portChat.InitializeRead(pressed.Uid[0] - '0');
            logTextBox.Text += portRead;
            received1.IsEnabled = false;
            received2.IsEnabled = false;
            received3.IsEnabled = false;
            received4.IsEnabled = false;
            Thread readThread = new Thread(ReadListen);
            readThread.Start();
        }

        private void ReadListen()
        {
            while (true)
            {
                var msg = portChat.Read();
                Dispatcher.Invoke(() => { outputTextBox.Text += msg; });
            }
        }


        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            portChat.Close();

            System.Windows.Application.Current.Shutdown();
        }

        private void textBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up || e.Key == Key.Delete || e.Key == Key.Left || e.Key == Key.Back)
            {
                e.Handled = true;
            }
        }

        private void TextBox_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            char inp = e.Text[0];

            if ((inp >= 'а' && inp <= 'я') || (inp >= 'А' && inp <= 'Я'))
            {
                e.Handled = true;
            }
            var s = textBox1.Text;
            if (s.Length > 2)
            {
                //var tempStr = hashStr;
                //s = s.Substring(0, s.Length - 1);
                //var t = s.GetHashCode();
                //var tt = hashStr.Substring(0, hashStr.Length - 1).GetHashCode();
                //if (tt != t)
                //{
                //    e.Handled = true;
                //}
            }
        }


        private void myButton_MouseEnter(object sender, EventArgs e)
        {
            //textBox1.Cursor = Cursors.None;           
        }


    }
}
