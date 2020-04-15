using Fleck;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
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

namespace WsServerUseFleck
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        List<IWebSocketConnection> allSockets = new List<IWebSocketConnection>();

        private ObservableCollection<string> _Clients = new ObservableCollection<string>();


        public ObservableCollection<string> Clients
        {
            get { return _Clients; }
            set
            {
                _Clients = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged();
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;



            //var allSockets = new List<IWebSocketConnection>();
            FleckLog.Level = LogLevel.Debug;
            
            // **若改成wss://** 則是安全web socket透過X.509認證
            var server = new WebSocketServer("ws://192.168.10.8:25000");

            try
            {
                server.Start(socket =>
                {
                    // 新的Socket已連線
                    socket.OnOpen = () =>
                    {
                        //lblMessage.Content = "Open!";
                        allSockets.Add(socket);

                        if (!Clients.Contains(socket.ConnectionInfo.ClientIpAddress))
                            Application.Current.Dispatcher.BeginInvoke((Action)delegate () { Clients.Add(socket.ConnectionInfo.ClientIpAddress); });

                    };
                    // Socket離線
                    socket.OnClose = () =>
                    {
                        //lblMessage.Content = "Close!";
                        allSockets.Remove(socket);

                        if (Clients.Contains(socket.ConnectionInfo.ClientIpAddress))
                            Application.Current.Dispatcher.BeginInvoke((Action)delegate () { Clients.Remove(socket.ConnectionInfo.ClientIpAddress); });
                    };
                    // 傳送訊息
                    socket.OnMessage = message =>
                    {
                        Console.WriteLine(message);
                        allSockets.ToList().ForEach(z => z.Send("Echo: " + message));
                        Application.Current.Dispatcher.BeginInvoke((Action)delegate () { lbxMessages.Items.Add(message); });
                    };
                });
            }
            catch (Exception ex)
            {

                throw ex;
            }

            
        }

        

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            foreach (var socket in allSockets)
            {
                JObject jObject = JObject.FromObject(new
                {
                    function = "getApiVersion"
                });


                socket.Send($@"{jObject.ToString()}");


                //socket.Send($@"{tbxMessage.Text}");
            }
        }
    }
}
