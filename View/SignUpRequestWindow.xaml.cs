using admin_client.Model;
using DotNetEnv;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections;
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
using System.Windows.Shapes;

namespace admin_client.View
{
    /// <summary>
    /// Interaction logic for SignUpRequestWindow.xaml
    /// </summary>
    public partial class SignUpRequestWindow : Window
    {
        public ObservableCollection<SignUpRequest> Requests { get; } = new ObservableCollection<SignUpRequest>();
        public SignUpRequestWindow()
        {
            InitializeComponent();
            RequestListBox.ItemsSource = Requests;
            LoadRequests();
        }

        private void DenyBtn_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            SignUpRequest request = (SignUpRequest) btn.DataContext;
            string query = $"update signup_requests set is_deny=true where temp_emp_id='{request.TempEmpId}';";
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

                    if (cmd.ExecuteNonQuery() == 1)
                    {
                        Console.WriteLine("Success Update");
                        LoadRequests();
                    }
                    else
                    {
                        Console.WriteLine("Failed Update");
                    }

                    connection.Close();
                    return;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void AllowBtn_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            SignUpRequest request = (SignUpRequest)btn.DataContext;

            UserData currentUserData = GetRequestorById(request.TempEmpId);
            UpdateAllowedRequestor(currentUserData);
        }

        private void UpdateAllowedRequestor(UserData userData)
        {
            if (userData == null) return;
            string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            string updateQuery = $"update signup_requests set is_allow=true where temp_emp_id='{userData.Id}';";
            string insertEmpQuery = $"insert into employees(id, role_id, password, salt, created_at) values ('{userData.Id}', 2, '{userData.Password}', '{userData.Salt}', '{now}');";
            string insertPolicyQuery = $"insert into policys(emp_id) values('{userData.Id}');";
            string insertBlockDomainQuery = $@"insert into blocked_domains (domain, emp_id) select domain, '{userData.Id}' from default_block_domains;";

            string? dbHost, dbPort, dbUid, dbPwd, dbName;
            string dbConnection;
            UserData currentUData = new UserData();

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
                    try
                    {
                        MySqlCommand updateCmd = new MySqlCommand(updateQuery, connection);
                        MySqlCommand insertEmpCmd = new MySqlCommand(insertEmpQuery, connection);
                        MySqlCommand insertPolicyCmd = new MySqlCommand(insertPolicyQuery, connection);
                        MySqlCommand insertBlockDomainCmd = new MySqlCommand(insertBlockDomainQuery, connection);
                        if (
                            insertEmpCmd.ExecuteNonQuery() == 1 
                            && updateCmd.ExecuteNonQuery() == 1 
                            && insertPolicyCmd.ExecuteNonQuery() == 1
                            && insertBlockDomainCmd.ExecuteNonQuery() == 1
                        ) {
                            Console.WriteLine("Success Update");
                        }
                        else
                        {
                            Console.WriteLine("Failed Update");
                        }
                        LoadRequests();
                    } catch(MySqlException mse)
                    {
                        Console.WriteLine(mse.ToString());
                    }

                    connection.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            return;
        }

        private UserData GetRequestorById(string tempEmpId)
        {
            string selectQuery = $"select temp_emp_id, password, salt from signup_requests where temp_emp_id='{tempEmpId}' limit 1;";

            string? dbHost, dbPort, dbUid, dbPwd, dbName;
            string dbConnection;
            UserData currentUData = new UserData();

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

                    MySqlCommand cmd = new MySqlCommand(selectQuery, connection);
                    MySqlDataReader rdr = cmd.ExecuteReader();
                    if (rdr == null) return null;

                    while (rdr.Read())
                    {
                        currentUData = new UserData()
                        {
                            Id = rdr[0].ToString(),
                            Password = rdr[1].ToString(),
                            Salt = rdr[2].ToString()
                        };
                    }

                    connection.Close();
                    return currentUData;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return null;
            }
        }

        private void LoadRequests()
        {
            Requests.Clear();

            string query = "select temp_emp_id from signup_requests where is_allow=false and is_deny=false";

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
                        SignUpRequest request = new SignUpRequest() { TempEmpId= rdr[0].ToString() };
                        Requests.Add(request);
                    }

                    connection.Close();
                    return;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
