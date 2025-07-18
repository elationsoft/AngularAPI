using Dapper;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace minimalCrudAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly string _superbaseConnection;
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _superbaseConnection = configuration.GetConnectionString("SuperbaseDB");
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpGet("api/GetSuperbaseData")]
        public IActionResult GetSuperbaseData()
        {
            using (var connection = new NpgsqlConnection(_superbaseConnection))
            {
                connection.Open();
                var todos = connection.Query<dynamic>("Select *from \"DemoTable\"").ToList();
                return Ok(new { success = true, result = todos });
            }
        }
        [HttpPost("api/SuperBaseAddUser")]
        public IActionResult SuperBaseAddUser(string name, int rollno, int mobile)
        {
            using (var connection = new NpgsqlConnection(_superbaseConnection))
            {
                connection.Open();
                var query = @"INSERT INTO ""DemoTable"" 
              (created_at, ""stdName"", ""stdRoll"", ""stdMobile"")
              VALUES (now(), @stdName, @stdRoll, @stdMobile)";
                var parameters = new
                {
                    stdName = name,
                    stdRoll = rollno,
                    stdMobile = mobile
                };
                connection.Execute(query, parameters);
                return Ok(new { success = true });
            }
        }

        [HttpPut("api/SuperBaseUpdateUser")]
        public IActionResult SuperBaseUpdateUser(int id, string name, int rollno, int mobile)
        {
            using (var connection = new NpgsqlConnection(_superbaseConnection))
            {
                connection.Open();
                var query = @"UPDATE ""DemoTable"" 
              SET ""stdName"" = @stdName, ""stdRoll"" = @stdRoll, ""stdMobile"" = @stdMobile
              WHERE id = @id";
                var parameters = new
                {
                    stdName = name,
                    stdRoll = rollno,
                    stdMobile = mobile,
                    id
                };
                connection.Execute(query, parameters);
                return Ok(new { success = true });
            }
        }


        [HttpDelete("api/SuperBaseDeleteUser")]
        public IActionResult SuperBaseDeleteUser(int id)
        {
            using (var connection = new NpgsqlConnection(_superbaseConnection))
            {
                connection.Open();
                var query = @"DELETE FROM ""DemoTable"" WHERE id = @id";
                connection.Execute(query, new { id });
                return Ok(new { success = true });
            }
        }

    }
}
