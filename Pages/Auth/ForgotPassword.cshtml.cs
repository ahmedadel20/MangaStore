using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using MangaStore.MyHelpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace MangaStore.Pages.Auth
{
    [RequireNoAuth]
    [BindProperties]
    public class ForgotPassword : PageModel
    {
        [Required(ErrorMessage = "The Email Address is required"), EmailAddress]
        public string email { get; set;}

        public string errorMessage = "";
        public string successMessage = "";

        private readonly ILogger<ForgotPassword> _logger;

        public ForgotPassword(ILogger<ForgotPassword> logger)
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

            // 1) Create Token, 2) Save the token in the database, 3) Send the token by email to the user

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
                                string firstName = reader.GetString(1);
                                string lastName = reader.GetString(2);

                                string token = Guid.NewGuid().ToString();

                                SaveToken(email, token);

                                // send confirmation email to the user
                                string resetURL = Url.PageLink("/Auth/ResetPassword") + "?token=" + token;
                                string userName = firstName + " " + lastName;
                                string subject = "Password Reset.";
                                string message = "Dear " + userName + ",\n\n" + "You can reset your password by visiting the following link" + resetURL +",\n\n";
                                EmailSender.SendEmail(email, userName, subject, message).Wait();
                            }
                            else
                            {
                                errorMessage = "Email does not exist";
                                return;
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
            successMessage = "Please Check your email and click on the Password reset link";
        }

        public void SaveToken(string email, string token)
        {
            try
            {
                string connectionString = "Data Source=.\\SQLEXPRESS; Initial Catalog=MangaStore; Integrated Security=True;";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "DELETE FROM password_resets WHERE email=@email;";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@email", email);

                        command.ExecuteNonQuery();
                    }

                    sql = "INSERT INTO password_resets (email, token) VALUES (@email, @token);";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@email", email);
                        command.Parameters.AddWithValue("@token", token);

                        command.ExecuteNonQuery();
                    }

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}