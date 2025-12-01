using Automacao.Services;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Configura MongoDB
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var config = builder.Configuration.GetSection("MongoDb");
    return new MongoClient(config["ConnectionString"]);
});

builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var mongoClient = sp.GetRequiredService<IMongoClient>();
    var config = builder.Configuration.GetSection("MongoDb");
    return mongoClient.GetDatabase(config["DatabaseName"]);
});

// Registra os serviços personalizados
builder.Services.AddSingleton<MongoService>();
builder.Services.AddSingleton<MqttService>();
builder.Services.AddSingleton<PowerBiService>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<MqttService>());

// MVC e Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();


app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
