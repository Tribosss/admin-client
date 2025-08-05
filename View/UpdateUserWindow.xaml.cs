using admin_client.Model;
using DotNetEnv;
using MySql.Data.MySqlClient;
using System.Windows;

namespace admin_client.View
{
    /// <summary>
    /// Interaction logic for UpdateUserWindow.xaml
    /// </summary>
    public partial class UpdateUserWindow : Window
    {
        private UserData _currentUserData;
        public UpdateUserWindow(UserData uData)
        {
            InitializeComponent();
            UserId.Text = uData.Id;
            UpdateNameBox.Text = uData.Name;
            UpdateEmailBox.Text = uData.Email;
            UpdateAddressBox.Text = uData.Address;
            UpdateAgeBox.Text = uData.Age.ToString();

            switch (uData.Position) {
                case "ADMIN": UserPositionComboBox.SelectedIndex = 0; break;
                case "STAFF": UserPositionComboBox.SelectedIndex = 1; break;
            }
            _currentUserData = uData;
        }

        private void DeleteUserBtn_Click(object sender, RoutedEventArgs e)
        {
            string query = $"delete from employees where id='{_currentUserData.Id}';";
            string? dbName, dbHost, dbPwd, dbUid, dbPort;
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

                    if (cmd.ExecuteNonQuery() == 1)
                    {
                        Console.WriteLine("Success Delete");
                        Close();
                    }
                    else
                    {
                        Console.WriteLine("Failed Delete");
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

        private void UpdateUserBtn_Click(object sender, RoutedEventArgs e)
        {
            string id = UserId.Text;
            string name = UpdateNameBox.Text;   
            if (string.IsNullOrEmpty(name) && _currentUserData.Name != name) return;
            string email = UpdateEmailBox.Text;
            if (string.IsNullOrEmpty(email) && _currentUserData.Email != email) return;
            string address = UpdateAddressBox.Text;
            if (string.IsNullOrEmpty(address) && _currentUserData.Address != address) return;
            string age = UpdateAgeBox.Text;
            if (string.IsNullOrEmpty(age) && _currentUserData.Age.ToString() != age) return;
            string position = UserPositionComboBox.Text;
            if (string.IsNullOrEmpty(position) && _currentUserData.Position != position) return;

            UserData ud = new UserData()
            {
                Id = id,
                Name = name,
                Email = email,
                Address = address,
                Age = Int32.Parse(age),
                Position = position == "관리자" ? "ADMIN" : "STAFF"
            };

            UpdateUser(ud);
        }

        private void UpdateUser(UserData ud)
        {
            string? dbName, dbHost, dbPwd, dbUid, dbPort;
            string roleId = ud.Position == "ADMIN" ? "1" : "2";
            string query = "update employees e set ";
            query += $"e.name='{ud.Name}'," +
                $"e.email='{ud.Email}'," +
                $"e.address='{ud.Address}'," +
                $"e.age={ud.Age}," +
                $"e.role_id={roleId} ";
            query += $"where e.id='{ud.Id}'";

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

                    if (cmd.ExecuteNonQuery() == 1)
                    {
                        Console.WriteLine("Success Update");
                        Close();
                    }
                    else
                    {
                        Console.WriteLine("Failed Update");
                        throw new Exception("Failed User Update");
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
    }
}
