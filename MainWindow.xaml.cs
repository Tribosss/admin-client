using admin_client.Model;
using admin_client.View;
using System.Windows;
using System.Windows.Controls;

namespace admin_client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private UserData _userData = null;
        public MainWindow()
        {
            InitializeComponent();
            RootGrid.Children.Clear();
            LoginControl control = new LoginControl();
            control.SuccessLoginEvent += HandleSuccessLogin;
            RootGrid.Children.Add(control);
        }

        public void HandleSuccessLogin(UserData userData)
        {
            _userData = userData;

            RootGrid.Children.Clear();
            SideBar snbControl = new SideBar(userData);
            snbControl.NavigateEvent += HandleNavigateControl;
            snbControl.ShowClickUserRow += ShowUpdateUserWindow;
            RootGrid.Children.Add(snbControl);
            RootGrid.Children.Add(new DashBoardControl(userData));
        }

        private void HandleNavigateControl(UserControl control)
        {
            RootGrid.Children.RemoveAt(1);
            RootGrid.Children.Add(control);
        }

        private void ShowUpdateUserWindow(UserData userData)
        {
            UpdateUserWindow popup = new UpdateUserWindow(userData);
            popup.Owner = this;           // 반드시 Owner를 지정하세요
            bool? result = popup.ShowDialog();
            // ShowDialog()가 닫힐 때까지 이 라인 이후 코드가 대기합니다.
            if (result == true)
            {
                // 다이얼로그에서 DialogResult = true 로 닫혔을 때 처리
            }
        }
    }
}