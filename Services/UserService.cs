using Microsoft.EntityFrameworkCore;

namespace AuthServiceNamespace.Services
{
    public class UserService : IUserService
    {
        private readonly AuthDbContext _context;

        public UserService(AuthDbContext context)
        {
            _context = context;
        }

        public async Task EditProfile(string userId, EditProfileDto editProfileDto)
        {
            // Validar que el userId sea un número válido
            if (!int.TryParse(userId, out var userIdInt))
            {
                throw new ArgumentException("Invalid userId format");
            }

            // Buscar el usuario
            var user = await _context.Users.SingleOrDefaultAsync(u => u.UserId == userIdInt);

            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found");
            }

            // Validar los campos del DTO
            if (string.IsNullOrWhiteSpace(editProfileDto.FirstName) ||
                string.IsNullOrWhiteSpace(editProfileDto.LastName) ||
                string.IsNullOrWhiteSpace(editProfileDto.SecondLastName))
            {
                throw new ArgumentException("All fields are required");
            }

            // Actualizar el usuario
            user.FirstName = editProfileDto.FirstName;
            user.LastName = editProfileDto.LastName;
            user.SecondLastName = editProfileDto.SecondLastName;

            // Guardar los cambios
            await _context.SaveChangesAsync();
        }
    }
}
