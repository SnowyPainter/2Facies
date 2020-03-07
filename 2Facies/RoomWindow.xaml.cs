using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace _2Facies
{
    /// <summary>
    /// RoomWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class RoomWindow : Window
    {

        private WsClient client;

        public void ChatHandler(Packet.ErrorCode code)
        {
            switch (code)
            {
                case Packet.ErrorCode.ChatConnect:
                    MessageBox.Show("채팅 연결이 불안정합니다.");
                    break;
                case Packet.ErrorCode.ChatSend:
                    MessageBox.Show("채팅을 보낼수 없습니다.");
                    break;
                case Packet.ErrorCode.ChatRecv:
                    MessageBox.Show("채팅을 주고 받는데에 문제가 있습니다.");
                    break;
            }
        }
        public RoomWindow()
        {
            InitializeComponent();
        }
        //--------------------------------------------------------
        //------------------Control, Object Init-----------------
        //--------------------------------------------------------
        private void InitSocketEvents()
        {

            client.On("message", (ev) =>
            {
                var data = ev.Data.Split('@')[1];
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                {
                    var msg = new Resource.ChatMessage
                    {
                        Message = data,
                        BackColor = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFA2A2A2")),
                        TextAlign = TextAlignment.Left
                    };
                    ChatPanel.Children.Add(msg);
                }));

            });

            client.On("participants", (ev) =>
            {
                var participants = ev.Data.Split('@')[1];
                
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                {
                    ParticipantsText.Text = $"{participants}";
                }));
            });
        }
        public RoomWindow(string room, string participants)
        {
            InitializeComponent();

            client = new WsClient(ChatHandler);
            client.Join(room);

            InitSocketEvents();

            client.Emit("participants", WsClient.Room.Id, "");
            ParticipantsText.Text = $"{participants}명 접속중";
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(UserWindow.userData != null && WsClient.Room != null)
            {
                var r = WsClient.Room.Id;
                client.Leave(WsClient.Room.Id);
                client.Emit("participants", r, "");
            }
                
        }
        private void WindowClose_Btn_Clicked(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void WindowMinimize_Btn_Clicked(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        private void NavigationBar_DragMove(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
        //--------------------------------------------------------
        //------------------Control, Object Init-----------------
        //--------------------------------------------------------

        private void ChatTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            var text = ChatTextBox.Text;
            if (text != "" && e.Key == Key.Enter)
            {
                client.Emit("broadcast", WsClient.Room.Id, text);
                var msg = new Resource.ChatMessage
                {
                    Message = text,
                    BackColor = (SolidColorBrush)(new BrushConverter().ConvertFrom("#cccccc")),
                    TextAlign = TextAlignment.Right
                };
                ChatPanel.Children.Add(msg);

                ChatTextBox.Text = "";
                ChatScrollViewer.ScrollToBottom();
            }

        }
    }
}
