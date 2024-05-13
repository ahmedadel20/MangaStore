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

namespace MangaStore.Pages.Admin.Books
{
    [RequireAuth(RequiredRole = "admin")]
    public class Create : PageModel
    {
        [BindProperty]
        [Required(ErrorMessage = "The Title is required")]
        [MaxLength (100, ErrorMessage = "The Title cannot exceed 100 characters")]
        public string Title { get; set; } = "";

        [BindProperty]
        [Required(ErrorMessage = "The Author is required")]
        [MaxLength (255, ErrorMessage = "The Authors cannot exceed 255 characters")]
        public string Authors { get; set; } = "";

        [BindProperty]
        [Required(ErrorMessage = "The ISBN is required")]
        [MaxLength (20, ErrorMessage = "The ISBN cannot exceed 20 characters")]
        public string Isbn { get; set; } = "";

        [BindProperty]
        [Required(ErrorMessage = "The Number of pages are required")]
        [Range (1, 10000, ErrorMessage = "The Number of pages cannot be more than 10000")]
        public int Pages { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "The Price is required")]
        public decimal Price { get; set; }

        [BindProperty, Required]
        public string Category { get; set; } = "";

        [BindProperty]
        [MaxLength (1000, ErrorMessage = "The Description cannot exceed 1000 characters")]
        public string? Description { get; set; } = "";

        [BindProperty]
        [Required(ErrorMessage = "The Image is required")]
        public IFormFile ImageFile { get; set; }

        public string errorMessage = "";
        public string successMessage = "";

        private readonly IWebHostEnvironment webHostEnvironment;

        public Create(IWebHostEnvironment env)
        {
            webHostEnvironment = env;
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

            //succesfull data validation

            Description ??= "";

            //save the image file on the server

            string newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            newFileName += Path.GetExtension(ImageFile.FileName);

            string imageFolder = webHostEnvironment.WebRootPath + "/images/books/";

            string imageFullPath = Path.Combine(imageFolder, newFileName);

            using(var stream = System.IO.File.Create(imageFullPath))
            {
                ImageFile.CopyTo(stream);
            }

            //save the new book in the database

            try
            {
                string connectionString = "Data Source=.\\SQLEXPRESS; Initial Catalog=MangaStore; Integrated Security=True;";
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = "INSERT INTO books " +
                    "(Title, Authors, ISBN, Pages, Price, Category, Description, Image_filename) VALUES " +
                    "(@Title, @Authors, @Isbn, @Pages, @Price, @Category, @Description, @Image_filename);";

                    using(SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Title", Title);
                        command.Parameters.AddWithValue("@Authors", Authors);
                        command.Parameters.AddWithValue("@Isbn", Isbn);
                        command.Parameters.AddWithValue("@Pages", Pages);
                        command.Parameters.AddWithValue("@Price", Price);
                        command.Parameters.AddWithValue("@Category", Category);
                        command.Parameters.AddWithValue("@Description", Description);
                        command.Parameters.AddWithValue("@Image_filename", newFileName);

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception e)
            {
                errorMessage = e.Message;
                return;
            }

            successMessage = "Data saved correctly";
            Response.Redirect("/Admin/Books/Index");
        }
    }
}