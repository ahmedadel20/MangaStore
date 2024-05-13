using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.Intrinsics.X86;
using System.Threading.Tasks;
using MangaStore.MyHelpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace MangaStore.Pages.Admin.Books
{
    [RequireAuth(RequiredRole = "admin")]
    public class Index : PageModel
    {
        public List<BookInfo> Books = new List<BookInfo>();
        public string search = "";
        public int page = 1;
        public int totalPages = 0;
        private readonly int pageSize = 5;

        public string column = "id";
        public string order = "desc";

        private readonly ILogger<Index> _logger;

        public Index(ILogger<Index> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            search = Request.Query["search"];
            search ??= "";
            
            page = 1;
            string requtestPage = Request.Query["page"];
            if(requtestPage != null)
            {
                try
                {
                    page = int.Parse(requtestPage);
                }
                catch (Exception e)
                {
                    page = 1;
                }
            }

            string[] validColumns = {"ID", "Title", "Authors", "Pages", "Price", "Category", "Created_on"};
            column = Request.Query["column"];

            if(column == null ||!validColumns.Contains(column))
            {
                column = "id";
            }

            order = Request.Query["order"];
            if(order == null ||!order.Equals("asc"))
            {
                order = "desc";
            }

            try
            {
                string connectionString = "Data Source=.\\SQLEXPRESS; Initial Catalog=MangaStore; Integrated Security=True;";
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sqlCount = "Select Count(*) FROM books";
                    if(search.Length > 0)
                    {
                        sqlCount += " WHERE title LIKE @search OR authors like @search";
                    }

                     using(SqlCommand command = new SqlCommand(sqlCount, connection))
                    {
                        command.Parameters.AddWithValue("@search", "%" + search + "%");
                        decimal count = (int)command.ExecuteScalar();
                        totalPages = (int)Math.Ceiling(count / pageSize); 
                    }

                    string sql = "SELECT * FROM books";
                    if(search.Length > 0)
                    {
                        sql += " WHERE Title LIKE @search OR Authors LIKE @search";
                    }
                    sql += " ORDER BY " + column + " " + order; //" ORDER BY id DESC";
                    sql += " OFFSET @skip ROWS FETCH NEXT @pageSize ROWS ONLY";

                    using(SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@search", "%" + search + "%");
                        command.Parameters.AddWithValue("@skip", (page - 1) * pageSize);
                        command.Parameters.AddWithValue("@pageSize", pageSize);

                        using(SqlDataReader reader = command.ExecuteReader())
                        {
                            while(reader.Read())
                            {
                                BookInfo bookInfo = new BookInfo();
                                bookInfo.ID = reader.GetInt32(0);
                                bookInfo.Title = reader.GetString(1);
                                bookInfo.Authors = reader.GetString(2);
                                bookInfo.Isbn = reader.GetString(3);
                                bookInfo.Pages = reader.GetInt32(4);
                                bookInfo.Price = reader.GetDecimal(5);
                                bookInfo.Category = reader.GetString(6);
                                bookInfo.Description = reader.GetString(7);
                                bookInfo.Image_filename = reader.GetString(8);
                                bookInfo.Created_on = reader.GetDateTime(9).ToString("MM/dd/yyyy");
                                Books.Add(bookInfo);
                            }
                        }
                    }
                }
            }

            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }

    public class BookInfo
    {
        public int ID { get; set; }
        public string Title { get; set; } = "";
        public string Authors { get; set; } = "";
        public string Isbn { get; set; } = "";
        public int Pages { get; set; }
        public decimal Price { get; set; }
        public string Category { get; set; } = ""; 
        public string Description { get; set; } = "";
        public string Image_filename { get; set; } = "";
        public string Created_on { get; set; } = "";
    }
}