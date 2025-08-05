using admin_client.Model;
using DotNetEnv;
using MySql.Data.MySqlClient;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace admin_client.View
{
    /// <summary>
    /// Interaction logic for StaffManageControl.xaml
    /// </summary>
    public partial class StaffManageControl : UserControl
    {
        public Action<UserData>? ShowClickUserRow;
        public Action? ShowSignUpRequestors;
        public ObservableCollection<UserData> UserDataList { get;  } = new ObservableCollection<UserData>();
        public StaffManageControl()
        {
            InitializeComponent();
            this.DataContext = this;

            GetUserListByKeyword("", "", "");
            int requestCount = GetSignUpRequestCount();
            SignUpRequestCount.Text = requestCount.ToString();
        }

        private void UserFilterButton_Click(object sender, RoutedEventArgs e)
        {
            string idKey = FilterIdBox.Text;
            string positionKey = FilterPositionBox.Text;
            string nameKey = FilterNameBox.Text;

            GetUserListByKeyword(idKey, positionKey, nameKey);
        }

        private int GetSignUpRequestCount()
        {
            string query = "select count(id) from signup_requests where is_allow=false and is_deny=false;";
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
                    if (rdr == null) return 0;
                    if (!rdr.Read()) return 0;

                    int count = Int32.Parse(rdr[0].ToString()!);

                    connection.Close();
                    return count;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return 0;
            }
        }

        private void GetUserListByKeyword(string idKey, string positionKey, string nameKey)
        {
            string query = "select e.id, e.name, r.position, e.email, e.address, e.created_at from employees e ";
            query += "inner join role r on r.id=e.role_id ";
            if (!string.IsNullOrEmpty(nameKey) || !string.IsNullOrEmpty(idKey) || !string.IsNullOrEmpty(positionKey))
            {
                query += @$"where 
                        {nameKey ?? $"e.name like '%{nameKey}%'"}
                        {(!string.IsNullOrEmpty(nameKey) && !string.IsNullOrEmpty(idKey) ? " and " : "")}
                        {idKey ?? $"e.id like '%{idKey}%'"}
                        {(!string.IsNullOrEmpty(positionKey) && !string.IsNullOrEmpty(idKey) ? " and " : "")}
                        {positionKey ?? $"r.position like '%{positionKey}%'"} ";
            }
            query += "order by e.name desc;";

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

                    UserDataList.Clear();

                    while (rdr.Read())
                    {
                        string id = rdr[0].ToString()!;
                        string name = rdr[1].ToString()!;
                        string position = rdr[2].ToString()!;
                        string email = rdr[3].ToString()!;
                        string address = rdr[4].ToString()!;
                        string createdAt = rdr[5].ToString()!;

                        UserData uData = new UserData
                        {
                            Id = id,
                            Name = name,
                            Position = position,
                            Email = email,
                            Address=address,
                            CreatedAt=createdAt
                        };

                        UserDataList.Add(uData);
                    }

                    connection.Close();

                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void UserData_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (ShowClickUserRow == null) return;
            if (!(sender is DataGridRow row && row.Item is UserData clickedUserData)) return;

            ShowClickUserRow.Invoke(clickedUserData);
            GetUserListByKeyword("", "", "");
            GetSignUpRequestCount();
        }

        private void CheckSignUpRequestBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ShowSignUpRequestors == null) return;

            ShowSignUpRequestors.Invoke();
            GetUserListByKeyword("", "", "");
            GetSignUpRequestCount();
        }
    }
}
