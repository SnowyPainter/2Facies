using _2Facies.Resource;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace _2Facies
{
    /// <summary>
    /// Interaction logic for UserWindow.xaml
    /// </summary>
    public partial class UserWindow : Window
    {
        //readonly constants variables
        private readonly string Token = null;
        //data variables
        public static Packet.DataPublic UserData;

        //private WsClient client;
        private Logger logger;

        
        public UserWindow(string token)
        {
            InitializeComponent();
            UserData = new Packet.DataPublic();
            logger = new Logger(new System.IO.FileInfo($@"{FileResources.LogFile}"));

            Token = token;
        }
        /*bool testing;
         public UserWindow() { testing = true; }*/


        private void QuickMatchUIReset(bool start)
        {
            isLookingForPlayer = start;
            MaterialDesignThemes.Wpf.ButtonProgressAssist.SetIsIndicatorVisible(FindFaciesButton, isLookingForPlayer);
        }

        //-------------------------------------------------------------------------------------
        //---------------------Window Initialize, navigion bar btn events------------------
        //-------------------------------------------------------------------------------------
        private void AsyncControlsInitilize(Packet.DataPublic user)
        {
            NameBlock.Text = user.Name;
            AgeBlock.Text = user.Age.ToString();
            EmailBlock.Text = user.Email;
        }
        private void ContorlsInitilize()
        {
            Title_Text.Text = $"2FACIES 안녕하세요";
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //if (testing) return;

            var loading = new LoadingWindow("서버와 연결중입니다 ...");
            loading.Show();

            var data = GetPrivateData(Token);
            var reqCheck = ServerClient.ServerConnectionCheck();

            //processing sync
            ContorlsInitilize();

            var jsonData = JsonConvert.DeserializeObject<Dictionary<string, string>>(await data);

            if (jsonData.ContainsKey("result") && jsonData["result"] == "false")
            {
                MessageBox.Show("토큰 정보가 올바르지 않습니다 로그인 창으로 돌아갑니다.");
                logger.Log("Error Token information is faulty");
                loading.LoadingDone();
                this.Close();
            }
            else
            {
                UserData.Bind(jsonData);
                AsyncControlsInitilize(UserData);
            }

            if (!(await reqCheck))
            {
                MessageBox.Show("서버와의 연결에 실패했습니다.");
                logger.Log("UserWindow connect with server failed", true);
                loading.LoadingDone();
                this.Close();
            }

            loading.LoadingDone();
        }
        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (MessageBox.Show("정말 종료하시겠습니까?", "종료", MessageBoxButton.YesNo) == MessageBoxResult.No)
            {
                e.Cancel = true;
                return;
            }
            /*
            if (WsClient.Room != null)
                client.Leave(WsClient.Room.Id);
            */
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
                var getDataByToken = $"{Http.Request.Domain}/{Http.Request.LogoutRequestURL}";
                string tokenData = await (await ServerClient.RequestGet(getDataByToken, client)).ReadAsStringAsync();
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
        //-------------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------------

        private bool isLookingForPlayer = false;
        private async Task<string> GetPrivateData(string token)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
                var getDataByToken = $"{Http.Request.Domain}/{Http.Request.UserTokenInfoURL}";
                string tokenData = await (await ServerClient.RequestGet(getDataByToken, client)).ReadAsStringAsync();
                return tokenData;
            }
        }
        private async void FindPlayerButton_Clicked(object sender, RoutedEventArgs e)
        {
            QuickMatchUIReset(true);
            MessageBox.Show(isLookingForPlayer ? "상대 매칭을 시작합니다." : "상대 매칭을 취소했습니다.");

            var list = await ServerClient.ConnectableRoomList();
            if(list.Count <= 0)
            {
                MessageBox.Show("현재 참여할수있는 방이 없습니다");
            }
            else
            {
                RoomWindow window = new RoomWindow(list[0].Id);
                window.ShowDialog();
            }

            QuickMatchUIReset(false);
        }
        private void OpenRoomBrowser_Clicked(object sender, RoutedEventArgs e)
        {
            var browser = new RoomBrowserWindow();
            browser.ShowDialog();
        }
        //**
        //deaccomplished
        private void TakePictureButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog() { Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png" };
            var result = dialog.ShowDialog();
            if (result == true)
            {
                ProfileCardImage.Source = new BitmapImage(new Uri(dialog.FileName));
            }
        }
    }
}
