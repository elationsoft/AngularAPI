using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Dapper;
using System.Data.SqlClient;
using WebAPIsAngular.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Npgsql;
using System.Numerics;

namespace WebAPIsAngular.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TodoController : ControllerBase
    {
        private readonly string _dapperConnection;
        private readonly string _superbaseConnection;
        private readonly IConfiguration _config;
        private readonly TokenService _tokenService;
        public TodoController(IConfiguration config, TokenService tokenService)
        {
            _dapperConnection = config.GetConnectionString("AngularDbCon");
            _superbaseConnection = config.GetConnectionString("SuperbaseDB");
            _config = config;
            _tokenService = tokenService;
        }
        // GET: api/todo
        [HttpGet("api/todoIndex")]
       public IActionResult Index(int id)
       {    using (var connection = new SqlConnection(_dapperConnection))
            {
                connection.Open();
                string query = $"SELECT top 10 FROM tbl_Selling where u_id<={id}";
                var todos = connection.Query<dynamic>(query).ToList();
                return Ok(todos);
            }

       }


        [Authorize]
        [HttpGet("secure-data")]
        public IActionResult GetSecureData()
        {
            var email = User.FindFirstValue(JwtRegisteredClaimNames.Email);
            return Ok($"This is protected data for {email}");
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] User login)
        {
            using var connection = new SqlConnection(_config.GetConnectionString("AngularDbCon"));

            var user = await connection.QueryFirstOrDefaultAsync<User>(
                "SELECT u_id as Id,Email as Email,password as password FROM tbl_userProfile WHERE Email = @Email", new { login.Email });

            if (user == null || user.password != login.password)
            {
                return Unauthorized("Invalid credentials");
            }
            var token = _tokenService.GenerateToken(user.Id, user.Email);
            return Ok(new { token });
        }

        [HttpGet("api/todoSelling")]
        public IActionResult GetSellingData()
        {
            using (var connection = new SqlConnection(_dapperConnection))
            {
                connection.Open();
                var todos = connection.Query<dynamic>("SELECT * FROM tbl_Selling").ToList();
                return Ok(new { success = true, result = todos });
            }
        }
        [HttpPost("api/SaveSelling")]
        public IActionResult SaveSelling(string msg,int Rid)
        {
            using (var connection = new SqlConnection(_dapperConnection))
            {
                connection.Open();
                string query = $"insert into tbl_review (review_id,u_id,stars,MsgReview,sell_id,selling_email,reviewTime) values({Rid},1,3,'{msg}',2,'2',GETDATE())";
                var rowaffected = connection.Execute(query);
                return Ok(new { success = true, result = rowaffected });
            }
        }

        [HttpPut("api/UpdateReply")]
        public IActionResult UddateReply(string msg, int Rid)
        {
            using (var connection = new SqlConnection(_dapperConnection))
            {
                connection.Open();
                string query = $"UPDATE tbl_review SET replyTo='@msg' WHERE review_id=@Rid";
                var rowaffected = connection.Execute(query, new { msg, Rid });
                return Ok(new { success = true, result = rowaffected });
            }
        }
        //=================================================SUPERBASE========================================================

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
        public IActionResult SuperBaseAddUser(string name,int rollno,int mobile)
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
                return Ok(new { success = true});
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

    public class UserDetails
    {
        public string?Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
    }

}
