using DAPMDuLich.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace DAPMDuLich.Areas.Contributors.Controllers
{
    public class BookingController : Controller
    {
        private DAPMDuLichEntities db = new DAPMDuLichEntities();

        // GET: Contributor/Booking
        public ActionResult List()
        {
            // Lấy Contributor từ session
            var contributor = Session["contributor"] as Contributor;
            if (contributor == null)
            {
                return RedirectToAction("Login", "Auth", new { area = "Contributors" });
            }

            // Lấy danh sách đặt tour của Contributor hiện tại
            var bookings = db.DatTours
                             .Include(b => b.TourDuLich) // Bao gồm thông tin Tour
                             .Include(b => b.TaiKhoan) // Bao gồm thông tin người đặt
                             .Where(b => b.TourDuLich.ContributorID == contributor.ContributorID)
                             .ToList();

            // Lấy danh sách thông báo chưa đọc
            var notifications = db.Notifications
                            .Include(n => n.TourDuLich)
                            .Include(n => n.TaiKhoan)
                            .Where(n => n.ContributorID == contributor.ContributorID) // Chỉ lọc theo ContributorID
                            .ToList();

            // Cập nhật ViewBag
            ViewBag.Notifications = notifications;
            ViewBag.NotificationCount = notifications.Count(n => n.IsRead == false);
            return View(bookings);
        }

        
        public ActionResult Search(string searchQuery, bool? status)
        {
            // Lấy UserID từ session
            var contributor = Session["contributor"] as Contributor;
            if (contributor == null)
            {
                return RedirectToAction("Login", "Auth", new { area = "Contributors" });
            }

            var tours = db.DatTours.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                // Tạo biến để lưu BookingID và UserID
                int bookingId;
                int userId;

                bool isBookingId = int.TryParse(searchQuery, out bookingId);
                bool isUserId = int.TryParse(searchQuery, out userId);

                // Lọc tour theo ContributorID và theo từ khóa tìm kiếm
                tours = tours.Where(t => t.ContributorID == contributor.ContributorID &&
                                         (isBookingId && t.BookingID == bookingId ||
                                          isUserId && t.UserID == userId ||
                                          t.TaiKhoan.TenHienThi.Contains(searchQuery)));
            }
            else
            {
                // Nếu không có từ khóa tìm kiếm, chỉ lấy tour của contributor
                tours = tours.Where(t => t.ContributorID == contributor.ContributorID);
            }

            // Lọc theo trạng thái thanh toán
            if (status.HasValue)
            {
                tours = tours.Where(t => t.Status == status.Value);
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
        public ActionResult MarkAsRead(int id)
        {
            // Tìm thông báo theo ID
            var notification = db.Notifications.Find(id);
            if (notification != null)
            {
                notification.IsRead = true; // Đánh dấu là đã đọc
                db.SaveChanges(); // Lưu thay đổi vào cơ sở dữ liệu
            }

            return RedirectToAction("List", "Booking"); // Quay lại danh sách thông báo
        }
        
    }

}