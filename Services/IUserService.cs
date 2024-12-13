namespace AuthServiceNamespace.Services
{
    public interface IUserService
    {
        Task EditProfile(string userId, EditProfileDto editProfileDto);

    }
}