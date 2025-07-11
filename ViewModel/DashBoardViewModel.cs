using DotNetEnv;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace admin_client.ViewModel
{
    public class DashBoardViewModel
    {
        public DashBoardViewModel()
        {
            LoadCountDatas();
        }
        private string _staffCount;
        public string StaffCount
        {
            get => _staffCount;
            set { _staffCount = value; RaisePropertyChanged(); }
        }

        private string _boardCount;
        public string BoardCount
        {
            get => _boardCount;
            set { _boardCount = value; RaisePropertyChanged(); }
        }

        private string _outflowCount;
        public string OutflowCount
        {
            get => _outflowCount;
            set { _outflowCount = value; RaisePropertyChanged(); }
        }

        private string _incursionCount;
        public string IncursionCount
        {
            get => _incursionCount;
            set { _incursionCount = value; RaisePropertyChanged(); }
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
        private void LoadCountDatas()
        {
            string query = "select " +
                "(select count(id) from employees)," +
                "(select count(id) from posts)," +
                "sum(`type`='outflow')," +
                "sum(`type`='incursion')" +
                "from suspicion_logs;";

            string? host, port, uid, pwd, name;
            string dbConnection;

            try
            {
                Env.Load();


                host = Environment.GetEnvironmentVariable("DB_HOST");
                if (host == null) throw new Exception(".env DB_HOST is null");
                port = Environment.GetEnvironmentVariable("DB_PORT");
                if (port == null) throw new Exception(".env DB_PORT is null");
                uid = Environment.GetEnvironmentVariable("DB_UID");
                if (uid == null) throw new Exception(".env DB_UID is null"); ;
                pwd = Environment.GetEnvironmentVariable("DB_PWD");
                if (pwd == null) throw new Exception(".env DB_PWD is null");
                name = Environment.GetEnvironmentVariable("DB_NAME");
                if (name == null) throw new Exception(".env DB_NAME is null");

                dbConnection = $"Server={host};Port={port};Database={name};Uid={uid};Pwd={pwd}";

                using (MySqlConnection connection = new MySqlConnection(dbConnection))
                {
                    connection.Open();

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    MySqlDataReader rdr = cmd.ExecuteReader();
                    if (rdr == null) return;
                    if (!rdr.Read()) return;
                    
                    _staffCount = rdr[0].ToString()!;
                    _boardCount = rdr[1].ToString()!;
                    _outflowCount = rdr[2].ToString()!;
                    _incursionCount = rdr[3].ToString()!;

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
    