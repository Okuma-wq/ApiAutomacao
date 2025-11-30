namespace Automacao.Models
{
    public class SensorData
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Tipo { get; set; } = string.Empty;
        public double Valor { get; set; }
        public DateTime DataHora { get; set; } = DateTime.UtcNow;
    }
}
