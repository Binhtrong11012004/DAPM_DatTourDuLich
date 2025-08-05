using DAPMDuLich.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;


namespace DAPMDuLich.App_Start
{
    public class CheckSessionsContributor : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            var contributor = (Contributor)HttpContext.Current.Session["contributor"];


            // Nếu chưa có session thì chuyển hướng đến trang đăng nhập
            if (contributor == null)
            {
                // Lưu URL hiện tại để chuyển hướng sau khi đăng nhập
                var currentUrl = HttpContext.Current.Request.Url.AbsoluteUri;
                HttpContext.Current.Session["ReturnUrlContributor"] = currentUrl;

                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary(new
                    {
                        controller = "Auth",
                        action = "Login",
                        area = "Contributors" // nếu không có area thì để trống
                    })
                );
                return;
            }
        }
    }
}