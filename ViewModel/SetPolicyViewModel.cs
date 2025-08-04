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
        public ObservableCollection<Domain> IndividualBlockDomains { get; } = new ObservableCollection<Domain>();
        public ObservableCollection<Domain> DefaultBlockDomains { get; } = new ObservableCollection<Domain>();

        private UserData _selectedUser;
        public UserData SelectedUser
        {
            get => _selectedUser;
            set
            {
                if (_selectedUser == value) return;
                _selectedUser = value;
                OnPropertyChanged();
                IndividualBlockDomains.Clear();
                LoadIndividualBlockedDomains();
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

        private bool _isDefaultAgentActive = false;
        public bool IsDefaultAgentActive
        {
            get => _isDefaultAgentActive;
            set
            {
                if (_isDefaultAgentActive == value) return;
                _isDefaultAgentActive = value;
                OnPropertyChanged();
            }
        }
        private bool _isDefaultDomainBlockActive = false;
        public bool IsDefaultDomainBlockActive
        {
            get => _isDefaultDomainBlockActive;
            set
            {
                if (_isDefaultDomainBlockActive == value) return;
                _isDefaultDomainBlockActive = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        public SetPolicyViewModel()
        {
            IndividualBlockDomains.Clear();
            DefaultBlockDomains.Clear();
            LoadDefaultBlockDomains();
            LoadDefaultPolicys();
        }

        private void LoadDefaultPolicys()
        {
            string query = $@"SELECT COLUMN_NAME, COLUMN_DEFAULT
                FROM INFORMATION_SCHEMA.COLUMNS
                where TABLE_SCHEMA = 'tribosss' 
                    AND TABLE_NAME = 'policys'
                    AND (
                        COLUMN_NAME = 'is_active_agent'
                        or COLUMN_NAME = 'is_active_domain_block'
                    ); ";

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
                        string columnName = rdr.GetString("COLUMN_NAME");
                        string columnDefault = rdr.GetString("COLUMN_DEFAULT");

                        if (columnName == "is_active_agent")
                        {
                            IsDefaultAgentActive = columnDefault == "1";
                        }
                        else if (columnName == "is_active_domain_block")
                        {
                            IsDefaultDomainBlockActive = columnDefault == "1";
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        public void ToggleDefaultBlockDomain(bool isActive)
        {
            string query = $@"ALTER TABLE policys
                MODIFY COLUMN is_active_domain_block TINYINT(1) NOT NULL DEFAULT {isActive};";
            QueryingWithSQL(query,
                $"Success: Toggle Default Domain Block ({isActive})",
                $"Fail: Toggle Default Domain Block ({isActive})"
            );
        }
        public void ToggleDefaultAgent(bool isActive)
        {
            string query = $@"ALTER TABLE policys
                MODIFY COLUMN is_active_agent TINYINT(1) NOT NULL DEFAULT {isActive};";
            QueryingWithSQL(query,
                $"Success: Toggle Default Agent ({isActive})",
                $"Fail: Toggle Default Agent ({isActive})"
            );
        }
        public void InsertBlockDomain(string domain)
        {
            string query = $@"insert into blocked_domains(emp_id, domain)
                values('{_selectedUser.Id}', '{domain}');";
            QueryingWithSQL(query, "Success: Insert Individual Domain", "Fail: Insert Individual Domain");
        }
        public void AddBlockDomain(string domain)
        {
            Domain d = new Domain() { DomainName = domain };
            IndividualBlockDomains.Add(d);
        }
        public void InsertDefaultBlockDomain(string domain)
        {
            string query = $@"insert into default_block_domains(domain)
                values('{domain}');";
            QueryingWithSQL(query, 
                "Success: Insert Default doamin",
                "Fail: Insert Default Domains"
            );
        }
        public void AddDefaultBlockDomain(string domain)
        {
            Domain d = new Domain() { DomainName = domain };
            DefaultBlockDomains.Add(d);
        }
        public void RemoveIndividualDomain(string domain)
        {
            if (SelectedUser == null) return;
            string query = $@"delete from blocked_domains where domain='{domain}' and emp_id='{SelectedUser.Id}';";
            QueryingWithSQL(query,
                "Success: Remove Individual Domain",
                "Fail: Remove Indiviudal Domain"
            );
            for (int i = 0; i < IndividualBlockDomains.Count; i++)
            {
                if (IndividualBlockDomains[i].DomainName == domain)
                {
                    IndividualBlockDomains.RemoveAt(i);
                    break;
                }
            }
        }
        public void RemoveDefaultDomain(string domain)
        {
            string query = $@"delete from default_block_domains where domain='{domain}';";
            QueryingWithSQL(query,
                "Success: Remove Default domain",
                "Fail: Remove Default Domain"
            );
            for (int i = 0; i < DefaultBlockDomains.Count; i++)
            {
                if (DefaultBlockDomains[i].DomainName == domain)
                {
                    DefaultBlockDomains.RemoveAt(i);
                    break;
                }
            }
        }
        private void LoadIndividualBlockedDomains()
        {
            if (SelectedUser == null) return;

            string query = $@"select domain from blocked_domains where emp_id={SelectedUser.Id};";

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
                        Domain domain = new Domain() { DomainName = rdr[0].ToString() };
                        IndividualBlockDomains.Add(domain);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        private void LoadDefaultBlockDomains()
        {
            string query = $@"select domain from default_block_domains;";

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
                        Domain domain = new Domain() { DomainName = rdr[0].ToString() };
                        DefaultBlockDomains.Add(domain);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void QueryingWithSQL(string query, string successMsg, string failureMsg)
        {
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

                    if (cmd.ExecuteNonQuery() >= 0)
                    {
                        Console.WriteLine(successMsg);
                    }
                    else
                    {
                        Console.WriteLine(failureMsg);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
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
