using Microsoft.AspNetCore.Identity;

public enum UserRole { Admin, StockKeeper, Salesman, Logistic }

public class User : IdentityUser
{
    public UserRole Role { get; set; }
    public DateTime LastLogin { get; set; }

}
