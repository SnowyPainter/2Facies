using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace _2Facies
{
    /// <summary>
    /// RoomBrowserWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class RoomBrowserWindow : Window
    {

        //--------------------------------
        //TESTCODE------------------------

        

        public RoomBrowserWindow()
        {
            InitializeComponent();
            List<Packet.Room> list = new List<Packet.Room>();
            list.Add(new Packet.Room("Hello", 5));
            list.Add(new Packet.Room("aaadf", 2));
            list.Add(new Packet.Room("Heladfaslo", 55));
            list.Add(new Packet.Room("Hesdfasfsfsdfasdllo", 12));

            RoomListView.ItemsSource = list;
        }
        public void ErrorHandler(Packet.ErrorCode code)
        {
            switch (code)
            {
                case Packet.ErrorCode.WrongCode:
                    MessageBox.Show("Wrong Code Sent");
                    break;
                case Packet.ErrorCode.RoomJoin:
                    MessageBox.Show("There was an error with join room");
                    break;
                case Packet.ErrorCode.RoomLeave:
                    MessageBox.Show("There was an error with leave room");
                    break;
            }
        }
        //--------------------------------
        //--------------------------------

        private WsClient client;

        public RoomBrowserWindow(List<Packet.Room> list)
        {
            InitializeComponent();
            client = new WsClient(ErrorHandler);

            RoomListView.ItemsSource = list;
        }

        //Window Navigation Bar
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
        //-----------------------

        private void RoomListView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var room = RoomListView.SelectedItem as Packet.Room;
            SelectedRoomTitle_Textbox.Text = room.Title;

            if(WsClient.Room != null && room.Id == WsClient.Room.Id) //already connected
            {
                JoinButton.IsEnabled = false;
            }
            else
            {
                JoinButton.IsEnabled = true;
            }
        }
        private void JoinButton_Click(object sender, RoutedEventArgs e)
        {
            var room = RoomListView.SelectedItem as Packet.Room;
            if (room == null || WsClient.Room != null && WsClient.Room.Id == room.Id)
                return;

            client.Join(room.Id);
            JoinButton.IsEnabled = false;

            RoomWindow rw = new RoomWindow();
            rw.Show();

            this.Close();
        }
    }
}
