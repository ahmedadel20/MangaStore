using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using MangaStore.MyHelpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace MangaStore.Pages.Admin.Messages
{
    [RequireAuth(RequiredRole = "admin")]
    public class Index : PageModel
    {
        public List<MessageInfo> messageList = new List<MessageInfo>();
        public int page = 1; // the current html page number
        public int totalPages = 0;
        public readonly int pageSize = 5; // each page size
        private readonly ILogger<Index> _logger;

        public Index(ILogger<Index> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            page = 1;
            string requestPage = Request.Query["page"];
            if(requestPage != null)
            {
                try
                {
                    page = Int32.Parse(requestPage);
                }
                catch (Exception e)
                {
                    page = 1;
                }
            }

            try
            {
                string connectionString = "Data Source=.\\SQLEXPRESS; Initial Catalog=MangaStore; Integrated Security=True;";


                using(SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sqlCount = "Select count(*) from messages";
                    using(SqlCommand command = new SqlCommand(sqlCount, connection))
                    {
                        decimal count = (int)command.ExecuteScalar();
                        totalPages = (int)Math.Ceiling(count / pageSize);
                    }

                    string sql = "SELECT * FROM messages ORDER BY id DESC";
                    sql += " OFFSET @skip ROWS FETCH NEXT @pageSize ROWS ONLY";    

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@skip", (page - 1 ) * pageSize);
                        command.Parameters.AddWithValue("@pageSize", pageSize);

                        using(SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                MessageInfo messageInfo = new MessageInfo();
                                messageInfo.id = reader.GetInt32(0);
                                messageInfo.firstName = reader.GetString(1);
                                messageInfo.lastName = reader.GetString(2);
                                messageInfo.email = reader.GetString(3);
                                messageInfo.phone = reader.GetString(4);
                                messageInfo.message = reader.GetString(5);
                                messageInfo.createdAt = reader.GetDateTime(6).ToString("MM/dd/yyyy");

                                messageList.Add(messageInfo);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                
                Console.WriteLine(ex.Message);
            }
        }

        public class MessageInfo
        {
            public int id { get; set; }
            public string firstName { get; set; } = "";
            public string lastName { get; set; } ="";
            public string email { get; set; }="";
            public string phone { get; set; } ="";
            public string subject { get; set; } ="";
            public string message { get; set; } ="";
            public string createdAt { get; set; } ="";
        }
    }
}