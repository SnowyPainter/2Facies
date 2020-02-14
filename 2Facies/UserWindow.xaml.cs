using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

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
        private Packet.DataPublic userData;

        public UserWindow(string token)
        {
            InitializeComponent();
            userData = new Packet.DataPublic();
            Token = token;
        }
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var data = GetPrivateData(Token);

            //processing sync
            ContorlsInitilize();

            var jsonData = JsonConvert.DeserializeObject<Dictionary<string, string>>(await data);

            if(jsonData.ContainsKey("result") && jsonData["result"] == "false")
            {
                MessageBox.Show("토큰 정보가 올바르지 않습니다 로그인 창으로 돌아갑니다.");
                this.Close();
            }
            else
            {
                userData.Bind(jsonData);
                AsyncControlsInitilize(userData);
            }
            
        }
        private async Task<string> GetPrivateData(string token)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
                var getDataByToken = $"{RequestingUrls.Domain}/{RequestingUrls.UserTokenInfoURL}";
                string tokenData = await(await ServerClient.RequestGet(getDataByToken, client)).ReadAsStringAsync();
                return tokenData;
            }
        }
        private void AsyncControlsInitilize(Packet.DataPublic user)
        {
            Title_Text.Text = $"안녕하세요 {user.Name}";
        }
        private void ContorlsInitilize()
        {

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
    }
}
