using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SPMS.Controllers
{
    public class BaseController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // If UserID session is missing, redirect to login
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserID")))
            { 
                context.Result = RedirectToAction("Login", "Account"); 
            }

            base.OnActionExecuting(context);
        }
    }
}
