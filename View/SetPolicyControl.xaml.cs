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

        private void AgentOffButton_Click(object sender, RoutedEventArgs e)
        {
            _vm.PublishMessageAtClient("AGENT<OFF>");
        }

        private void AgentOnButton_Click(object sender, RoutedEventArgs e)
        {
            _vm.PublishMessageAtClient("AGENT<ON>");
        }
    }
}
