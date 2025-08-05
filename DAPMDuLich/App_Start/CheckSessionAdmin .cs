using DAPMDuLich.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace DAPMDuLich.App_Start
{
    public class CheckSessionAdmin : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            // Kiểm tra nếu session admin không tồn tại
            var admin = (TaiKhoan)HttpContext.Current.Session["admin"];

            // Nếu chưa có session admin thì chuyển hướng đến trang đăng nhập
            if (admin == null)
            {
                // Lưu URL hiện tại để chuyển hướng sau khi đăng nhập
                var currentUrl = HttpContext.Current.Request.Url.AbsoluteUri;
                HttpContext.Current.Session["ReturnUrl"] = currentUrl;

                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary(new
                    {
                        controller = "Account",
                        action = "Login",
                        area = "Admin" // điều hướng đến area thích hợp (Admin)
                    })
                );
                return;
            }

            // Nếu có session admin thì không làm gì và tiếp tục với action được gọi
        }
    }
}