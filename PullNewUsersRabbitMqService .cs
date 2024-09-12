
using System.Text;
using System.Text.Json;
using MongoDB.Driver;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Users;

namespace WebApi2.BackgroudServices;

public class PullNewUsersRabbitMqService : IHostedService
{
    public static List<User> Users { get; set; }

    private readonly ConnectionFactory rabbitMqConnectionFactory;
    private readonly IConnection connection;
    private readonly IModel model;
    private const string QUEUE_NAME = "messages";

    static PullNewUsersRabbitMqService() {
        Users = new List<User>();
    }

    public PullNewUsersRabbitMqService()
    {
        this.rabbitMqConnectionFactory = new ConnectionFactory()
        {
            HostName = "rabbitmq_app",
            UserName = "azureuser",
            Password = "Password123!"
        };

        this.connection = this.rabbitMqConnectionFactory.CreateConnection();
        this.model = connection.CreateModel();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var result = this.model.QueueDeclare(
            queue: QUEUE_NAME,
            durable: true,
            exclusive: false,
            autoDelete: false
        );

        var consumer = new EventingBasicConsumer(this.model);

        consumer.Received += async (sender, deliverEventArgs) =>
        {
            string? newUserJson = null;

            try {
                newUserJson = Encoding.ASCII.GetString(deliverEventArgs.Body.ToArray());

                var newUser = JsonSerializer.Deserialize<User>(newUserJson)!;

                const string connectionString = "mongodb://mongodb:27017";

                var client = new MongoClient(connectionString);

                var database = client.GetDatabase("MyDatabase");

                var collection = database.GetCollection<User>("InfoDb");


                await collection.InsertOneAsync(newUser);

            }
            catch(Exception ex) {
                System.Console.WriteLine($"Couldn't pull new user: '{ex}' | Body: {newUserJson}");
            }
        };

        this.model.BasicConsume(
            queue: QUEUE_NAME,
            autoAck: true,
            consumer: consumer
        );

    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}