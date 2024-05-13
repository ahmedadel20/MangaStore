using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public class Login : PageModel
    {
        [Required(ErrorMessage = "The Email is required"), EmailAddress]
        public string email { get; set; } = "";

        [Required(ErrorMessage = "The Password is required"), PasswordPropertyText]
        public string password { get; set; } = "";

        public string errorMessage = "";
        public string successMessage = "";

        private readonly ILogger<Login> _logger;

        public Login(ILogger<Login> logger)
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

            //successfull data validation

            //connect to the database to check the user credentials
            try
            {
                string connectionString = "Data Source=.\\SQLEXPRESS; Initial Catalog=MangaStore; Integrated Security=True;";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "SELECT * FROM users WHERE email=@email;";

                    using (SqlCommand command = new SqlCommand(sql, connection))
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
                                string hashedPassword = reader.GetString(6);
                                string role = reader.GetString(7);
                                string created_on = reader.GetDateTime(8).ToString("MM/dd/yyyy");

                                var passwordHasher = new PasswordHasher<IdentityUser>();
                                var result = passwordHasher.VerifyHashedPassword(new IdentityUser(), hashedPassword, password);

                                if(result == PasswordVerificationResult.Success || result == PasswordVerificationResult.SuccessRehashNeeded)
                                {
                                    // succesfull login => initialize the session
                                    HttpContext.Session.SetInt32("id", id);
                                    HttpContext.Session.SetString("firstName", firstName);
                                    HttpContext.Session.SetString("lastName", lastName);
                                    HttpContext.Session.SetString("email", email);
                                    HttpContext.Session.SetString("phone", phone);
                                    HttpContext.Session.SetString("address", address);
                                    HttpContext.Session.SetString("role", role);
                                    HttpContext.Session.SetString("created_on", created_on);

                                    // the user is authenticated successfully => redirect to the homepage
                                    Response.Redirect("/");
                                }
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

            //wrong password or wrong password

            errorMessage = "Wrong Email or Password";

        }
    }
}