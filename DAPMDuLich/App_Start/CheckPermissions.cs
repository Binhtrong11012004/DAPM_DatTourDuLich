using DAPMDuLich.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace DAPMDuLich.App_Start
{
    public class CheckPermissions : AuthorizeAttribute
    {
        public string ChucNang { get; set; }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            var user = (TaiKhoan)HttpContext.Current.Session["user"];
            

            // Nếu chưa có session thì chuyển hướng đến trang đăng nhập
            if (user == null)
            {
                // Lưu URL hiện tại để chuyển hướng sau khi đăng nhập
                var currentUrl = HttpContext.Current.Request.Url.AbsoluteUri;
                HttpContext.Current.Session["ReturnUrl"] = currentUrl;

                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary(new
                    {
                        controller = "Auth",
                        action = "Login",
                        area = "User" // nếu không có area thì để trống
                    })
                );
                return;
            }
          
            // Kiểm tra Role của user
            //if (user.Role == "admin" /*|| user.Role == "contributor"*/)
            //{

            //    return;
            //}


            // Nếu không điền chức năng => Cho phép chạy
            if (string.IsNullOrEmpty(ChucNang))
            {
                return;
            }

            // Kiểm tra quyền của user với chức năng cụ thể (có thể thêm logic tại đây nếu cần)
        }
    }
}