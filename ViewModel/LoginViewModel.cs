using admin_client.Model;
using DotNetEnv;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace admin_client.ViewModel
{
    class LoginViewModel
    {
        public bool ValidAdminInfo(AdminAuth auth)
        {
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

                    string selectQuery = "select e.id, e.password from employees as e ";
                    selectQuery += "inner join role r on r.id=e.role_id ";
                    selectQuery += $"where r.id=1 and e.id='{auth.LoginId}' and e.password='{auth.Password}' ";
                    selectQuery += "limit 1;";

                    MySqlCommand cmd = new MySqlCommand(selectQuery, connection);
                    MySqlDataReader rdr = cmd.ExecuteReader();
                    if (rdr == null) return false;
                    if (!rdr.Read()) return false;

                    string currentLoginId = rdr[0].ToString()!;
                    string currentPassword = rdr[1].ToString()!;

                    if (currentLoginId == null || currentPassword == null) return false;

                    AdminAuth currentAdminAuth = new AdminAuth
                    {
                        LoginId = currentLoginId,
                        Password = currentPassword,
                    };

                    connection.Close();
                    return true;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
        }
        public UserData GetUserData(AdminAuth auth)
        {
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

                    string selectQuery = "select e.id, e.name, r.position from employees as e ";
                    selectQuery += "inner join role r on r.id=e.role_id ";
                    selectQuery += $"where r.id=1 and e.id='{auth.LoginId}' and e.password='{auth.Password}' ";
                    selectQuery += "limit 1;";

                    MySqlCommand cmd = new MySqlCommand(selectQuery, connection);
                    MySqlDataReader rdr = cmd.ExecuteReader();
                    if (rdr == null) throw new Exception("MySqlDataReader is null");
                    if (!rdr.Read()) throw new Exception("MySqlDataReader is empty");

                    string currentId = rdr[0].ToString()!;
                    string currentName = rdr[1].ToString()!;
                    string currentPosition = rdr[2].ToString()!;

                    if (currentId == null || currentName == null || currentPosition == null) return null;

                    UserData userData = new UserData()
                    {
                        Id = currentId,
                        Name = currentName,
                        Position = currentPosition
                    };
                    return userData;
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
