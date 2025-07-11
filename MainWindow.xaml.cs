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
            snbControl.ShowSignUpRequestors += ShowSignUpRequestors;
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
            popup.Owner = this;           
            bool? result = popup.ShowDialog();
        }
        private void ShowSignUpRequestors()
        {
            SignUpRequestWindow popup = new SignUpRequestWindow();
            popup.Owner = this;           
            bool? result = popup.ShowDialog();
            RootGrid.Children.RemoveAt(1);
            RootGrid.Children.Add(new StaffManageControl());
        }
    }
}