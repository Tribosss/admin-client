using System.Windows;
using System.Windows.Controls;
using admin_client.ViewModel;

namespace admin_client.View
{
    /// <summary>
    /// Interaction logic for LoginControl.xaml
    /// </summary>
    public partial class LoginControl : UserControl
    {
        private readonly LoginViewModel _viewModel;

        internal event Action? SuccessLoginEvent;
        public LoginControl()
        {
            InitializeComponent();
            _viewModel = this.DataContext as LoginViewModel ?? new LoginViewModel();
            this.DataContext = _viewModel;
        }

        private void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            string loginId = LoginIdInput.Text;
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

            SuccessLoginEvent?.Invoke();
        }
    }
}
