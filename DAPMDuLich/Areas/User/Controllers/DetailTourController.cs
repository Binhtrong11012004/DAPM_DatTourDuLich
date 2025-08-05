using DAPMDuLich.App_Start;
using DAPMDuLich.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;  // Import namespace này để sử dụng Include()
namespace DAPMDuLich.Areas.User.Controllers
{
    public class DetailTourController : Controller
    {
        private DAPMDuLichEntities db;

        // Constructor khởi tạo DbContext
        public DetailTourController()
        {
            db = new DAPMDuLichEntities();
        }

        // GET: User/DetailTour
        public ActionResult DetailTour(int id)
        {
            var user = (TaiKhoan)HttpContext.Session["user"];

            if (user == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Eager loading: load luôn các danh gia cùng với Tour
            var tour = db.TourDuLiches
                         .Include(t => t.DanhGias) // Eager Loading các đánh giá
                         .FirstOrDefault(t => t.ID == id);

            if (tour == null)
            {
                return HttpNotFound();
            }

            // Trả về tour cùng với danh sách danh gia đã được load
            return View(tour);
        }

        // POST: User/DetailTour/AddReview
        // POST: User/DetailTour/AddReview
        [HttpPost]
        public ActionResult AddReview(int tourId, int DanhGiaSao, string NhanXet)
        {
            var user = (TaiKhoan)HttpContext.Session["user"];
            if (user == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Thêm đánh giá
            var tour = db.TourDuLiches.FirstOrDefault(t => t.ID == tourId);
            if (tour != null)
            {
                var review = new DanhGia
                {
                    ID = tourId,
                    UserID = user.UserID,
                    DanhGiaSao = DanhGiaSao,
                    NhanXet = NhanXet, // Lưu nhận xét
                    ThoiGianDanhGia = DateTime.Now
                };

                db.DanhGias.Add(review);
                db.SaveChanges();
            }

            return RedirectToAction("DetailTour", new { id = tourId });
        }

        // Đảm bảo DbContext được giải phóng khi controller bị hủy
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose(); // Giải phóng DbContext
            }
            base.Dispose(disposing);
        }
    }
}