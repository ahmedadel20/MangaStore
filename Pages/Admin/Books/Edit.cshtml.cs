using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
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
    public class Edit : PageModel
    {
        [BindProperty]
        public int ID { get; set; }

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
        public string Image_filename { get; set;} = "";

        [BindProperty]
        public IFormFile? ImageFile { get; set; }


        public string errorMessage = "";
        public string successMessage = "";

        private IWebHostEnvironment webHostEnvironment;

        public Edit(IWebHostEnvironment env)
        {
            webHostEnvironment = env;
        }

        public void OnGet()
        {
            string requestID = Request.Query["id"];
            try
            {
                string connectionString = "Data Source=.\\SQLEXPRESS; Initial Catalog=MangaStore; Integrated Security=True;";
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = "SELECT * FROM books WHERE ID=@ID";
                    using(SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@ID", requestID);
                        using(SqlDataReader reader = command.ExecuteReader())
                        {
                            if(reader.Read())
                            {
                                ID = reader.GetInt32(0);
                                Title = reader.GetString(1);
                                Authors = reader.GetString(2);
                                Isbn = reader.GetString(3);
                                Pages = reader.GetInt32(4);
                                Price = reader.GetDecimal(5);
                                Category = reader.GetString(6);
                                Description = reader.GetString(7);
                                Image_filename = reader.GetString(8);
                            }
                            else
                            {
                                Response.Redirect("/Admin/Books/Index");
                            }
                        }
                    }
                    //"(Title, Authors, ISBN, Pages, Price, Category, Description, Image_filename) VALUES " +
                    //"(@Title, @Authors, @Isbn, @Pages, @Price, @Category, @Description, @Image_filename);";
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Response.Redirect("/Admin/Books/Index");
            }
        }

        public void OnPost()
        {
            if(!ModelState.IsValid)
            {
                errorMessage = "Data is not Valid";
                return;
            }

            //successfull data validation

            Description ??= "";

            // if theres a new image file => replace the old file with the new
            string newFileName = Image_filename;
            
            if(ImageFile != null)
            {
                newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                newFileName += Path.GetExtension(ImageFile.FileName);
                
                string imageFolder = webHostEnvironment.WebRootPath + "/images/books/";
                string imageFullPath = Path.Combine(imageFolder, newFileName);


                using(var stream = System.IO.File.Create(imageFullPath))
                {
                    ImageFile.CopyTo(stream);
                }

                // delete old image
                string oldImageFullPath = Path.Combine(imageFolder, Image_filename);
                System.IO.File.Delete(oldImageFullPath);
                Console.WriteLine("Delete Image in this path: " + oldImageFullPath);
            }
            //update the book in the database

            try
            {
                string connectionString = "Data Source=.\\SQLEXPRESS; Initial Catalog=MangaStore; Integrated Security=True;";
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "UPDATE books SET Title=@Title, Authors=@Authors, Isbn=@Isbn, Pages=@Pages, Price=@Price, Category=@Category, Description=@Description, image_filename=@Image_filename WHERE ID=@id";


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
                        command.Parameters.AddWithValue("@id", ID);

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception e)
            {
                errorMessage = e.Message;
                return;
            }

            successMessage = "Data updated successfully";
            Response.Redirect("/Admin/Books/Index");
        }
    }
}