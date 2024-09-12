using MongoDB.Driver;
using Users;
using WebApi2.BackgroudServices;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHostedService<PullNewUsersRabbitMqService>();

var app = builder.Build();


    app.UseSwagger();
    app.UseSwaggerUI();





app.MapGet("/Users", async () =>
{
                const string connectionString = "mongodb://mongodb:27017";

                var client = new MongoClient(connectionString);

                var database = client.GetDatabase("MyDatabase");

                var collection = database.GetCollection<User>("InfoDb");

                var findAllQuery = collection.Find(phoneInfo => true);

                var phoneInfos = await findAllQuery.ToListAsync();
})
.WithOpenApi();

app.Run();





