using DAPMDuLich.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace DAPMDuLich.Areas.User.Controllers
{
    public class InfomationCusController : Controller
    {
        // GET: User/InfomationCus
        private DAPMDuLichEntities db = new DAPMDuLichEntities();

        // GET: ThongTinCaNhan/Details
        public async Task<ActionResult> Details()
        {
            // Lấy đối tượng tài khoản từ session
            var user = (TaiKhoan)HttpContext.Session["user"];

            // Kiểm tra nếu session không tồn tại (user == null), chuyển hướng tới trang đăng nhập
            if (user == null)
            {
                return RedirectToAction("Login", "Auth"); // Redirect đến action "Login" trong controller "Auth"
            }

            // Tìm kiếm tài khoản trong database
            var taiKhoanKH = await db.TaiKhoans.SingleOrDefaultAsync(x => x.TenDangNhap == user.TenDangNhap);

           

            // Trả về view với đối tượng tài khoản
            return View(taiKhoanKH);
        }

        // GET: ThongTinCaNhan/Edit
        public async Task<ActionResult> Edit()
        {
            var user = (TaiKhoan)HttpContext.Session["user"];
            if (user == null)
            {
                return Content("Tài khoản KH session không tìm thấy");
            }

            var taiKhoanKH = await db.TaiKhoans.SingleOrDefaultAsync(x => x.TenDangNhap == user.TenDangNhap);
            if (taiKhoanKH == null)
            {
                return Content("Tài khoản KH không tìm thấy trong database");
            }
            return View(taiKhoanKH);
        }

        // POST: ThongTinCaNhan/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "UserID,MatKhau,TenHienThi,Email,SoDienThoai,CreateAt,DiaChi,Tien")] TaiKhoan taiKhoanKH)
        {
            var user = (TaiKhoan)HttpContext.Session["user"];
            if (user == null)
            {
                return Content("Tài khoản KH session không tìm thấy");
            }

            if (ModelState.IsValid)
            {
                var existingAccount = await db.TaiKhoans.SingleOrDefaultAsync(x => x.TenDangNhap == user.TenDangNhap);
                if (existingAccount == null)
                {
                    return Content("Tài khoản KH không tìm thấy trong database");
                }

                // Giữ giá trị TenDangNhap và Tien từ cơ sở dữ liệu
                taiKhoanKH.UserID = existingAccount.UserID;
                taiKhoanKH.TenDangNhap = existingAccount.TenDangNhap;
                taiKhoanKH.Tien = existingAccount.Tien;
                taiKhoanKH.CreateAt = existingAccount.CreateAt;

                // Cập nhật các thuộc tính khác từ form
                existingAccount.MatKhau = taiKhoanKH.MatKhau;
                existingAccount.TenHienThi = taiKhoanKH.TenHienThi;
                existingAccount.SoDienThoai = taiKhoanKH.SoDienThoai;
                existingAccount.DiaChi = taiKhoanKH.DiaChi;
                existingAccount.Email = taiKhoanKH.Email;

                db.Entry(existingAccount).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Details");
            }
            if (!ModelState.IsValid)
            {
                return View(taiKhoanKH);
            }
            return View(taiKhoanKH);
        }
    }
}