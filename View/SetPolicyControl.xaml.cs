using admin_client.Model;
using admin_client.ViewModel;
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
    /// Interaction logic for SetPolicyControl.xaml
    /// </summary>
    public partial class SetPolicyControl : UserControl
    {
        SetPolicyViewModel _vm;
        public SetPolicyControl()
        {
            InitializeComponent();
            _vm = new SetPolicyViewModel();
            this.DataContext = _vm;
        }

        private void UserSearchBox_TextChanged(object sender, TextChangedEventArgs e)
      {
            string keyword = UserSearchBox.Text;
            _vm.LoadUserListByKeyword(keyword);
            SearchUserList.Visibility = Visibility.Visible;
        }

        private void SearchUserList_LostFocus(object sender, RoutedEventArgs e)
        {
            SearchUserList.Visibility = Visibility.Hidden;
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Border border = (Border)sender;
            UserData uData = (UserData)border.DataContext;
            _vm.SelectedUser = uData;
            SelectedName.Text = uData.Name;
            SelectedPosition.Text = uData.Position;

            IndividualAgentToggleButton.IsChecked = uData.IsActiveAgent;
            IndividualDomainBlockToggleButton.IsChecked = uData.IsActiveDomainBlock;
            _vm.IsIndividualAgentActive = uData.IsActiveAgent;
            _vm.IsIndividualDomainBlockActive = uData.IsActiveDomainBlock;

            SearchUserList.Visibility = Visibility.Hidden;
            _vm.SearchUserList.Clear();
            UserSearchBox.Text = "";
        }

        //private void IndividualAgentToggleButton_Checked(object sender, RoutedEventArgs e)
        //{
        //    _vm.SetIsActiveAgentByToggleButton(true);
        //    _vm.PublishMessageAtClient("AGENT<ON>");
        //}

        //private void IndividualAgentToggleButton_Unchecked(object sender, RoutedEventArgs e)
        //{
        //    _vm.SetIsActiveAgentByToggleButton(false);
        //    _vm.PublishMessageAtClient("AGENT<OFF>");
        //}
    }
}
