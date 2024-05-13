using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using MangaStore.MyHelpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;

namespace MangaStore.Pages
{
    public class ContactModel : PageModel
    {
        private readonly ILogger<ContactModel> _logger;

        public ContactModel(ILogger<ContactModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }

        [BindProperty]
        [Display(Name = "First Name*")]
        [Required(ErrorMessage = "The First Name is Required")]
        public string FirstName { get; set; } = "";

        [BindProperty]
         [Display(Name = "Last Name*")]
        [Required(ErrorMessage = "The Last Name is Required")]
        public string LastName { get; set; } = "";

        [BindProperty]
        [EmailAddress]
         [Display(Name = "Email*")]
        [Required(ErrorMessage = "The Email is Required")]
        public string Email { get; set; } = "";

        [BindProperty]
        public string? Phone { get; set; } = "";

        [BindProperty]
        [Required]
         [Display(Name = "Subject*")]
        public string Subject { get; set; } = "";

        [BindProperty]
         [Display(Name = "Message*")]
        [Required(ErrorMessage = "You have to provide a valid Message")]
        [MinLength(5, ErrorMessage = "The Message must be at least 5 characters")]
        [MaxLength(1024, ErrorMessage = "The Message must be at most 1024 characters")]
        public string Message { get; set; } = "";

        public List<SelectListItem> SubjectList { get;} = new List<SelectListItem>
        {
            new SelectListItem { Value = "Order Status", Text = "Order Status"},
            new SelectListItem { Value = "Refund Request", Text = "Refund Request"},
            new SelectListItem { Value = "Job Application", Text = "Job Application"},
            new SelectListItem { Value = "Other", Text = "Other"},
        };

        
        public string SuccessMessage { get; set; } = "";
        public string ErrorMessage { get; set; } = "";

        public void OnPost()
        {
            if(!ModelState.IsValid)
            {
                // Error
                ErrorMessage = "Please Fill all required fields";
                return;
            }
            
            if(Phone == null) Phone ="";

            try
            {
                string connectionString = "Data Source=.\\SQLEXPRESS; Initial Catalog=MangaStore; Integrated Security=True;";
                // ; Connect Timeout=30; Encrypt=False; TrustServerCertificate=False; ApplicationIntent=ReadWrite; MultiSubnetFailover=False
                using(SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "INSERT INTO messages " +
                      "(firstname, lastname, email, phone, message) VALUES " +
                      "(@firstname, @lastname, @email, @phone, @message);";

                    SqlCommand command = new SqlCommand(sql, connection);                    
                    command.Parameters.AddWithValue("@firstname", FirstName);
                    command.Parameters.AddWithValue("@lastname", LastName);
                    command.Parameters.AddWithValue("@email", Email);
                    command.Parameters.AddWithValue("@phone", Phone);
                    command.Parameters.AddWithValue("@message", Message);
                    command.BeginExecuteNonQuery();
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return; 
            }

            string username = FirstName + " " + LastName;
            string emailSubject = "About Your Message";
            string emailMessage = "Dear " + username + ",\n\n" + "We have received your Message \n"+ "Best Regards \n" + "Your Message: " + Message;
            EmailSender.SendEmail(Email, username, emailSubject, emailMessage).Wait();

            // Send Confirmation Email to the Client

           SuccessMessage = "Your message has been received successfully.";

            FirstName = "";
            LastName = "";
            Email = "";
            Phone = "";
            Subject = "";
            Message = "";

            ModelState.Clear();
        }
    }
}