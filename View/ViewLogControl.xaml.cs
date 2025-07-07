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
        public List<UserData> UserDataList = [];

        public ViewLogControl()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private void UserSearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedUserId)) return;

            Logs.Clear();
            UserList.Visibility = Visibility.Hidden;

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

        private void UserList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (UserList.SelectedItem == null) return;
            _selectedTxt = UserList.SelectedItem!.ToString()!;
            if (string.IsNullOrEmpty(_selectedTxt)) return;

            _selectedUserPosition = _selectedTxt.Split("[")[1].Split("]")[0].Trim();
            _selectedUserName = _selectedTxt.Split("]")[1].Split("(")[0].Trim();
            _selectedUserId = _selectedTxt.Split("(")[1].TrimEnd(')');

            UserSearchBox.Text = _selectedTxt;

            SelectedPosition.Text = _selectedUserPosition;
            SelectedName.Text = _selectedUserName;

            UserList.Visibility = Visibility.Hidden;
        }

        private void UserSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (UserSearchBox.Text == _selectedTxt) return;

            string listTxt, position = "";
            string keyword = UserSearchBox.Text;

            _selectedUserId = "";
            _selectedUserName = "";
            _selectedUserPosition = "";
            Logs.Clear();
            UserList.Visibility = Visibility.Hidden;

            if (string.IsNullOrEmpty(keyword)) return;

            List<UserData> uDataList = GetUserListByKeyword(keyword);
            if (uDataList == null || uDataList.Count <= 0) return;

            for (int i = 0; i < uDataList.Count; i++)
            {
                switch (uDataList[i].Position)
                {
                    case "ADMIN": position = "관리자"; break;
                    case "STAFF": position = "사원"; break;
                }
                listTxt = $"[{position}] {uDataList[i].Name}({uDataList[i].Id})";
                UserList.Items.Add(listTxt);
            }
            UserList.Visibility = Visibility.Visible;
        }

        private List<UserData> GetUserListByKeyword(string keyword)
        {
            if (string.IsNullOrEmpty(keyword)) return null;

            string query = "select e.id, e.name, r.position from employees e ";
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
                    if (rdr == null) return null;

                    UserList.Items.Clear();
                    UserDataList.Clear();

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

                    UserList.Height = UserDataList.Count * 24;

                    return UserDataList;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return null;
            }
        }

        private void UserList_LostFocus(object sender, RoutedEventArgs e)
        {
            UserList.Visibility = Visibility.Hidden;
        }
    }
}
