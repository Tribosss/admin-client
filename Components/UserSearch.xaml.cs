using admin_client.Model;
using DotNetEnv;
using MySql.Data.MySqlClient;
using System.Windows;
using System.Windows.Controls;

namespace admin_client.Components
{
    /// <summary>
    /// Interaction logic for UserSearch.xaml
    /// </summary>
    public partial class UserSearch : UserControl
    {
        string _selectedUserId;

        public UserSearch()
        {
            InitializeComponent();
        }
        private void UserSearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedUserId)) return;
        }

        private void UserList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selectedTxt = UserList.SelectedItem.ToString();
            if (string.IsNullOrEmpty(selectedTxt)) return;

            _selectedUserId = selectedTxt.Split("(")[1].TrimEnd(')');
            UserSearchBox.Text = selectedTxt;
        }

        private void UserSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string listTxt, position = "";
            string keyword = UserSearchBox.Text;

            _selectedUserId = "";
            UserList.Visibility = Visibility.Hidden;

            if (string.IsNullOrEmpty(keyword)) return;

            List<UserData> uDataList = GetUserListByKeyword(keyword);
            if (uDataList == null || uDataList.Count <= 0) return;

            for (int i = 0; i < uDataList.Count; i++) {
                switch (uDataList[i].Position)
                {
                    case "Admin": position = "관리자"; break;
                    case "Staff": position = "사원"; break;
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
            List<UserData> userDataList = new List<UserData>();

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

                    while (rdr.Read())
                    {
                        string id = rdr[0].ToString()!;
                        string name = rdr[1].ToString()!;
                        string position = rdr[2].ToString()!;

                        UserData uData = new UserData
                        {
                            Id=id,
                            Name=name,
                            Position=position
                        };

                        userDataList.Add(uData);
                    }

                    return userDataList;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return null;
            }
        }
    }
}
