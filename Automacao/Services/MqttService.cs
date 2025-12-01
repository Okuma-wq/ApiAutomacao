using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Formatter;
using System.Text;
using System.Text.Json;

namespace Automacao.Services
{
    public class MqttService : IHostedService
    {
        private readonly ILogger<MqttService> _logger;
        private readonly IMongoCollection<DadosMaquina> _collection;
        private readonly PowerBiService _powerBiService;
        private readonly IConfiguration _config;
        private IMqttClient? _client;
        private MqttClientOptions _options = null!;

        public MqttService(ILogger<MqttService> logger, IConfiguration config, IMongoDatabase database, PowerBiService powerBi)
        {
            _logger = logger;
            _config = config;
            _powerBiService = powerBi;

            var mqttSection = _config.GetSection("Mqtt");
            var broker = mqttSection.GetValue<string>("Broker");
            var port = mqttSection.GetValue<int>("Port");
            var username = mqttSection.GetValue<string>("Username");
            var password = mqttSection.GetValue<string>("Password");
            var clientIdPrefix = mqttSection.GetValue<string>("ClientIdPrefix");
            var topicSubscribe = mqttSection.GetValue<string>("TopicSubscribe");
            var useTls = mqttSection.GetValue<bool>("UseTls");

            _collection = database
                .GetCollection<DadosMaquina>(_config["MongoDb:CollectionName"]);

            var factory = new MqttFactory();
            _client = factory.CreateMqttClient();

            _options = new MqttClientOptionsBuilder()
                .WithClientId($"{clientIdPrefix}-{Guid.NewGuid()}")
                .WithTcpServer(broker, port)
                .WithCredentials(username, password)
                .WithProtocolVersion(MqttProtocolVersion.V500)
                .WithTlsOptions(o =>
                {
                    o.UseTls(true);
                })
                .Build();

            _client.ApplicationMessageReceivedAsync += HandleMessageReceivedAsync;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _client!.ConnectAsync(_options, cancellationToken);
            _logger.LogInformation("✅ Conectado ao broker MQTT.");

            var topic = _config["Mqtt:TopicSubscribe"];
            await _client.SubscribeAsync(topic);
            _logger.LogInformation($"📡 Subscrito ao tópico '{topic}'.");
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_client != null)
            {
                await _client.DisconnectAsync(cancellationToken: cancellationToken);
                _logger.LogInformation("🔌 Desconectado do broker MQTT.");
            }
        }

        private async Task HandleMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)
        {
            try
            {
                var payload = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);
                _logger.LogInformation($"📥 Mensagem recebida: {payload}");

                var message = JsonSerializer.Deserialize<DadosMaquina>(payload);

                var lista = new List<DadosMaquina>();
                lista.Add(message!);

                await _powerBiService.SendAsync(message!);

                await _collection.InsertOneAsync(message);
                _logger.LogInformation("💾 Mensagem salva no MongoDB.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar mensagem MQTT");
            }
        }

        public async Task PublishAsync(string comando)
        {
            var topic = _config["Mqtt:TopicPublish"];
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(comando)
                .Build();

            await _client!.PublishAsync(message);
            _logger.LogInformation($"📤 Comando publicado: {comando}");
        }
    }

    public class DadosMaquina
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public int Volume { get; set; }
        public int Temp { get; set; }
        public string Maquina { get; set; } = string.Empty;
        public DateTime Data { get; set; } = DateTime.UtcNow;
    }
}
