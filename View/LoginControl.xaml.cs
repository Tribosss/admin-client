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
using DotNetEnv;
using MySql.Data.MySqlClient;
using Mysqlx;

namespace admin_client.View
{
    /// <summary>
    /// Interaction logic for LoginControl.xaml
    /// </summary>
    public partial class LoginControl : UserControl
    {

        internal event Action<AdminAuth>? SuccessLoginEvent;
        public LoginControl()
        {
            InitializeComponent();
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

            ValidAdminInfo(auth);
        }

        private void ValidAdminInfo(AdminAuth auth)
        {
            string? host, port, uid, pwd, name;
            string dbConnection;
            List<AdminAuth> existAuthInfos = []; 

            try
            {
                Env.Load();

                host = Environment.GetEnvironmentVariable("DB_HOST");
                if (host == null) return;
                port = Environment.GetEnvironmentVariable("DB_PORT");
                if (port == null) return;
                uid = Environment.GetEnvironmentVariable("DB_UID");
                if (uid == null) return;
                pwd = Environment.GetEnvironmentVariable("DB_PWD");
                if (pwd == null) return;
                name = Environment.GetEnvironmentVariable("DB_NAME");
                if (name == null) return;

                dbConnection = $"Server={host};Port={port};Database={name};Uid={uid};Pwd={pwd}";

                using (MySqlConnection connection = new MySqlConnection(dbConnection))
                {
                    connection.Open();

                    string selectQuery = "select a.login_id, a.password from employees as e ";
                    selectQuery += "inner join role r on r.id=e.role_id ";
                    selectQuery += "inner join auth a on a.id=e.id ";
                    selectQuery += $"where r.id=1 and a.login_id='{auth.LoginId}' and a.password='{auth.Password}' ";
                    selectQuery += "limit 1;";

                    MySqlCommand cmd = new MySqlCommand(selectQuery, connection);
                    MySqlDataReader rdr = cmd.ExecuteReader();
                    if (rdr == null) return;
                    if (!rdr.Read()) return;

                    string currentLoginId = rdr[0].ToString();
                    string currentPassword = rdr[1].ToString();
                    AdminAuth currentAdminAuth = new AdminAuth
                    {
                        LoginId = currentLoginId,
                        Password = currentPassword,
                    };

                    SuccessLoginEvent?.Invoke(currentAdminAuth);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
