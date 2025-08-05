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
    public class BookingController : Controller
    {
        // GET: Admin/Booking
        private DAPMDuLichEntities db = new DAPMDuLichEntities();

        // GET: Admin/DatTour
        public ActionResult List()
        {
            var admin = (TaiKhoan)Session["admin"];
            if (admin == null)
            {
                // Nếu không tồn tại admin session, CheckSessionAdmin sẽ tự động chuyển hướng
                // nên đoạn này sẽ không bao giờ chạy
                return RedirectToAction("Login", "Account", new { area = "Admin" });
            }
            return View(db.DatTours.ToList());
        }

        // GET: Admin/Booking/Search
        // Action tìm kiếm
        public ActionResult Search(string searchQuery, DateTime? createAt)
        {
            var admin = (TaiKhoan)Session["admin"];
            if (admin == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Admin" });
            }

            var tours = db.DatTours.AsQueryable();

            // Tìm kiếm theo BookingID, Tên công ty, hoặc Tên khách hàng
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                int bookingId;
                bool isBookingId = int.TryParse(searchQuery, out bookingId);
                tours = tours.Where(t =>
                    (isBookingId && t.BookingID == bookingId) ||
                    t.TaiKhoan.TenHienThi.Contains(searchQuery) ||
                    t.Contributor.ContributorNickName.Contains(searchQuery));
            }

            // Lọc theo Ngày Tạo
            if (createAt.HasValue)
            {
                tours = tours.Where(t => t.CreateAt.HasValue && t.CreateAt.Value.Date == createAt.Value.Date);
            }

            return View("List", tours.ToList());
        }


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
