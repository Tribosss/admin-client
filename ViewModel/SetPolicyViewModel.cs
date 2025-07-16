using admin_client.Model;
using DotNetEnv;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Utilities;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace admin_client.ViewModel
{
    class SetPolicyViewModel
    {
        public ObservableCollection<UserData> SearchUserList { get; } = new ObservableCollection<UserData>();
        public UserData selectedUser;
        private UserData _selectedUser;
        public UserData SelectedUser
        {
            get => _selectedUser;
            set
            {
                if (_selectedUser == value) return;
                _selectedUser = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        public SetPolicyViewModel()
        {

        }

        public void LoadUserListByKeyword(string keyword)
        {
            if (string.IsNullOrEmpty(keyword)) return;

            SearchUserList.Clear();

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

                        SearchUserList.Add(uData);
                    }

                    connection.Close();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public async Task PublishMessageAtClient(string msg)
        {
            string exchangeName = "tribosss";
            string rountingKey = "policy.set";
            ConnectionFactory factory = new ConnectionFactory()
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest",
                ClientProvidedName = $"[12345678]"
            };
            IConnection conn = await factory.CreateConnectionAsync();
            IChannel channel = await conn.CreateChannelAsync();
            await channel.ExchangeDeclareAsync(exchangeName,
                ExchangeType.Direct,
                durable: true,
                autoDelete: false,
                arguments: null
            );

            byte[] msgBodyBytes = Encoding.UTF8.GetBytes(msg);
            BasicProperties props = new BasicProperties();
            await channel.BasicPublishAsync(
                exchange: exchangeName,
                routingKey: rountingKey,
                mandatory: true,
                basicProperties: props,
                body: msgBodyBytes
            );
            Console.WriteLine("Published Message");

            channel.CloseAsync();
            conn.CloseAsync();
        }
    }
}
