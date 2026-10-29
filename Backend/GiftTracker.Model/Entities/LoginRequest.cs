namespace GiftTracker.Model.Entities;

// Not a table: the email and password sent to POST /api/Profile/login
public class LoginRequest
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
}
