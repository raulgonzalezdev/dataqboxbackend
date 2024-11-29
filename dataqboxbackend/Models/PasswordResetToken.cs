public class PasswordResetToken
{
    public int Id { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public DateTime Expiration { get; set; }
}
