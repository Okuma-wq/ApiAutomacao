using Automacao.Services;
using Microsoft.AspNetCore.Mvc;

namespace Automacao.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SensorController : ControllerBase
{
    private readonly MongoService _mongo;
    private readonly MqttService _mqtt;

    public SensorController(MongoService mongo, MqttService mqtt)
    {
        _mongo = mongo;
        _mqtt = mqtt;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var dados = await _mongo.GetAllAsync();
        return Ok(dados);
    }

    //[HttpPost("comando/{comando}")]
    //public async Task<IActionResult> EnviarComando([FromRoute] string comando)
    //{
    //    await _mqtt.PublishAsync(comando);
    //    return Ok("Comando enviado via MQTT");
    //}
}
