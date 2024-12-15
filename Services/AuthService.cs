using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace AuthServiceNamespace.Services
{
    public class AuthService : IAuthService
    {
        private readonly AuthDbContext _context;
        private readonly RabbitMQPublisher _rabbitMQPublisher;

        public AuthService(AuthDbContext context, RabbitMQPublisher rabbitMQPublisher)
        {
            _context = context;
            _rabbitMQPublisher = rabbitMQPublisher;
        }

     public async Task<LoginResponseDto> Login(LoginRequestDto loginRequestDto)
{
    var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == loginRequestDto.Email);


    if (user == null || !BCrypt.Net.BCrypt.Verify(loginRequestDto.Password, user.PasswordHash))
    {
        throw new UnauthorizedAccessException("Invalid credentials");
    }


    return new LoginResponseDto
    {
        Id = user.UserId,
        Token = GenerateJwtToken(user)
    };
}

        public async Task<LoginResponseDto> Register(RegisterStudentDto registerStudentDto)
{
    // Generar el hash de la contraseña
    var passwordSalt = BCrypt.Net.BCrypt.GenerateSalt();
    var passwordHash = BCrypt.Net.BCrypt.HashPassword(registerStudentDto.Password, passwordSalt);

    // Crear el usuario
    var user = new User
    {
        FirstName = registerStudentDto.FirstName,
        LastName = registerStudentDto.LastName,
        SecondLastName = registerStudentDto.SecondLastName,
        RUT = registerStudentDto.RUT,
        Email = registerStudentDto.Email,
        CareerId = registerStudentDto.CareerId,
        PasswordHash = passwordHash,
        PasswordSalt = passwordSalt
    };

    // Guardar el usuario en la base de datos
    _context.Users.Add(user);
    await _context.SaveChangesAsync();

    // Preparar el mensaje para la cola
    var userMessage = new
    {
        UserId = user.UserId,
        FirstName = user.FirstName,
        LastName = user.LastName,
        SecondLastName = user.SecondLastName,
        Email = user.Email,
        CareerId = user.CareerId
    };

    // Convertir el mensaje a JSON
    var messageJson = JsonSerializer.Serialize(userMessage);

    // Publicar el mensaje en la cola RabbitMQ
    try
    {
        _rabbitMQPublisher.Publish(messageJson, "user_queue", "user.register");
        Console.WriteLine($"Message published to RabbitMQ: {messageJson}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error publishing message to RabbitMQ: {ex.Message}");
        // Manejar el error, si es necesario
    }

    // Retornar la respuesta con el ID del usuario
    return new LoginResponseDto
    {
        Id = user.UserId
    };
}

public async Task UpdatePassword(string userId, UpdatePasswordDto updatePasswordDto)
{
    var user = await _context.Users.SingleOrDefaultAsync(u => u.UserId.ToString() == userId);
    if (user == null)
    {
        throw new UnauthorizedAccessException("User not found");
    }

    if (!BCrypt.Net.BCrypt.Verify(updatePasswordDto.CurrentPassword, user.PasswordHash))
    {
        throw new UnauthorizedAccessException("Invalid current password");
    }

    if (updatePasswordDto.NewPassword != updatePasswordDto.ConfirmPassword)
    {
        throw new ArgumentException("New password and confirmation password do not match");
    }

    var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(updatePasswordDto.NewPassword, user.PasswordSalt);
    user.PasswordHash = newPasswordHash;

    await _context.SaveChangesAsync();

    var NewPassword = new
    {
        UserId = user.UserId,
        NewPassword = user.PasswordHash
    };

    var messageJson = JsonSerializer.Serialize(NewPassword);

    try
    {
        _rabbitMQPublisher.Publish(messageJson, "password_queue", "user.updatePassword");
        Console.WriteLine($"Message published to RabbitMQ: {messageJson}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error publishing message to RabbitMQ: {ex.Message}");
    }
}





        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET"));
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Email, user.Email)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
