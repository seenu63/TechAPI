using Azure.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using TechAPI.DBContext;
using TechAPI.Email;

namespace TechAPI.Services
{
    public interface IUserService 
    {
        Task<object> LoginAsync(dynamic json);
        Task<object> GetStudent(dynamic json);
        Task<object> AddStudentData(dynamic json);
    }
    public class UserService : IUserService
    {
        private readonly IDBContextService _IDBContextService;
        private readonly IConfiguration _Configuration;
        private readonly IEmailService _emailService;


        public string strSPName = "";
        public UserService(IDBContextService oIDBContextService, IConfiguration oConfiguration, IEmailService oEmailService)
        {
            _IDBContextService = oIDBContextService;
            _Configuration = oConfiguration;
            _emailService = oEmailService;
        }
        public async Task<object> LoginAsync(dynamic json)
        {
            JsonElement root = json;
            string? strUserID = root.GetProperty("request").GetProperty("userid").GetString();
            string? strPassword = root.GetProperty("request").GetProperty("password").GetString();


            strSPName = "xa_auth_login";
            var allTables = await _IDBContextService.Execute(strSPName,
               new
               {
                   @sUserid = strUserID,
                   @sPassword = strPassword
               }
               );

            var status = allTables[0].FirstOrDefault(); 
                               
            var Result = new object();

            if (status.Status == "1")
            {
                var client = allTables[1].FirstOrDefault();
                var Token = GenerateToken(strUserID, client.Role, client.RowID);
                object student = allTables[1];
                if (client.Role?.ToLower() == "student")
                {
                    object marks = allTables[2];
                    object attendance = allTables[3];
                    Result = new { status, Token, student, marks, attendance };
                }
                else
                {
                    Result = new { status, Token, student };
                }
            }
            else if(status.Status == "0")
                Result = new { status };
            

            return Result;
        }
        private string GenerateToken(string email, string role, int userId)
        {
            var jwtSettings = _Configuration.GetSection("Jwt");

            var claims = new[]
            {
            new Claim(ClaimTypes.Name, email),
            new Claim(ClaimTypes.Role, role),
            new Claim("UID", userId.ToString()) };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(jwtSettings["DurationInMinutes"])),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        public async Task<object> GetStudent(dynamic json)
        {
            JsonElement root = json;

            string? strID = null;
            string? strClass = null;
            string? strMonth = null;
            string? strYear = null;
            string? strType = null;
            string? strPeriod = null;

            if (root.TryGetProperty("request", out var req))
            {
                strID = req.GetString("id");
                strClass = req.GetString("class");
                strMonth = req.GetString("month");
                strYear = req.GetString("year");
                strType = req.GetString("type");
                strPeriod = req.GetString("period");
            }

            strSPName = "xa_get_data";
            var allTables = await _IDBContextService.Execute(strSPName,
               new
               {
                   @sSUID  = strID,
                   @iClass = strClass,
                   @sMonth = strMonth,
                   @sYear  = strYear,
                   @sType  = strType,
                   @sPeriod= strPeriod
               }
               );

            var status = allTables;

            var Result = new object();
        
            Result = new { status };


            return Result;
        }

        public async Task<object> AddStudentData(dynamic json)
        {
            JsonElement root = json;

            string? strJSON = null;
            string? strOperation = null;
            string? strType = null;
            

            if (root.TryGetProperty("request", out var req))
            {
                strJSON = req.GetString("student");
                strOperation = req.GetString("operation");   
                strType = req.GetString("type");
            }

            strSPName = "xa_student_action";
            var allTables = await _IDBContextService.Execute(strSPName,
               new
               {
                   @sJSON = strJSON,
                   @sMode = strOperation ,
                   @sType = strType
               }
               );

            if (strType == "attendance")
            {

                var node = JsonNode.Parse(strJSON);

                List<(string Name, string Email)> students = node!.AsArray()
                    .Where(x => (int)x!["Present"]! == 0)
                    .Select(x => (
                        Name: x!["StudentName"]!.ToString(),
                        Email: x!["Email"]!.ToString()
                    ))
                    .ToList();

               if (students.Count > 0)
               await _emailService.SendAsync(students);
            }

            var status = allTables;

            var Result = new object();

            Result = new { status };

            return Result;
        }


    }

}
