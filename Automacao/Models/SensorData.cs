using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Automacao.Models
{
    public class SensorData
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
