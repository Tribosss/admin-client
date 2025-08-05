using admin_client.Model;
using DotNetEnv;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            LoadLogs();
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
        public ObservableCollection<Log> Logs { get; } = new ObservableCollection<Log>();
        private void LoadLogs()
        {
            Logs.Clear();

            string query = "select s.emp_id, s.msg, s.source_ip, s.source_port, s.dest_ip, s.dest_port, s.detected_at" +
                "   from employees e ";
            query += "inner join suspicion_logs s on s.emp_id=e.id ";
            query += "inner join role r on r.id=e.role_id ";
            query += "order by s.detected_at desc;";

            string? dbHost, dbPort, dbUid, dbPwd, dbName;
            string msg, source, destination, detectedAt, targetId;
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
                        targetId = rdr[0].ToString()!;
                        msg = rdr[1].ToString()!;
                        source = $"{rdr[2].ToString()}:{rdr[3].ToString()}";
                        destination = $"{rdr[4].ToString()}:{rdr[5].ToString()}";
                        detectedAt = rdr[6].ToString()!;

                        Logs.Add(new Log() { TargetId = targetId, Msg = msg, Source = source, Destination = destination, DetectedAt = detectedAt });
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
    