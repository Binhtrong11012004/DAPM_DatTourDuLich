using DAPMDuLich.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DAPMDuLich.Areas.Contributors.Controllers
{
    public class AuthController : Controller
    {
        private DAPMDuLichEntities db = new DAPMDuLichEntities();

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(Contributor taikhoancontributor)
        {
            var taikhoanform = taikhoancontributor.ContributorName;
            var matkhauform = taikhoancontributor.ContributorPassword;

            // Debugging: Kiểm tra giá trị nhập vào
            System.Diagnostics.Debug.WriteLine($"Tên đăng nhập: {taikhoanform}, Mật khẩu: {matkhauform}");

            // Kiểm tra thông tin đăng nhập cho Contributors
            var userCheck = db.Contributors.SingleOrDefault(x =>
                x.ContributorName.Equals(taikhoanform, StringComparison.OrdinalIgnoreCase) &&
                x.ContributorPassword.Equals(matkhauform));

            if (userCheck != null)
            {
                Session["contributor"] = userCheck; // Lưu thông tin vào session
                return RedirectToAction("List", "Tour", new { area = "Contributors" }); // Đảm bảo bạn sử dụng "area" với chữ thường
            }
            else
            {
                ViewBag.LoginFail = "Đăng nhập thất bại, vui lòng kiểm tra lại!";
                return View();
            }
        }

        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register([Bind(Include = "ContributorID,ContributorName,ContributorPassword,ContributorNickName,ContributorEmail,ContributorPhone,ContributorAddress,CreateAt,Role,Tien")] Contributor users)
        {
            if (ModelState.IsValid)
            {
                if (db.Contributors.Any(x => x.ContributorName == users.ContributorName))
                {
                    ModelState.AddModelError("", "Tên đăng nhập đã tồn tại");
                }
                else if (db.Contributors.Any(x => x.ContributorEmail == users.ContributorEmail))
                {
                    ModelState.AddModelError("", "Email đã được sử dụng");
                }
                else
                {
                    if (!string.IsNullOrEmpty(users.ContributorPassword) && users.ContributorPassword.Length >= 6)
                    {
                        users.CreateAt = DateTime.Now;
                       
                        users.Active = true;

                        try
                        {
                            db.Contributors.Add(users);
                            db.SaveChanges();
                            return RedirectToAction("Login", "Auth", new { Areas = "Contributors" });
                        }
                        catch (DbEntityValidationException e)
                        {
                            foreach (var eve in e.EntityValidationErrors)
                            {
                                foreach (var ve in eve.ValidationErrors)
                                {
                                    ModelState.AddModelError(ve.PropertyName, ve.ErrorMessage);
                                }
                            }
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Mật khẩu phải có ít nhất 6 ký tự!");
                    }
                }
            }
            return View(users);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult DangXuat()
        {
            try
            {
                // Xóa tất cả session
                Session.Clear();
                Session.Abandon();

                // Xóa cookies nếu cần
                Response.Cookies.Clear();

                // Xóa cookie cụ thể nếu có
                if (Request.Cookies["yourCookieName"] != null)
                {
                    var cookie = new HttpCookie("yourCookieName");
                    cookie.Expires = DateTime.Now.AddDays(-1);  // Đặt lại thời gian hết hạn cho cookie
                    Response.Cookies.Add(cookie);
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Logout failed: " + ex.Message);
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}