using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Color = System.Windows.Media.Color;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        PortChat portChat;
        int inputTextIndex = 0;
        string hashStr = "";
        int hashVal = 0;
        string currentFrameString = "";

        public MainWindow()
        {
            portChat = new PortChat();

            InitializeComponent();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            e.Source.ToString();
            bool b = true;
            var val = textBox.Text[inputTextIndex];
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
                    portChat.Write('\n');
                }
                else
                {
                    currentFrameString = portChat.Write(val);
                    if (!string.IsNullOrEmpty(currentFrameString))
                    {
                        UpdateLogs();
                    }
                }
            }
            inputTextIndex++;
            hashStr = textBox.Text;
            hashVal = textBox.Text.GetHashCode();

        }

        private void StartInput(object sender, RoutedEventArgs e)
        {
            RadioButton pressed = (RadioButton)sender;
            try
            {
                portChat.InitializeWrite(pressed.Uid[0] - '0');
                portChat.InitializeRead(pressed.Uid[0] - '0');
            }
            catch(Exception ex)
            {
                pressed.IsChecked = false;
                MessageBox.Show(ex.Message);
                return;
            }
            logTextBox.AppendText("\n");
            logTextBox.AppendText(portChat.WritePort);
            logTextBox.AppendText(portChat.ReadPort);
            textBox1.IsReadOnly = false;
            sent1.IsEnabled = false;
            sent2.IsEnabled = false;
            sent3.IsEnabled = false;
            sent4.IsEnabled = false;
            Thread readThread = new Thread(ReadListen);
            readThread.Start();
        }

        private void UpdateLogs()
        {
            var mask = portChat.StringMask;
            for (int i =0; i < currentFrameString.Length; i++)
            {
                TextRange rangeOfText = new TextRange(logTextBox.Document.ContentEnd, logTextBox.Document.ContentEnd);
                rangeOfText.Text = currentFrameString[i].ToString();
                switch (mask[i])
                {
                    case '0': rangeOfText.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Black); break;
                    case '1': rangeOfText.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Green); break;
                    case '2': rangeOfText.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Orange); break;
                    case '3': rangeOfText.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Red); break;
                }
            }
            logTextBox.AppendText("\n");
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
            if (e.Key == Key.Up || e.Key == Key.Delete || e.Key == Key.Left || e.Key == Key.Back
                || e.Key == Key.LeftCtrl)
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
        }
    }
}
