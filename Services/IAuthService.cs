namespace AuthServiceNamespace.Services
{
    public interface IAuthService
    {
        Task<LoginResponseDto> Login(LoginRequestDto loginRequestDto);
        Task<LoginResponseDto> Register(RegisterStudentDto registerStudentDto);
        Task UpdatePassword(string userId,UpdatePasswordDto updatePasswordDto);
    }
}
