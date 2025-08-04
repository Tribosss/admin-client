using admin_client.Model;
using DotNetEnv;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for DashBoardControl.xaml
    /// </summary>
    public partial class DashBoardControl : UserControl
    {
        public DashBoardControl(UserData userData)
        {
            InitializeComponent();
        }
    }
}
