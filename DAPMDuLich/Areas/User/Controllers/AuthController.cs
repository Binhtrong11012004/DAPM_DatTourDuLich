
using DAPMDuLich.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DAPMDuLich.Areas.User.Controllers
{
    public class AuthController : Controller
    {
        // GET: User/Auth
        DAPMDuLichEntities db = new DAPMDuLichEntities();

        
        public ActionResult Register()
        {
            return View();
        }

        // POST: User/Register/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register([Bind(Include = "ContributorID,TenDangNhap,MatKhau,TenHienThi,Email,SoDienThoai,DiaChi,CreateAt,Role,Tien")] TaiKhoan users)
        {
           

            if (ModelState.IsValid)
            {
                // Kiểm tra xem tên đăng nhập đã tồn tại chưa
                if (db.TaiKhoans.FirstOrDefault(x => x.TenDangNhap == users.TenDangNhap) != null)
                {
                    ModelState.AddModelError("", "Tên đăng nhập đã tồn tại");
                }
                // Kiểm tra xem email đã tồn tại chưa
                else if (db.TaiKhoans.FirstOrDefault(x => x.Email == users.Email) != null)
                {
                    ModelState.AddModelError("", "Email đã được sử dụng");
                }
                else
                {
                    // Kiểm tra độ dài mật khẩu
                    if (!string.IsNullOrEmpty(users.MatKhau) && users.MatKhau.Length >= 6)
                    {
                        
                        
                        users.CreateAt = DateTime.Now;
                        users.Role = "user";
                        users.Active = true;

                        // Thêm người dùng mới vào cơ sở dữ liệu
                        db.TaiKhoans.Add(users);
                        db.SaveChanges();

                       

                        // Chuyển hướng người dùng sang trang đăng nhập
                        return RedirectToAction("Login", "Auth");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Mật khẩu phải có ít nhất 6 ký tự!");
                    }
                }
            } 
            return View(users);
        }

        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(TaiKhoan taiKhoanKH)
        {
            var taikhoanform = taiKhoanKH.TenDangNhap;
            var matkhauform = taiKhoanKH.MatKhau;

            // Kiểm tra thông tin đăng nhập (tên đăng nhập và mật khẩu)
            var userCheck = db.TaiKhoans.SingleOrDefault(x => x.TenDangNhap.Equals(taikhoanform) && x.MatKhau.Equals(matkhauform));

            if (userCheck != null)
            {
                // Kiểm tra nếu Role là "user", nếu không phải thì báo lỗi
                if (userCheck.Role != "user")
                {
                    ViewBag.LoginFail = "Bạn không có quyền truy cập. Vui lòng liên hệ quản trị viên!";
                    return View("Login");
                }

                // Lưu thông tin người dùng vào session
                var taiKhoan = new mapTaiKhoan().ChiTiet(taikhoanform);
                Session["user"] = taiKhoan;

                // Kiểm tra xem có URL nào người dùng yêu cầu trước khi đăng nhập
                var returnUrl = Session["ReturnUrl"] as string;
                if (!string.IsNullOrEmpty(returnUrl))
                {
                    // Xóa ReturnUrl khỏi session sau khi sử dụng
                    Session["ReturnUrl"] = null;
                    return Redirect(returnUrl); // Chuyển hướng về URL trước đó
                }

                // Chuyển hướng đến trang chủ (kiểm tra lại tên action và controller)
                return RedirectToAction("Home", "Home");
            }
            else
            {
                ViewBag.LoginFail = "Đăng nhập thất bại, vui lòng kiểm tra lại!";
                return View("Login");
            }
        }



        [HttpPost]
        public ActionResult DangXuat()
        {
            Session.Clear();
            Session.Abandon();

            return RedirectToAction("Login", "Auth");
        }
    }
}