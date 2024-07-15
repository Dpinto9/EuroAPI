using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MySqlConnector;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MinhaApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IsController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly string _jwtKey;
        private readonly IConfiguration _configuration;

        public IsController(MyDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _jwtKey = _configuration["Jwt:Key"];
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] Conta conta)
        {
            var existingUser = _context.Contas.FirstOrDefault(u => u.Username == conta.Username && u.Password == conta.Password);
            if (existingUser == null) return Unauthorized();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, existingUser.Username)
            };

            if (!string.IsNullOrEmpty(existingUser.Role))
            {
                claims.Add(new Claim(ClaimTypes.Role, existingUser.Role));
            }

            var token = GenerateJwtToken(claims);
            return Ok(new { Token = token });
        }

        [HttpGet("getDataWithoutRole")]
        [Authorize(Roles = "Exame25")]
        public IActionResult GetDataWithoutRole()
        {
            var results = new List<Dictionary<string, object>>();
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new MySqlCommand("SELECT * FROM Contas", connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var row = new Dictionary<string, object>();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                row[reader.GetName(i)] = reader.GetValue(i);
                            }
                            results.Add(row);
                        }
                    }
                }
            }

                    return Ok(results);
        }

        [HttpGet("getDataWithRole")]
        [Authorize(Roles = "Exame25")]
        public IActionResult GetDataWithRole()
        {
            var results = new List<Dictionary<string, object>>();
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new MySqlCommand("SELECT * FROM Estadios", connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var row = new Dictionary<string, object>();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                row[reader.GetName(i)] = reader.GetValue(i);
                            }
                            results.Add(row);
                        }
                    }
                }
            }

            return Ok(results);
        }

        private string GenerateJwtToken(IEnumerable<Claim> claims)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
