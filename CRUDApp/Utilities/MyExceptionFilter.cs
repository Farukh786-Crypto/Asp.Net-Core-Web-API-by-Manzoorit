using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CRUDApp.Utilities
{
    public class MyExceptionFilter : IExceptionFilter
    {
        
        public void OnException(ExceptionContext context)
        {
            HttpResponse response = context.HttpContext.Response;
            response.StatusCode = 500;
            response.ContentType = "application/json";
            context.Result = new ObjectResult("We are Working on it on Application Level..!");
        }
    }
}
