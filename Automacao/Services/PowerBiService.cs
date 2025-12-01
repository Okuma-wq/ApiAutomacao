using System.Text;
using System.Text.Json;

namespace Automacao.Services
{
    public class PowerBiService
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _http;
        private readonly ILogger<PowerBiService> _logger;
        private readonly string? _url;
        private readonly bool _enabled;

        public PowerBiService(IConfiguration config, ILogger<PowerBiService> logger)
        {
            _config = config;
            _logger = logger;
            _http = new HttpClient();

            _url = _config["PowerBI:StreamingUrl"];
            _enabled = _config.GetValue<bool>("PowerBI:Enabled");
        }

        public async Task SendAsync(object data)
        {
            if (!_enabled || string.IsNullOrWhiteSpace(_url))
                return;

            try
            {
                var json = JsonSerializer.Serialize(new[] { data });
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _http.PostAsync(_url, content);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("⚠️ PowerBI streaming falhou: {Status}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar dados para Power BI");
            }
        }
    }
}
