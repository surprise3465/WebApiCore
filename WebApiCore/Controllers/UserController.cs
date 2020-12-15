using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using WebApiCore.Models;
using WebApiCore.Services;
using WebApiCore.Entities;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using WebApiCore.Helpers;

namespace WebApiCore.Controllers
{
    public abstract class BaseController : ControllerBase
    {
        protected User GetCurrentUser()
        {
            var currentUser = (User)HttpContext.Items["User"];
            return currentUser;
        }
    }

    [ApiController]
    [Route("[controller]")]
    public class UsersController : BaseController
    {
        private IUserService _userService;
        private IMapper _mapper;

        public UsersController(IUserService userService, IMapper mapper)
        {
            _mapper = mapper;
            _userService = userService;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] UserModel UserModel)
        {
            // map dto to entity
            var user = _mapper.Map<User>(UserModel);

            try
            {
                // save 
                _userService.Create(user, UserModel.Password);
                return Ok();
            }
            catch (AppException ex)
            {
                // return error message if there was an exception
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("authenticate")]
        public IActionResult Authenticate(AuthenticateRequest model)
        {
            var response = _userService.Authenticate(model);

            if (response == null)
                return BadRequest(new { message = "Username or password is incorrect" });

            return Ok(response);
        }

        [Authorize]
        [HttpGet]
        public IActionResult GetAll()
        {
            var currentUser = GetCurrentUser();
            var users = _userService.GetAll();
            return Ok(users);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] UserModel UserModel)
        {
            // map dto to entity and set id
            var user = _mapper.Map<User>(UserModel);
            user.Id = id;

            try
            {
                // save 
                _userService.Update(user, UserModel.Password);
                return Ok();
            }
            catch (AppException ex)
            {
                // return error message if there was an exception
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _userService.Delete(id);
            return Ok();
        }
    }
}
