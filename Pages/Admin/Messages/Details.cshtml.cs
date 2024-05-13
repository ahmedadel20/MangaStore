using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using MangaStore.MyHelpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using static MangaStore.Pages.Admin.Messages.Index;

namespace MangaStore.Pages.Admin.Messages
{
    [RequireAuth(RequiredRole = "admin")]
    public class Details : PageModel
    {
        public MessageInfo messageInfo = new MessageInfo();
        private readonly ILogger<Details> _logger;

        public Details(ILogger<Details> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            string requestId = Request.Query["id"];

            try
            {
                string connectionString = "Data Source=.\\SQLEXPRESS; Initial Catalog=MangaStore; Integrated Security=True;";


                using(SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "SELECT * FROM messages WHERE id=@id";

                    using(SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@id", requestId);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if(reader.Read())
                            {
                                messageInfo.id = reader.GetInt32(0);
                                messageInfo.firstName = reader.GetString(1);
                                messageInfo.lastName = reader.GetString(2);
                                messageInfo.email = reader.GetString(3);
                                messageInfo.phone = reader.GetString(4);
                                messageInfo.message = reader.GetString(5);
                                messageInfo.createdAt = reader.GetDateTime(6).ToString("MM/dd/yyyy");

                            }
                            else
                            {
                                Response.Redirect("/Admin/Messages/Index");
                            }
                        }
                    } 
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Response.Redirect("/Admin/Messages/Index");
            }
        }
    }
}