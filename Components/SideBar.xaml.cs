using admin_client.Model;
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

namespace admin_client.View
{
    /// <summary>
    /// Interaction logic for SideBar.xaml
    /// </summary>
    public partial class SideBar : UserControl
    {
        public Action<UserControl>? NavigateEvent;
        public Action<UserData>? ShowClickUserRow;
        public Action? ShowSignUpRequestors;
        private UserData _userData = null;
        public SideBar(UserData userData)
        {
            InitializeComponent();
            _userData = userData;
            EmpId.Text += " " + userData.Id;
        }

        private void DashBoardNav_Click(object sender, RoutedEventArgs e)
        {
            DashBoardControl control = new DashBoardControl(_userData);
            NavigateEvent?.Invoke(control);
        }

        private void PolicyNav_Click(object sender, RoutedEventArgs e)
        {
            SetPolicyControl control = new SetPolicyControl();
            NavigateEvent?.Invoke(control);
        }

        private void LogViewNav_Click(object sender, RoutedEventArgs e)
        {
            ViewLogControl control = new ViewLogControl();
            NavigateEvent?.Invoke(control);
        }

        private void StaffManageNav_Click(object sender, RoutedEventArgs e)
        {
            StaffManageControl control = new StaffManageControl();
            control.ShowClickUserRow += ShowClickUserRow.Invoke;
            control.ShowSignUpRequestors += ShowSignUpRequestors.Invoke;
            NavigateEvent?.Invoke(control);
        }
    }
}
