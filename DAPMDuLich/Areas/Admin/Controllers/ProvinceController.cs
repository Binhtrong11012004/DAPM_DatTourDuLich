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
    public class ProvinceController : Controller
    {
        private DAPMDuLichEntities db = new DAPMDuLichEntities();
        // GET: Admin/Province/List
        public ActionResult List()
        {
            var admin = (TaiKhoan)Session["admin"];
            if (admin == null)
            {
                // Nếu không tồn tại admin session, CheckSessionAdmin sẽ tự động chuyển hướng
                // nên đoạn này sẽ không bao giờ chạy
                return RedirectToAction("Login", "Account", new { area = "Admin" });
            }
            var provinces = db.TinhThanhs.ToList(); // Lấy tất cả loại tour từ cơ sở dữ liệu
            return View(provinces); // Truyền danh sách loại tour vào View
        }
        // GET: Admin/Province/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/Province/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(TinhThanh model)
        {
            if (ModelState.IsValid)
            {
                db.TinhThanhs.Add(model); // Thêm tỉnh thành vào DbSet
                db.SaveChanges(); // Lưu thay đổi vào cơ sở dữ liệu
                return RedirectToAction("List"); // Chuyển hướng về trang List
            }
            return View(model); // Hiển thị lại form nếu dữ liệu không hợp lệ
        }
        // GET: Admin/TourType/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TinhThanh provinces = db.TinhThanhs.Find(id);
            if (provinces == null)
            {
                return HttpNotFound();
            }
            return View(provinces);
        }

        // POST: Admin/Province/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(TinhThanh provinces)
        {
            if (ModelState.IsValid)
            {
                db.Entry(provinces).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("List");
            }
            return View(provinces);
        }

        // GET: Admin/Province/Delete/5
        
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Tìm tỉnh thành bằng ID_Tinh
            TinhThanh province = db.TinhThanhs.Find(id);
            if (province == null)
            {
                return HttpNotFound();
            }

            return View(province);
        }

        // POST: Admin/Province/Delete/5
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            // Kiểm tra nếu tỉnh thành tồn tại trong CSDL
            TinhThanh province = db.TinhThanhs.Find(id);
            if (province != null)
            {
                db.TinhThanhs.Remove(province); // Xóa tỉnh thành
                db.SaveChanges(); // Lưu thay đổi vào cơ sở dữ liệu
            }

            return RedirectToAction("List"); // Quay lại trang danh sách
        }
    }
}