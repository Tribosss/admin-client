using admin_client.Model;
using DotNetEnv;
using MySql.Data.MySqlClient;
using RabbitMQ.Client;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;

namespace admin_client.ViewModel
{
    class SetPolicyViewModel
    {
        public ObservableCollection<UserData> SearchUserList { get; } = new ObservableCollection<UserData>();
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
        private bool _isIndividualAgentActive = false;
        public bool IsIndividualAgentActive
        {
            get => _isIndividualAgentActive;
            set
            {
                if (_isIndividualAgentActive == value) return;
                _isIndividualAgentActive = value;
                OnPropertyChanged();

                if (_isIndividualAgentActive)
                {
                    PublishMessageAtClient("AGENT<ON>");
                } else
                {
                    PublishMessageAtClient("AGENT<OFF>");
                }
            }
        }
        private bool _isIndividualDomainBlockActive = false;
        public bool IsIndividualDomainBlockActive
        {
            get => _isIndividualDomainBlockActive;
            set
            {
                if (_isIndividualDomainBlockActive == value) return;
                _isIndividualDomainBlockActive = value;
                OnPropertyChanged();

                if (_selectedUser == null) return;

                if (!_isIndividualDomainBlockActive)
                {
                    PublishMessageAtClient("DOMAIN<CLEAR>");
                } else
                {
                    List<string> blockedDomains = GetBlockedDomain(_selectedUser.Id);
                    for (int i = 0; i < blockedDomains.Count; i++) PublishMessageAtClient($"DOMAIN<{blockedDomains[i]}>");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        public SetPolicyViewModel()
        {

        }

        private List<string> GetBlockedDomain(string empId)
        {
            string query = $@"select domain from blocked_domains where emp_id = '{empId}';";

            string? dbHost, dbPort, dbUid, dbPwd, dbName;
            string dbConnection;
            List<string> blockedDomains = [];

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

                    while (rdr.Read())
                    {
                        blockedDomains.Add(rdr[0].ToString());
                    }

                    connection.Close();
                    return blockedDomains;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return null;
            }
        }

        public void LoadUserListByKeyword(string keyword)
        {
            if (string.IsNullOrEmpty(keyword)) return;

            SearchUserList.Clear();

            string query = "select e.id, e.name, r.position, p.is_active_agent, p.is_active_domain_block " +
                "from employees e ";
            query += "inner join role r on r.id=e.role_id ";
            query += "inner join policys p on p.emp_id=e.id ";
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
                        bool isActiveAgent = (bool)rdr[3];    
                        bool isActiveDomainBlock = (bool)rdr[4];    

                        UserData uData = new UserData
                        {
                            Id = id,
                            Name = name,
                            Position = position,
                            IsActiveAgent = isActiveAgent,
                            IsActiveDomainBlock = isActiveDomainBlock
                        };

                        SearchUserList.Add(uData);
                    }

                    SelectedUser = null;
                    connection.Close();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void SetIsActiveAgentByToggleButton(bool isActiveAgent)
        {
            if (SelectedUser == null || SelectedUser.Id == null) return;
            string query = $@"
                update policys
                set is_active_agent={isActiveAgent}
                where emp_id={SelectedUser.Id}";

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
                    if (!rdr.Read()) return;

                    if (cmd.ExecuteNonQuery() == 1)
                    {
                        Console.WriteLine("Success Update");
                    }
                    else
                    {
                        Console.WriteLine("Failed Update");
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
            if (_selectedUser == null || _selectedUser.Id == null) return;

            string exchangeName = "tribosss";
            string rountingKey = $"policy.set.{SelectedUser.Id}";
            ConnectionFactory factory = new ConnectionFactory()
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest",
                ClientProvidedName = $"[{SelectedUser.Id}]"
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
            Console.WriteLine($"Published Message: {msg}");

            channel.CloseAsync();
            conn.CloseAsync();
        }
    }
}
