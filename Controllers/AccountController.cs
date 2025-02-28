﻿using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NewsPortal_App.Models;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NewsPortal_App.Controllers
{
    public class AccountController : Controller
    {
        private readonly IConfiguration _configuration;

        public AccountController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // GET: /Account/SignIn
        public IActionResult SignIn()
        {
            if (HttpContext.Session.GetInt32("UserId") != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();

        }
        // POST: /Account/SignIn
        // POST: /Account/SignIn
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignIn(SignInViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please fill all required fields correctly.";
                return View(model);
            }

            try
            {
                // Admin credentials check
                if (model.Email == "ryadav943@rku.ac.in" && model.Password == "Admin")
                {
                    await SetAdminSessionAsync();
                    TempData["SuccessMessage"] = "Admin login successful!";
                    return RedirectToAction("Index", "Dashboard");
                }

                // Database se user check
                using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                await conn.OpenAsync();
                string query = @"
            SELECT UserID, Username, FullName, Email, ProfileImagePath, Password 
            FROM Users 
            WHERE Email = @Email";
                using var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Email", model.Email);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    string storedPassword = reader["Password"]?.ToString();
                    if (!string.IsNullOrEmpty(storedPassword) && storedPassword == HashPassword(model.Password))
                    {
                        await SetUserSessionAsync(reader);
                        TempData["SuccessMessage"] = "Welcome back! Login successful.";
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Invalid email or password.";
                        return View(model);
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Invalid email or password.";
                    return View(model);
                }
            }
            catch (SqlException ex)
            {
                TempData["ErrorMessage"] = "Database error occurred. Please try again later.";
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred. Please try again.";
                return View(model);
            }
        }
        // GET: /Account/SignUp
        [HttpGet]
        public IActionResult SignUp()
        {
            return View();
        }

        // POST: /Account/SignUp
        // POST: /Account/SignUp
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignUp(SignUpViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please fill all required fields correctly.";
                return View(model);
            }

            try
            {
                using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                await conn.OpenAsync();

                // Check for existing user
                string checkQuery = "SELECT COUNT(*) FROM Users WHERE Username = @Username OR Email = @Email";
                using var checkCmd = new SqlCommand(checkQuery, conn);
                checkCmd.Parameters.AddWithValue("@Username", model.Username);
                checkCmd.Parameters.AddWithValue("@Email", model.Email);
                int existingCount = (int)await checkCmd.ExecuteScalarAsync();
                if (existingCount > 0)
                {
                    TempData["ErrorMessage"] = "Username or email already exists.";
                    return View(model);
                }

                // Insert new user with corrected ProfileImagePath
                string insertQuery = @"
            INSERT INTO Users (Username, Email, Password, FullName, ProfileImagePath, CreatedAt) 
            VALUES (@Username, @Email, @Password, @FullName, @ProfileImagePath, @CreatedAt)";
                using var insertCmd = new SqlCommand(insertQuery, conn);
                insertCmd.Parameters.AddWithValue("@Username", model.Username);
                insertCmd.Parameters.AddWithValue("@Email", model.Email);
                insertCmd.Parameters.AddWithValue("@Password", HashPassword(model.Password));
                insertCmd.Parameters.AddWithValue("@FullName", model.Username); // FullName default as Username
                insertCmd.Parameters.AddWithValue("@ProfileImagePath", "/images/Avatar.png"); // Changed to /images/Avatar.png
                insertCmd.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);

                await insertCmd.ExecuteNonQueryAsync();

                TempData["SuccessMessage"] = "Registration successful! Please sign in to continue.";
                return RedirectToAction("SignIn");
            }
            catch (SqlException ex)
            {
                TempData["ErrorMessage"] = "A database error occurred. Please try again later.";
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred. Please try again.";
                return View(model);
            }
        }
        // GET: /Account/Logout
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            HttpContext.Session.Clear();
            return RedirectToAction("SignIn");
        }

        // Helper Methods

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        private async Task SetAdminSessionAsync()
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Name, "Admin"),
                new Claim(ClaimTypes.Email, "ryadav943@rku.ac.in"),
                new Claim("FullName", "Administrator"),
                new Claim("ProfileImagePath", "~/images/admin.png")
            };

            var identity = new ClaimsIdentity(claims, "custom");
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync("custom", principal);

            // Session for backward compatibility
            HttpContext.Session.SetInt32("UserId", 1);
            HttpContext.Session.SetString("UserEmail", "ryadav943@rku.ac.in");
            HttpContext.Session.SetString("Username", "Admin");
            HttpContext.Session.SetString("FullName", "Administrator");
            HttpContext.Session.SetString("UserProfileImage", "~/images/admin.png");
        }

        private async Task SetUserSessionAsync(SqlDataReader reader)
        {
            var userId = reader.GetInt32(reader.GetOrdinal("UserID")).ToString();
            var username = reader["Username"]?.ToString() ?? string.Empty;
            var fullName = reader["FullName"]?.ToString() ?? string.Empty;
            var email = reader["Email"]?.ToString() ?? string.Empty;
            var profileImagePath = reader.HasColumn("ProfileImagePath") && !reader.IsDBNull(reader.GetOrdinal("ProfileImagePath"))
                ? reader["ProfileImagePath"].ToString()
                : "~/images/avatar.png";

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Email, email),
                new Claim("FullName", fullName),
                new Claim("ProfileImagePath", profileImagePath)
            };

            var identity = new ClaimsIdentity(claims, "custom");
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync("custom", principal);

            // Session for backward compatibility
            HttpContext.Session.SetInt32("UserId", int.Parse(userId));
            HttpContext.Session.SetString("Username", username);
            HttpContext.Session.SetString("FullName", fullName);
            HttpContext.Session.SetString("Email", email);
            HttpContext.Session.SetString("UserProfileImage", profileImagePath);
        }
    }

    public static class SqlDataReaderExtensions
    {
        public static bool HasColumn(this SqlDataReader reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
    }
}