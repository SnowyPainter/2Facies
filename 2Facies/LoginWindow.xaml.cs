using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Resources;
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

namespace _2Facies
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LoginWindow : Window
    {
        //-------------------------------------------------------------------------------------
        //--------------------------타이틀바와 생성 초기화 함수----------------------------------
        //-------------------------------------------------------------------------------------
        public LoginWindow()
        {
            InitializeComponent();
            ControlsInitilize(); //Wpf control 초기화
        }
        private void ControlsInitilize()
        {
            Id_TextBox.MaxLength = Packet.MaxLength["Id"];
            Password_TextBox.MaxLength = Packet.MaxLength["Password"];
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
        //ResourceManager UrlResourceManager = new ResourceManager("_2Facies.RequestingUrls", Assembly.GetExecutingAssembly());

        readonly string domain = RequestingUrls.Domain;

        private async void LoginButton_Clicked(object sender, RoutedEventArgs e)
        {
            Packet.Login loginData = new Packet.Login(Id_TextBox.Text, Password_TextBox.Text);
            string json = await ServerClient.Login(loginData);
            Dictionary<string, string> result = JsonConvert.DeserializeObject<Dictionary<string,string>>(json);

            if(result["succeed"] == "true")
            {
                //next window
                string token = result["token"];

                using(var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);
                    string info = await (await ServerClient.RequestGet($"{domain}/user/info", client)).ReadAsStringAsync();

                    MessageBox.Show(info);
                }

            }
        }
    }
}
