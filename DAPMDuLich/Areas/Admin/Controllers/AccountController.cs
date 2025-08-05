using DAPMDuLich.App_Start;
using DAPMDuLich.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace DAPMDuLich.Areas.Admin.Controllers
{
    public class AccountController : Controller
    {
        private DAPMDuLichEntities db = new DAPMDuLichEntities();
        // GET: Admin/Account
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(string tenDangNhap, string matKhau)
        {
            //1. Kiểm tra tên đăng nhập hoặc mật khẩu có trống => Trở về trang đăng nhập: Thông báo thiếu thông tin
            if (string.IsNullOrEmpty(tenDangNhap) == true | string.IsNullOrEmpty(matKhau) == true)
            {
                ViewBag.thongbao = "Thông báo thiếu thông tin";
                return View();
            }
            //2. Tìm tài khoản theo tên đăng nhập trong Database
            var taiKhoan = new mapTaiKhoan().ChiTiet(tenDangNhap);
            //3. Kiểm tra tồn tại tài khoản => nếu ko tồn tại => Trở về trang đăng nhập: Tài khoản hoặc mật khẩu không đúng
            if (taiKhoan == null)
            {
                ViewBag.thongbao = "Tài khoản hoặc mật khẩu không đúng";
                ViewBag.tenDangNhap = tenDangNhap;
                return View();
            }
            //4. Kiểm tra mật khẩu => Nếu sai => Trở về trang đăng nhập: Tài khoản hoặc mật khẩu không đúng
            //string matKhauMaHoa = new Common.MaHoa.MaHoaDuLieu().CreateMd5(matKhau);
            if (taiKhoan.MatKhau != matKhau)
            {
                ViewBag.thongbao = "Tài khoản hoặc mật khẩu không đúng";
                ViewBag.tenDangNhap = tenDangNhap;
                return View();
            }
            //5. Kiểm tra active (hoat dong)
            if (taiKhoan.Active != true)
            {
                ViewBag.thongbao = "Tài khoản đang tạm khóa";
                ViewBag.tenDangNhap = tenDangNhap;
                return View();
            }
            // 7. Kiểm tra Role của tài khoản => Nếu không phải là admin thì không được đăng nhập
            if (taiKhoan.Role != "admin")
            {
                ViewBag.thongbao = "Tài khoản không có quyền truy cập";
                ViewBag.tenDangNhap = tenDangNhap;
                return View();
            }

            //6. Tài khoản đăng nhập ok: Lưu lại session server
            Session["admin"] = taiKhoan;



            //8. Chuyển hướng sang trang chủ Admin 
            return Redirect("/Admin/HomeAdmin/Index");
        }

        //[CheckPermissions(ChucNang = "TaiKhoan_ChiTiet")]
        //Danh sách tài khoản
        public ActionResult List()
        {
            var admin = (TaiKhoan)Session["admin"];
            if (admin == null)
            {
                // Nếu không tồn tại admin session, CheckSessionAdmin sẽ tự động chuyển hướng
                // nên đoạn này sẽ không bao giờ chạy
                return RedirectToAction("Login", "Account", new { area = "Admin" });
            }
            return View(new mapTaiKhoan().DanhSach());
        }


        //public ActionResult Detail(string tenDangNhap)
        //{
        //    var taikhoan = new mapTaiKhoan().ChiTiet(tenDangNhap);
        //    return View(taikhoan);
        //}

        // GET: Admin/TaiKhoan/Create
        //Thêm tài khoản
        
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "UserID,TenDangNhap,MatKhau,TenHienThi,Email,SoDienThoai,DiaChi,CreateAt,Role,Active,Tien")] TaiKhoan taiKhoanKH)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra xem tên đăng nhập đã tồn tại trong cơ sở dữ liệu hay chưa
                var existingUser = db.TaiKhoans.FirstOrDefault(u => u.TenDangNhap == taiKhoanKH.TenDangNhap);

                if (existingUser != null)
                {
                    // Nếu tên đăng nhập đã tồn tại, thêm thông báo lỗi vào ModelState và trả lại view
                    ModelState.AddModelError("TenDangNhap", "Tên đăng nhập đã tồn tại. Vui lòng chọn tên khác.");
                    return View(taiKhoanKH);
                }

                // Nếu không có giá trị Role thì mặc định là "user"
                if (string.IsNullOrEmpty(taiKhoanKH.Role))
                {
                    taiKhoanKH.Role = "user"; // Mặc định là "user"
                }

                taiKhoanKH.CreateAt = DateTime.Now; // Gán thời gian tạo tài khoản
                db.TaiKhoans.Add(taiKhoanKH);       // Thêm vào database
                db.SaveChanges();                   // Lưu vào database

                return RedirectToAction("List");    // Chuyển hướng về danh sách tài khoản
            }

            return View(taiKhoanKH); // Trả lại View nếu có lỗi
        }
        // GET: Admin/Account/Edit/5
        public ActionResult Edit(int id)
        {
            var taiKhoan = db.TaiKhoans.Find(id);
            if (taiKhoan == null)
            {
                return HttpNotFound();
            }
            return View(taiKhoan);
        }

        // POST: Admin/Account/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(TaiKhoan taiKhoanKH)
        {
            if (ModelState.IsValid)
            {
                // Fetch the original account from the database
                var existingTaiKhoan = db.TaiKhoans.Find(taiKhoanKH.UserID);

                if (existingTaiKhoan != null)
                {
                    // Only update the Role field, leave other fields as is
                    existingTaiKhoan.Role = taiKhoanKH.Role;

                    db.Entry(existingTaiKhoan).State = EntityState.Modified;
                    db.SaveChanges();

                    return RedirectToAction("List"); // Redirect back to the account list
                }
            }
            return View(taiKhoanKH); // Return the same view if there are validation errors
        }

        // GET: Admin/Account/Search

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}