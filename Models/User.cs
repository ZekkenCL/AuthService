public class User
{
    public int UserId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string SecondLastName { get; set; }
    public string RUT { get; set; }
    public string Email { get; set; }
    public int CareerId { get; set; }
    public string PasswordHash { get; set; }
    public string PasswordSalt { get; set; }
}
