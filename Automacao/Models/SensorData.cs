using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Automacao.Models
{
    public class SensorData
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Nome { get; set; } = string.Empty;
        public int Volume { get; set; }
        public int Temperatura { get; set; }
        public bool Status { get; set; }
        public DateTime Data { get; set; } = DateTime.UtcNow;
    }
}
