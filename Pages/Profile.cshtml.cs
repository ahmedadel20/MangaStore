using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using MangaStore.MyHelpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace MangaStore.Pages
{
    [RequireAuth]
    public class Profile : PageModel
    {
        [Required(ErrorMessage = "The First Name is required")]
        public string firstName { get; set; } = "";

        [Required(ErrorMessage = "The Last Name is required")]
        public string lastName { get; set; } = "";

        [Required(ErrorMessage = "The First Name is required"), EmailAddress]
        public string Email { get; set; } = "";

        public string? Phone { get; set; } = "";

        [Required(ErrorMessage = "The Address is required")]
        public string Address { get; set; } = "";


        public void OnGet()
        {
        }
    }
}