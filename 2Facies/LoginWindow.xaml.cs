using _2Facies.Resource;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace _2Facies
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LoginWindow : Window
    {
        private Logger logger;

        //-------------------------------------------------------------------------------------
        //--------------------------타이틀바와 생성 초기화 함수----------------------------------
        //-------------------------------------------------------------------------------------
        public LoginWindow()
        {
            InitializeComponent();
            logger = new Logger(new System.IO.FileInfo($@"{FileResources.LogFile}"));
        }
        private async void Window_Initialized(object sender, System.EventArgs e)
        {
            var reqCheck = ServerClient.ServerConnectionCheck();
            var loading = new LoadingWindow("서버 연결 확인 중 ...");
            loading.Show();
            ControlsInitilize();

            if (!(await reqCheck))
            {
                MessageBox.Show("서버와의 연결에 실패했습니다.");
                logger.Log("Error connect server Failed");
                loading.LoadingDone();
                this.Close();
            }

            loading.LoadingDone();
        }
        private void ControlsInitilize()
        {
            Id_TextBox.MaxLength = Packet.MaxLength["id"];
            Password_TextBox.MaxLength = Packet.MaxLength["password"];
            SignInTemplateGrid.Visibility = Visibility.Visible;
            RegisterTemplateGrid.Visibility = Visibility.Hidden;
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

        private async void LoginButton_Clicked(object sender, RoutedEventArgs e)
        {
            Packet.Login loginData = new Packet.Login(Id_TextBox.Text, Password_TextBox.Password.ToString());
            string json = await ServerClient.Login(loginData);
            Dictionary<string, string> result = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

            if (result["succeed"] == "true")
            {
                //next user window
                UserWindow window = new UserWindow(result["token"]);
                window.Show();

                this.Close();
            }
            else
            {
                MessageBox.Show(result["message"]);
                logger.Log($"Error login Failed/{result["message"]}");
            }
        }
        private async void RegisterButton_Clicked(object sender, RoutedEventArgs e)
        {
            Packet.Register signUpData = new Packet.Register(
                Register_Id_Textbox.Text, Register_Password_TextBox.Text, Register_Name_Textbox.Text, Register_Email_Textbox.Text, int.Parse(Register_Age_Textbox.Text)
            );

            string json = await ServerClient.Register(signUpData);
            Dictionary<string, string> result = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

            if (result["result"] == "true")
            {
                MessageBox.Show("회원이 되신것을 축하드립니다.");
            }
            else
            {
                MessageBox.Show(result["message"]);
                logger.Log($"Error register Failed/{result["message"]}");
            }
        }
        private void RegisterLink_Textblock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (RegisterTemplateGrid.Visibility == Visibility.Visible) //To Signin state
            {
                (sender as TextBlock).Text = "아직 회원이 아니신가요?";
                RegisterTemplateGrid.Visibility = Visibility.Hidden;
                SignInTemplateGrid.Visibility = Visibility.Visible;
            }
            else
            {
                (sender as TextBlock).Text = "로그인 하기";
                RegisterTemplateGrid.Visibility = Visibility.Visible;
                SignInTemplateGrid.Visibility = Visibility.Hidden;
            }

        }
        private void ChangePasswordLink_Click(object sender, MouseButtonEventArgs e)
        {

        }

        private static readonly Regex _regex = new Regex("[^0-9.-]+");
        private void NumberOnly_PreviewInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = _regex.IsMatch(e.Text);
        }
    }
}
