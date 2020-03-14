using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
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

        public RoomBrowserWindow()
        {
            InitializeComponent();
            client = new WsClient(ErrorHandler);
        }
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            RoomListView.ItemsSource = await ServerClient.RoomList(10);
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
        private async void ReloadRoomList_Btn_Clicked(object sender, RoutedEventArgs e)
        {
            RoomListView.ItemsSource = await ServerClient.RoomList(10);
        }
        //-----------------------

        private void RoomListView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var room = RoomListView.SelectedItem as Packet.Room;
            SelectedRoomTitle_Textbox.Text = room.Title;

            if (WsClient.Room != null && room.Id == WsClient.Room.Id) //already connected
            {
                AlarmMessageTextblock.Text = "이미 접속된 방입니다.";
                JoinButton.IsEnabled = false;
            }
            else
            {
                AlarmMessageTextblock.Text = "";
                JoinButton.IsEnabled = true;
            }
        }
        private void JoinButton_Click(object sender, RoutedEventArgs e)
        {
            var room = RoomListView.SelectedItem as Packet.Room;
            if (room == null || WsClient.Room != null && WsClient.Room.Id == room.Id)
                return;

            JoinButton.IsEnabled = false;

            RoomWindow rw = new RoomWindow(room.Id); //, room.Participants
            rw.Show();

            this.Close();
        }
    }
}
