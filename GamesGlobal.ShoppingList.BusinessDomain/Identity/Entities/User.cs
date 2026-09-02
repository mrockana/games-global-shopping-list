using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DataAccess;
using Microsoft.AspNetCore.Identity;

namespace GamesGlobal.ShoppingList.BusinessDomain.Identity.Entities;

public sealed class User : BaseEntity
{
    [Key]
    public long UserId { get; set; }

    public Guid UserCode { get; set; }

    [Required]
    public string? FirstName { get; set; }

    [Required]
    public string? LastName { get; set; }

    [Required]
    public string? Email { get; set; }

    [Required]
    public string? Password { get; set; }

    public ICollection<Role> Roles { get; set; } = new Collection<Role>();

    public void SetUserPassword(string password)
    {
        var hashedPassword = new PasswordHasher<User>().HashPassword(this, password);
        Password = hashedPassword;
    }

    public bool VerifyUserPassword(string password)
    {
        var hasher = new PasswordHasher<User>();
        var verificationResult = hasher.VerifyHashedPassword(this, Password!, password);
        return verificationResult == PasswordVerificationResult.Success;
    }
}