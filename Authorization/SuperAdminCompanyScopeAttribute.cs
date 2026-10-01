using HRManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Task = System.Threading.Tasks.Task;

namespace HRManagement.Authorization
{
    /// <summary>
    /// Lets the SuperAdmin work on one company's configuration (roles, permissions, role menus)
    /// by sending the target company in the <see cref="HeaderName"/> header.
    /// The header is honored only on endpoints carrying this attribute, so the SuperAdmin still
    /// cannot read a company's HR data (employees, payroll…) elsewhere.
    /// Company users are unaffected: they always work in the company from their token.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class SuperAdminCompanyScopeAttribute : Attribute, IAsyncActionFilter
    {
        public const string HeaderName = "X-Company-Id";

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var http = context.HttpContext;
            if (http.User.IsInRole(PlatformRoles.SuperAdmin))
            {
                if (!int.TryParse(http.Request.Headers[HeaderName], out var companyId))
                {
                    context.Result = new BadRequestObjectResult(new { message = "Chọn công ty cần cấu hình." });
                    return;
                }

                var db = http.RequestServices.GetRequiredService<HrmsDbContext>();
                if (!await db.Companies.AnyAsync(c => c.CompanyId == companyId))
                {
                    context.Result = new NotFoundObjectResult(new { message = "Không tìm thấy công ty." });
                    return;
                }
            }

            await next();
        }
    }
}
