using admin_client.Model;
using admin_client.View;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
            RootGrid.Children.Add();
            RootGrid.Children.Add(new DashBoardControl(userData));
        }

        private void HandleNavigateControl(UserControl control)
        {
            RootGrid.Children.RemoveAt(1);
            RootGrid.Children.Add(control);
        }
    }
}