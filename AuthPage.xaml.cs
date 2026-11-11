    using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Windows.Threading;

namespace Laba10
{
    /// <summary>
    /// Логика взаимодействия для AuthPage.xaml
    /// </summary>
    public partial class AuthPage : Page
    {
        Random rnd = new Random(); // генератор случайных символов
        string captchaText = ""; // текущая капча
        DispatcherTimer timer = new DispatcherTimer();

        public AuthPage()
        {
            InitializeComponent();
            timer.Interval = TimeSpan.FromSeconds(10); // блокировка на 10 секунд
            timer.Tick += (s, e) =>
            {
                LoginBtn.IsEnabled = true; // разблокируем кнопку
                timer.Stop();
            };
        }

        private void GuesBtn_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new ProductPage(null));
            LoginTB.Text = "";
            PassTB.Text = "";
            CaptchaTB.Text = "";
            CaptchaPanel.Visibility = Visibility.Collapsed;
        }

        private void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginTB.Text;
            string password = PassTB.Text;
            if(login == "" || password == "")
            {
                MessageBox.Show("Есть пустые поля");
                return;
            }

            if (CaptchaPanel.Visibility == Visibility.Visible && CaptchaTB.Text != captchaText)
            {
                MessageBox.Show("Неверная капча");
                GenerateCaptcha();
                Block();
                return;
            }

            User user = Губайдуллин41Entities.GetContext().User.ToList().Find(p => p.UserLogin == login && p.UserPassword == password);
            if (user != null)
            {
                Manager.MainFrame.Navigate(new ProductPage(user));
                LoginTB.Text = "";
                PassTB.Text = "";
                CaptchaPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                MessageBox.Show("Введены неверные данные");
                if (CaptchaPanel.Visibility == Visibility.Visible)
                {
                    Block(); // ошибка, блок на 10 секунд
                }
                GenerateCaptcha();
            }

           
        }

        private void GenerateCaptcha()
        {
            string chars  = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

            captchaText = "";
            for (int i = 0; i <4; i++)
            {
                captchaText += chars[rnd.Next(chars.Length)];
            }
            captchaOneWord.Text = captchaText[0].ToString();
            captchaTwoWord.Text = captchaText[1].ToString();
            captchaThreeWord.Text = captchaText[2].ToString();
            captchaFourWord.Text = captchaText[3].ToString();
            CaptchaTB.Text = "";
            CaptchaPanel.Visibility = Visibility.Visible;
        }
        private void Block()
        {
            LoginBtn.IsEnabled = false; 
            timer.Start();
        }
        
    }
}
