using System.Text.Json.Serialization;

namespace Automacao.Models
{
    public class AlertaCommand
    {
        [JsonPropertyName("lubrifica_maquina")]
        public bool LubrificaMaquina { get; set; }
        
        [JsonPropertyName("excesso_descarte")]
        public bool ExcessoDescarte { get; set; }
        
        [JsonPropertyName("alta_temperatura")]
        public bool AltaTemperatura { get; set; }
    }
}