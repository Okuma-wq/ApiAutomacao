using System.Text;
using System.Text.Json;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Formatter;
using MongoDB.Driver;

namespace Automacao.Services
{
    public class MqttService : IHostedService
    {
        private readonly ILogger<MqttService> _logger;
        private readonly IMongoCollection<MqttMessage> _collection;
        private readonly IConfiguration _config;
        private IMqttClient? _client;
        private MqttClientOptions _options = null!;

        public MqttService(ILogger<MqttService> logger, IConfiguration config, IMongoDatabase database)
        {
            _logger = logger;
            _config = config;

            var mqttSection = _config.GetSection("Mqtt");
            var broker = mqttSection.GetValue<string>("Broker");
            var port = mqttSection.GetValue<int>("Port");
            var username = mqttSection.GetValue<string>("Username");
            var password = mqttSection.GetValue<string>("Password");
            var clientIdPrefix = mqttSection.GetValue<string>("ClientIdPrefix");
            var topicSubscribe = mqttSection.GetValue<string>("TopicSubscribe");
            var useTls = mqttSection.GetValue<bool>("UseTls");

            _collection = database
                .GetCollection<MqttMessage>(_config["MongoDb:CollectionName"]);

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

                var message = new MqttMessage
                {
                    Topic = e.ApplicationMessage.Topic,
                    Payload = payload,
                    Timestamp = DateTime.UtcNow
                };

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

    public class MqttMessage
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Topic { get; set; } = string.Empty;
        public string Payload { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}
