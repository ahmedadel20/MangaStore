using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using MangaStore.MyHelpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace MangaStore.Pages.Auth
{
    [RequireNoAuth]
    public class ResetPassword : PageModel
    {
        private readonly ILogger<ResetPassword> _logger;

        public ResetPassword(ILogger<ResetPassword> logger)
        {
            _logger = logger;
        }

        [BindProperty, Required(ErrorMessage = "Password is required")]
        [StringLength(15, ErrorMessage = "Password must be between 5 and 15 characters", MinimumLength = 5)]
        public string Password { get; set; } = "";

        [BindProperty, Required(ErrorMessage = "Confirm Password is required")]
        [Compare("Password", ErrorMessage = "Password and the Confirm Password do not match")]
        public string ConfirmPassword { get; set; } = "";

        public string errorMessage = "";
        public string successMessage = "";

        public void OnGet()
        {
            string token = Request.Query["token"];
            if(string.IsNullOrEmpty(token))
            {
                Response.Redirect("/");
                return;
            }
        }

        public void OnPost()
        {
            string token = Request.Query["token"];
            if(string.IsNullOrEmpty(token))
            {
                Response.Redirect("/");
                return;
            }

            if(!ModelState.IsValid)
            {
                errorMessage = "Data is not Valid";
                return;
            }


            //connect to the database to update the password

            try
            {
                string connectionString = "Data Source=.\\SQLEXPRESS; Initial Catalog=MangaStore; Integrated Security=True;";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string email = "";
                    string sql = "SELECT * FROM password_resets WHERE token=@token;";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@token", email);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if(reader.Read())
                            {
                                email = reader.GetString(0);
                            }
                            else
                            {
                                errorMessage = "Wrong or Expired Token";
                                return;
                            }
                        }
                    }

                    //2nd step

                    var passwordHasher = new PasswordHasher<IdentityUser>();
                    string hashedPassword = passwordHasher.HashPassword(new IdentityUser(), Password);

                    sql = "UPDATE users SET password=@password WHERE email=@email";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@password", hashedPassword);
                        command.Parameters.AddWithValue("@email", email);

                        command.ExecuteNonQuery();
                    }

                    //3rd step
                    sql = "DELETE FROM password_resets WHERE email=@email";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@email", email);

                        command.ExecuteNonQuery();
                    }
                }

            }

            catch (Exception e)
            {
                errorMessage = e.Message;
                return;
            }

            successMessage = "Password changed successfully";
        }
    }
}