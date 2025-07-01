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
                if (host == null) return false;
                port = Environment.GetEnvironmentVariable("DB_PORT");
                if (port == null) return false;
                uid = Environment.GetEnvironmentVariable("DB_UID");
                if (uid == null) return false;
                pwd = Environment.GetEnvironmentVariable("DB_PWD");
                if (pwd == null) return false;
                name = Environment.GetEnvironmentVariable("DB_NAME");
                if (name == null) return false;

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

                    return true;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
        }
    }
}
