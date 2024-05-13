using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MangaStore.MyHelpers
{
    public class RequireNoAuthAttribute : Attribute, IPageFilter
    {
        public void OnPageHandlerExecuted(PageHandlerExecutedContext context)
        {
        }

        public void OnPageHandlerExecuting(PageHandlerExecutingContext context)
        {
            string? role = context.HttpContext.Session.GetString("role");
            
            if(role != null)
            {
                //the user is authenticated => redirect the user to the home page
                context.Result = new RedirectResult("/");
            }
        }

        public void OnPageHandlerSelected(PageHandlerSelectedContext context)
        {
        }
    }
}