using System.Collections.Generic;
using System.IO.Ports;
using System.Windows;

namespace TestSerialCommunication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string _messageBuilder;

        public MainWindow()
        {
            InitializeComponent();

            _messageBuilder = string.Empty;

            StartUp();
        }

        private void StartUp()
        {
            var ports = SerialPort.GetPortNames();

            foreach (var port in ports)
                cbComPorts.Items.Add(port);


            var macroList = new List<MacroModel>
            {
                new (){ Name = "DEC Queue", Type = (char)0x05, Macro = "DECQ1" },
                new (){ Name = "Konfiguration", Type = (char)0x05, Macro = "GETB" },
                new (){ Name = "BTN1", Type = (char)0x06, Macro = "BTN1" },
                new (){ Name = "BTN2", Type = (char)0x06, Macro = "BTN2" },
                new (){ Name = "BTN3", Type = (char)0x06, Macro = "BTN3" },
                new (){ Name = "BTN4", Type = (char)0x06, Macro = "BTN4" }
            };

            macroList.ForEach(x => cbMacros.Items.Add(x));
        }

        private SerialPort _arduinoPort;
        private void btnStartSerial_Click(object sender, RoutedEventArgs e)
        {
            _arduinoPort = new SerialPort(cbComPorts.SelectedItem.ToString());

            _arduinoPort.BaudRate = 9600;
            _arduinoPort.Parity = Parity.None;
            _arduinoPort.StopBits = StopBits.One;
            _arduinoPort.DataBits = 8;
            _arduinoPort.Handshake = Handshake.None;

            _arduinoPort.DataReceived += OnArduinoDataReceived;

            _arduinoPort.Open();
        }

        private string _dataBuffer = string.Empty;
        private void OnArduinoDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var sp = (SerialPort)sender;
            _dataBuffer += sp.ReadExisting();

            if (_dataBuffer.EndsWith((char)0x03))
                OnDataComplete();
        }

        private void OnDataComplete()
        {
            /*if (_dataBuffer.StartsWith("BTN"))
            {
                _arduinoPort.Write("ACK" + _dataBuffer);
            }*/

            _dataBuffer = GetControlReplacedString(_dataBuffer);

            Dispatcher.Invoke(() => lvLog.Items.Add("Received: " + _dataBuffer));
            _dataBuffer = string.Empty;
        }

        private void btnSendSerial_Click(object sender, RoutedEventArgs e)
        {
            _arduinoPort?.Write(tbContentToSend.Text);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _arduinoPort?.Close();
            _arduinoPort?.Dispose();
        }

        private void btnClearLog_Click(object sender, RoutedEventArgs e)
        {
            lvLog.Items.Clear();
        }

        private string GetControlReplacedString(string toReplace)
        {
            toReplace = toReplace.Replace(((char)0x01).ToString(), "(SOH)");
            toReplace = toReplace.Replace(((char)0x02).ToString(), "(STX)");
            toReplace = toReplace.Replace(((char)0x03).ToString(), "(ETX)");
            toReplace = toReplace.Replace(((char)0x05).ToString(), "(ENQ)");
            toReplace = toReplace.Replace(((char)0x06).ToString(), "(ACK)");
            toReplace = toReplace.Replace(((char)0x1e).ToString(), "(RS)");
            toReplace = toReplace.Replace(((char)0x1f).ToString(), "(US)");

            return toReplace;
        }

        private void btnBuilderAddContent_Click(object sender, RoutedEventArgs e)
        {
            _messageBuilder += tbBuilderContent.Text;
            lblBuilderContent.Content = GetControlReplacedString(_messageBuilder);
        }

        private void btnBuilderAddETX_Click(object sender, RoutedEventArgs e)
        {
            _messageBuilder += (char)0x03;
            lblBuilderContent.Content = GetControlReplacedString(_messageBuilder);
        }

        private void btnBuilderAddENQ_Click(object sender, RoutedEventArgs e)
        {
            _messageBuilder += (char)0x05;
            lblBuilderContent.Content = GetControlReplacedString(_messageBuilder);
        }

        private void btnBuilderAddACK_Click(object sender, RoutedEventArgs e)
        {
            _messageBuilder += (char)0x06;
            lblBuilderContent.Content = GetControlReplacedString(_messageBuilder);
        }

        private void btnBuilderAddSTX_Click(object sender, RoutedEventArgs e)
        {
            _messageBuilder += (char)0x02;
            lblBuilderContent.Content = GetControlReplacedString(_messageBuilder);
        }

        private void btnBuilderClear_Click(object sender, RoutedEventArgs e)
        {
            _messageBuilder = string.Empty;
            lblBuilderContent.Content = GetControlReplacedString(_messageBuilder);
        }

        private void btnBuilderSend_Click(object sender, RoutedEventArgs e)
        {
            _arduinoPort.Write(_messageBuilder);
            lvLog.Items.Add("Sent: " + GetControlReplacedString(_messageBuilder));
        }

        private void btnUseMacro_Click(object sender, RoutedEventArgs e)
        {
            var macro = cbMacros.SelectedItem as MacroModel;
            _messageBuilder = macro.GetSerialString();
            lblBuilderContent.Content = GetControlReplacedString(_messageBuilder);
        }

        private void btnBuilderAddSOH_Click(object sender, RoutedEventArgs e)
        {
            _messageBuilder += (char)0x01;
            lblBuilderContent.Content = GetControlReplacedString(_messageBuilder);
        }
    }
}
