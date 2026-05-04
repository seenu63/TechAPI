using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using TechAPI.DBContext;
using TechAPI.Services;

namespace TechAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : Controller
    {
        private readonly IUserService _IUserService;
        private readonly IStudentService _IStudentService;
        
        public UserController(IUserService oIUserService , IStudentService oIStudentService) {
            _IUserService = oIUserService;
            _IStudentService = oIStudentService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> UserLogin([FromBody] dynamic json)
        {
            var result  = await _IUserService.LoginAsync(json);            
            return Ok(result);
        }    

        [HttpPost("logout")]
        public string login()
        {
            _IStudentService.get();
            return "success";
        }

        
        [HttpPost("student")]
        public async Task<IActionResult> Student([FromBody] dynamic json)
        {
            var result = await _IUserService.GetStudent(json);
            return Ok(result);
        }



        [HttpPost("addstudent")]
        public async Task<IActionResult> AddStudent([FromBody] dynamic json)
        {
            var result = await _IUserService.AddStudentData(json);
            return Ok(result);
        }


        [Authorize(Roles = "Student")]
        [HttpGet("admin")]
        public IActionResult AdminPanel()
        {
            return Ok("Welcome Admin!");
        }

    }
}

