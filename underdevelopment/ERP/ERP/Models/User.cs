public enum UserRole { Admin, StockKeeper, Salesman }

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty; 
    public UserRole Role { get; set; }
    public DateTime LastLogin { get; set; }

    // Függvények
    public bool HasPermission(UserRole requiredRole) { return Role == requiredRole; }
    public void ChangePassword(string newPassword) { /* hash-elés után tárolni */ }
}
