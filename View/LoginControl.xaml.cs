using System.Windows;
using System.Windows.Controls;
using admin_client.Model;
using admin_client.ViewModel;

namespace admin_client.View
{
    /// <summary>
    /// Interaction logic for LoginControl.xaml
    /// </summary>
    public partial class LoginControl : UserControl
    {
        private readonly LoginViewModel _viewModel;

        internal event Action<UserData>? SuccessLoginEvent;
        public LoginControl()
        {
            InitializeComponent();
            _viewModel = this.DataContext as LoginViewModel ?? new LoginViewModel();
            this.DataContext = _viewModel;
        }
         
        private void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            string loginId = IdInput.Text;
            string password = PasswordInput.Text;
            AdminAuth auth = new AdminAuth
            {
                LoginId = loginId,
                Password = password,
            };

            bool isSuccessValid = _viewModel.ValidAdminInfo(auth);
            // 오류메시지 표시
            if (!isSuccessValid)
            {
                return;
            }
            UserData userData = _viewModel.GetUserData(auth);

            SuccessLoginEvent?.Invoke(userData);
        }
    }
}
