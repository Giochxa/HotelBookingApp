﻿using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using HotelBookingApp.DTOs;
using HotelBookingApp.Services;

namespace HotelBookingApp.Controllers.Api
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var result = await _authService.AuthenticateUser(loginDto);
            if (result == null)
                return Unauthorized(new { message = "Invalid credentials" });

            return Ok(result);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            var result = await _authService.RegisterUser(registerDto);
            if (!result)
                return BadRequest(new { message = "User registration failed" });

            return Ok(new { message = "User registered successfully" });
        }
    }
}
