using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace admin_client.ViewModel
{
    class SetPolicyViewModel
    {
        public SetPolicyViewModel()
        {
            PublishMessageAtClient();
        }

        private async Task PublishMessageAtClient()
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

            byte[] msgBodyBytes = Encoding.UTF8.GetBytes("HelloRabbitMQ");
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
