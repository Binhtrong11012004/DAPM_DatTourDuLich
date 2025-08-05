using DAPMDuLich.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DAPMDuLich.Areas.Contributors.Controllers
{
    public class FeedbackController : Controller
    {
        private DAPMDuLichEntities db = new DAPMDuLichEntities(); // Khởi tạo DbContext

        // GET: Contributors/Feedback/List

        public ActionResult List()
        {
            // Lấy thông tin Contributor từ session
            var contributor = Session["contributor"] as Contributor;

            // Kiểm tra nếu Contributor không tồn tại trong session (người dùng chưa đăng nhập)
            if (contributor == null)
            {
                return RedirectToAction("Login", "Auth", new { area = "Contributors" });
            }

            // Lấy danh sách các tour của Contributor đã được duyệt
            var tours = db.TourDuLiches
                          .Where(t => t.ContributorID == contributor.ContributorID && t.Status == "A") // Chỉ lấy tour có Status = "A"
                          .Include(t => t.DanhGias) // Include các đánh giá của tour
                          .ToList();

            // Lấy danh sách thông báo chưa đọc và đánh dấu là đã đọc
            var notifications = db.Notifications
                                  .Where(n => n.ContributorID == contributor.ContributorID && (n.IsRead ?? false) == false)
                                  .ToList();

            foreach (var notification in notifications)
            {
                notification.IsRead = true; // Đánh dấu thông báo là đã đọc
            }

            // Lưu các thay đổi vào cơ sở dữ liệu
            db.SaveChanges();

            // Cập nhật ViewBag cho thông báo đã đọc
            ViewBag.Notifications = notifications;
            ViewBag.NotificationCount = notifications.Count(); // Số lượng thông báo chưa đọc

            return View(tours); // Trả về View với danh sách tour đã được duyệt và đánh giá
        }
        public ActionResult Details(int id)
        {
            var contributor = Session["contributor"] as Contributor;

            if (contributor == null)
            {
                return RedirectToAction("Login", "Auth", new { area = "Contributors" });
            }

            // Lấy thông tin tour theo ID và các đánh giá liên quan
            var tour = db.TourDuLiches
                         .Include(t => t.DanhGias)
                         .FirstOrDefault(t => t.ID == id && t.ContributorID == contributor.ContributorID);

            if (tour == null)
            {
                return HttpNotFound();
            }
            var notifications = db.Notifications
                                 .Where(n => n.ContributorID == contributor.ContributorID && (n.IsRead ?? false) == false)
                                 .ToList();

            foreach (var notification in notifications)
            {
                notification.IsRead = true; // Đánh dấu thông báo là đã đọc
            }

            // Lưu các thay đổi vào cơ sở dữ liệu
            db.SaveChanges();

            // Cập nhật ViewBag cho thông báo đã đọc
            ViewBag.Notifications = notifications;
            ViewBag.NotificationCount = notifications.Count(); // Số lượng thông báo chưa đọc
            return View(tour); // Trả về View với thông tin chi tiết tour và đánh giá
        }
    }
}