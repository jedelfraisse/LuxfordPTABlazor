// Quick utility to generate ASP.NET Core Identity password hash
// Run this in LINQPad or create a console app

using Microsoft.AspNetCore.Identity;
using LuxfordPTAWeb.Shared.Models;

var hasher = new PasswordHasher<ApplicationUser>();
var user = new ApplicationUser { UserName = "jonathan@delfraisse.com" };

// Hash the password "Admin123!" or whatever you want
var hashedPassword = hasher.HashPassword(user, "Admin123!");

Console.WriteLine("Password Hash:");
Console.WriteLine(hashedPassword);
Console.WriteLine();
Console.WriteLine("SQL Command:");
Console.WriteLine($"UPDATE AspNetUsers SET PasswordHash = '{hashedPassword}', SecurityStamp = NEWID(), EmailConfirmed = 1 WHERE Email = 'jonathan@delfraisse.com';");
