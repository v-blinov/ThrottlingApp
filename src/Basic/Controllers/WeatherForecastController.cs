using Basic.Models;
using Microsoft.AspNetCore.Mvc;

namespace Basic.Controllers;

[Route("weather")]
public class WeatherForecastController : Controller
{
    private static readonly string[] Summaries = { "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching" };

    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

    [HttpGet("get")]
    public ActionResult<IEnumerable<WeatherForecast>> Get()
    {
        return Ok(Enumerable.Range(1, 5)
                            .Select(index => new WeatherForecast
                            {
                                Date = DateTime.Now.AddDays(index), 
                                TemperatureC = Random.Shared.Next(-20, 55), 
                                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
                            })
                            .ToArray());
    }
    
    [HttpGet("get2")]
    public ActionResult<IEnumerable<WeatherForecast>> Get2()
    {
        return Ok(Enumerable.Range(1, 5)
                            .Select(index => new WeatherForecast
                            {
                                Date = DateTime.Now.AddDays(index), 
                                TemperatureC = Random.Shared.Next(-20, 55), 
                                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
                            })
                            .ToArray());
    }
    
    [HttpGet("get3")]
    public ActionResult<IEnumerable<WeatherForecast>> Get3()
    {
        return Ok(Enumerable.Range(1, 5)
                            .Select(index => new WeatherForecast
                            {
                                Date = DateTime.Now.AddDays(index), 
                                TemperatureC = Random.Shared.Next(-20, 55), 
                                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
                            })
                            .ToArray());
    }
}
