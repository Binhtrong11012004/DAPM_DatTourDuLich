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
    public class PriceController : Controller
    {
        // GET: Admin/Price
        private DAPMDuLichEntities db = new DAPMDuLichEntities();

        // GET: Admin/TourType/List
        public ActionResult List()
        {
            var admin = (TaiKhoan)Session["admin"];
            if (admin == null)
            {
                // Nếu không tồn tại admin session, CheckSessionAdmin sẽ tự động chuyển hướng
                // nên đoạn này sẽ không bao giờ chạy
                return RedirectToAction("Login", "Account", new { area = "Admin" });
            }
            var tourTypes = db.MucGias.ToList(); // Lấy tất cả loại tour từ cơ sở dữ liệu
            return View(tourTypes); // Truyền danh sách loại tour vào View
        }
        // GET: Admin/TourType/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/TourType/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(MucGia model)
        {
            if (ModelState.IsValid)
            {
                db.MucGias.Add(model); // Thêm loại tour vào DbSet
                db.SaveChanges(); // Lưu thay đổi vào cơ sở dữ liệu
                return RedirectToAction("List"); // Chuyển hướng về trang Index
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
            MucGia mucgia = db.MucGias.Find(id);
            if (mucgia == null)
            {
                return HttpNotFound();
            }
            return View(mucgia);
        }

        // POST: Admin/TourType/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(MucGia mucgia)
        {
            if (ModelState.IsValid)
            {
                db.Entry(mucgia).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("List");
            }
            return View(mucgia);
        }
        // GET: Admin/TourType/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MucGia mucgia = db.MucGias.Find(id);
            if (mucgia == null)
            {
                return HttpNotFound();
            }
            return View(mucgia);
        }

        // POST: Admin/TourType/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            MucGia mucgia = db.MucGias.Find(id);
            if (mucgia != null)
            {
                db.MucGias.Remove(mucgia);
                db.SaveChanges();
            }
            return RedirectToAction("List");
        }
    }
}