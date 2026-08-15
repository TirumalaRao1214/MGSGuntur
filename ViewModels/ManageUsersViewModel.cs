using System.ComponentModel.DataAnnotations;
using MarwadiGheeSweetsWeb.Models;

namespace MarwadiGheeSweetsWeb.ViewModels;

public class ManageUsersViewModel
{
    public List<AppUser> Users          { get; set; } = new();
    public AddUserViewModel AddForm     { get; set; } = new();
    public string? SuccessMessage       { get; set; }
    public string? ErrorMessage         { get; set; }
}

public class AddUserViewModel
{
    [Required] public string Username    { get; set; } = string.Empty;
    [Required] public string DisplayName { get; set; } = string.Empty;
    [Required] public string Role        { get; set; } = "Admin";

    [Required, MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
