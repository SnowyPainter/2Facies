using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace _2Facies
{
    /// <summary>
    /// CreateRoomWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CreateRoomWindow : Window
    {
        public void ErrorHandler(Packet.ErrorCode code)
        {
            switch (code)
            {
                case Packet.ErrorCode.WrongCode:
                    MessageBox.Show("Wrong Code");
                    break;
                case Packet.ErrorCode.FormatError:
                    MessageBox.Show("Socket format error");
                    break;
                case Packet.ErrorCode.RoomExist:
                    MessageBox.Show("Cannot create room. existing");
                    break;
            }
        }

        private WsClient client;

        public CreateRoomWindow()
        {
            InitializeComponent();

            client = new WsClient(ErrorHandler);
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
        private static readonly Regex _regex = new Regex("[^0-9.-]+");
        private void NumberOnly_PreviewInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = _regex.IsMatch(e.Text);
        }

        private void CreateConfirm_Clicked(object sender, RoutedEventArgs e)
        {
            client.Create(Title_Textbox.Text, int.Parse(MaxPeople_Textbox.Text), (msge) =>
            {
                string id = msge.Data.Split('@')[1];
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() => {
                    RoomWindow rw = new RoomWindow(id);
                    rw.Show();

                    Close();
                }));
            });
        }
    }
}
