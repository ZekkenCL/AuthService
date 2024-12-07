using Microsoft.AspNetCore.Mvc;
using AuthServiceNamespace.Services;
using AuthServiceNamespace.Models.Dtos;

namespace AuthServiceNamespace.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TokenController : ControllerBase
    {
        private readonly ITokenService _tokenService;

        public TokenController(ITokenService tokenService)
        {
            _tokenService = tokenService;
        }

        [HttpPost("validate-token")]
        public IActionResult ValidateToken([FromBody] TokenValidationDto validationDto)
        {
            var isValid = _tokenService.ValidateToken(validationDto.Token);
            if (isValid)
                return Ok(new { message = "Token is valid" });
            
            return Unauthorized(new { message = "Token is invalid or revoked" });
        }

        [HttpPost("revoke-token")]
        public IActionResult RevokeToken([FromBody] TokenValidationDto revokeDto)
        {
            _tokenService.RevokeToken(revokeDto.Token);
            return Ok(new { message = "Token has been revoked" });
        }
    }
}
