using admin_client.Model;
using DotNetEnv;
using MySql.Data.MySqlClient;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace admin_client.View
{
    /// <summary>
    /// Interaction logic for ViewLogControl.xaml
    /// </summary>
    public partial class ViewLogControl : UserControl
    {
        string _selectedUserId;
        string _selectedUserName;
        string _selectedUserPosition;
        string _selectedTxt;
        public ObservableCollection<Log> Logs { get;  } = new ObservableCollection<Log>();
        public ObservableCollection<UserData> UserDataList { get;  } = new ObservableCollection<UserData>();

        public ViewLogControl()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private void UserSearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedUserId)) return;

            SelectedName.Text = _selectedUserName;
            SelectedPosition.Text = _selectedUserPosition;

            Logs.Clear();

            string query = "select s.msg, s.source_ip, s.source_port, s.dest_ip, s.dest_port, s.detected_at from employees e ";
            query += "inner join suspicion_logs s on s.emp_id=e.id ";
            query += "inner join role r on r.id=e.role_id ";
            query += $"where e.id='{_selectedUserId}' ";
            query += "limit 1;";

            string? dbHost, dbPort, dbUid, dbPwd, dbName;
            string msg, source, destination, detectedAt;
            string dbConnection;

            try
            {
                Env.Load();

                dbHost = Environment.GetEnvironmentVariable("DB_HOST");
                if (dbHost == null) throw new Exception(".env DB_HOST is null");
                dbPort = Environment.GetEnvironmentVariable("DB_PORT");
                if (dbPort == null) throw new Exception(".env DB_PORT is null");
                dbUid = Environment.GetEnvironmentVariable("DB_UID");
                if (dbUid == null) throw new Exception(".env DB_UID is null"); ;
                dbPwd = Environment.GetEnvironmentVariable("DB_PWD");
                if (dbPwd == null) throw new Exception(".env DB_PWD is null");
                dbName = Environment.GetEnvironmentVariable("DB_NAME");
                if (dbName == null) throw new Exception(".env DB_NAME is null");

                dbConnection = $"Server={dbHost};Port={dbPort};Database={dbName};Uid={dbUid};Pwd={dbPwd}";

                using (MySqlConnection connection = new MySqlConnection(dbConnection))
                {
                    connection.Open();

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    MySqlDataReader rdr = cmd.ExecuteReader();
                    if (rdr == null) return;

                    while (rdr.Read()) {
                        msg = rdr[0].ToString()!;
                        source = $"{rdr[1].ToString()}:{rdr[2].ToString()}";
                        destination = $"{rdr[3].ToString()}:{rdr[4].ToString()}";
                        detectedAt = rdr[5].ToString()!;

                        Logs.Add(new Log() { Msg = msg, Source = source, Destination = destination, DetectedAt = detectedAt });
                    }

                    connection.Close(); 
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return;
            }
        }

        private void UserSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string keyword = UserSearchBox.Text;

            if (string.IsNullOrEmpty(keyword)) return;

            UserDataList.Clear();

            string query = "select e.id, e.name, r.position " +
                "from employees e ";
            query += "inner join role r on r.id=e.role_id ";
            query += $"where e.name like '%{keyword}%' or e.id like '%{keyword}%' or r.position like '%{keyword}%' ";
            query += "order by e.name desc ";
            query += "limit 15;";

            string? dbHost, dbPort, dbUid, dbPwd, dbName;
            string dbConnection;

            try
            {
                Env.Load();

                dbHost = Environment.GetEnvironmentVariable("DB_HOST");
                if (dbHost == null) throw new Exception(".env DB_HOST is null");
                dbPort = Environment.GetEnvironmentVariable("DB_PORT");
                if (dbPort == null) throw new Exception(".env DB_PORT is null");
                dbUid = Environment.GetEnvironmentVariable("DB_UID");
                if (dbUid == null) throw new Exception(".env DB_UID is null"); ;
                dbPwd = Environment.GetEnvironmentVariable("DB_PWD");
                if (dbPwd == null) throw new Exception(".env DB_PWD is null");
                dbName = Environment.GetEnvironmentVariable("DB_NAME");
                if (dbName == null) throw new Exception(".env DB_NAME is null");

                dbConnection = $"Server={dbHost};Port={dbPort};Database={dbName};Uid={dbUid};Pwd={dbPwd}";

                using (MySqlConnection connection = new MySqlConnection(dbConnection))
                {
                    connection.Open();

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    MySqlDataReader rdr = cmd.ExecuteReader();
                    if (rdr == null) return;

                    while (rdr.Read())
                    {
                        string id = rdr[0].ToString()!;
                        string name = rdr[1].ToString()!;
                        string position = rdr[2].ToString()!;

                        UserData uData = new UserData
                        {
                            Id = id,
                            Name = name,
                            Position = position
                        };

                        UserDataList.Add(uData);
                    }
                    connection.Close();
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            SearchUserList.Visibility = Visibility.Visible;
        }

        private void UserList_LostFocus(object sender, RoutedEventArgs e)
        {
            SearchUserList.Visibility = Visibility.Hidden;
        }

        private void Border_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is UserData userData)
            {
                _selectedUserId = userData.Id;
                _selectedUserName = userData.Name;
                _selectedUserPosition = userData.Position;

                UserSearchBox.Text = $"[{_selectedUserPosition}] {_selectedUserName} ({_selectedUserId})";
                SearchUserList.Visibility = Visibility.Hidden;
            }
        }
    }
}
