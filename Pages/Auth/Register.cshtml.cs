using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using MangaStore.MyHelpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace MangaStore.Pages.Auth
{
    [RequireNoAuth]
    [BindProperties]
    public class Register : PageModel
    {
        [Required(ErrorMessage = "The First Name is required")]
        public string firstName { get; set; } ="";

        [Required(ErrorMessage = "The Last Name is required")]
        public string lastName { get; set; } ="";

        [Required(ErrorMessage = "The Email is required"), EmailAddress]
        public string email { get; set; } ="";

        public string? phone { get; set; } ="";

        [Required(ErrorMessage = "The Address is required")]
        public string address { get; set; } ="";

        [Required(ErrorMessage = "The Password is required")]
        [StringLength(50, ErrorMessage = "The Password must be between 5 and 50 characters", MinimumLength = 5)]
        public string password { get; set; } ="";

        [Required(ErrorMessage = "Confirm Password is required")]
        [Compare("password", ErrorMessage = "The Password and Confirmation Password do not match")]
        public string confirmPassword { get; set; } ="";

        public string errorMessage = "";
        public string successMessage = "";

        private readonly ILogger<Register> _logger;

        public Register(ILogger<Register> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }

        public void OnPost()
        {
            if(!ModelState.IsValid)
            {
                errorMessage = "Data is not Valid";
                return;
            }

            //successful data validation
            phone ??="";

            // add user to the database
            string connectionString = "Data Source=.\\SQLEXPRESS; Initial Catalog=MangaStore; Integrated Security=True;";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "INSERT INTO users " + 
                    "(firstname, lastname, email, phone, address, password, role) VALUES " +
                    "(@firstname, @lastname, @email, @phone, @address, @password, 'client');";

                    var passwordHasher = new PasswordHasher<IdentityUser>();
                    string hashedPassword = passwordHasher.HashPassword(new IdentityUser(), password);

                    using(SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@firstname", firstName);
                        command.Parameters.AddWithValue("@lastname", lastName);
                        command.Parameters.AddWithValue("@email", email);
                        command.Parameters.AddWithValue("@phone", phone);
                        command.Parameters.AddWithValue("@address", address);
                        command.Parameters.AddWithValue("@password", hashedPassword);

                        command.ExecuteNonQuery();
                    }
                }

            }
            catch (Exception e)
            {
                if(e.Message.Contains(email))
                {
                    errorMessage = "Email already exists";
                }
                else
                {
                    errorMessage = e.Message;
                }
                return;
            }

            // send confirmation email to the user
            string userName = firstName + " " + lastName;
            string subject = "Account created successfully";
            string message = "Dear " + userName + ",\n\n" + "Best Regards";
            EmailSender.SendEmail(email, userName, subject, message).Wait();

            // initialize the authenticated session => add the user details to the session data

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "SELECT * FROM users WHERE email=@email";
                    using(SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@email", email);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if(reader.Read())
                            {
                                int id = reader.GetInt32(0);
                                string firstName = reader.GetString(1);
                                string lastName = reader.GetString(2);
                                string email = reader.GetString(3);
                                string phone = reader.GetString(4);
                                string address = reader.GetString(5);
                                //string hashedPassword = reader.GetString(6);
                                string role = reader.GetString(7);
                                string created_on = reader.GetDateTime(8).ToString("MM/dd/yyyy");


                                HttpContext.Session.SetInt32("id", id);
                                HttpContext.Session.SetString("firstName", firstName);
                                HttpContext.Session.SetString("lastName", lastName);
                                HttpContext.Session.SetString("email", email);
                                HttpContext.Session.SetString("phone", phone);
                                HttpContext.Session.SetString("address", address);
                                HttpContext.Session.SetString("role", role);
                                HttpContext.Session.SetString("created_on", created_on);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                errorMessage = e.Message;
                return;
            }

            successMessage = "Account Created Successfully";

            //redirect to the homepage
            Response.Redirect("/");
        }
    }
}