using DAPMDuLich.Areas.Contributors.Data.ViewModel;
using DAPMDuLich.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DAPMDuLich.Areas.Contributors.Controllers
{
    public class StatisticsController : Controller
    {
        private DAPMDuLichEntities db = new DAPMDuLichEntities();

        // GET: Statistics/Index/{contributorID}

        public ActionResult Index()
        {
            // Lấy contributor từ session
            var contributor = Session["contributor"] as Contributor;

            // Kiểm tra nếu contributor không tồn tại trong session (người dùng chưa đăng nhập)
            if (contributor == null)
            {
                return RedirectToAction("Login", "Auth", new { area = "Contributors" });
            }

            int contributorID = contributor.ContributorID;

            // Tổng số bài viết đã được duyệt
            var totalApprovedPosts = db.TourDuLiches
                .Where(t => t.ContributorID == contributorID && t.Status == "A")
                .Count();

            // Tổng số đặt tour
            var totalBookings = db.DatTours
                .Where(d => d.ContributorID == contributorID)
                .Count();

            // Tổng số đặt tour đã thanh toán
            var totalPaidBookings = db.DatTours
                .Where(d => d.ContributorID == contributorID && d.ThanhToan == true)
                .Count();

            // Tổng số đặt tour chưa thanh toán
            var totalUnpaidBookings = db.DatTours
                .Where(d => d.ContributorID == contributorID && d.ThanhToan == false)
                .Count();

            // Tính số tiền đã thanh toán cho Contributor
            var totalPaidAmount = 0m; // Tổng số tiền đã thanh toán cho Contributor

            // Lấy danh sách chi tiết thanh toán
            var paidBookings = db.DatTourChiTiets
                .Where(d => d.DatTour.ContributorID == contributorID && d.DatTour.ThanhToan == true)
                .ToList();

            // Biến để theo dõi số lần thanh toán
            foreach (var bookingDetail in paidBookings)
            {
                decimal paymentAmount = bookingDetail.Price.Value;

                // Nếu đây là lần thanh toán thứ 2 trở đi, cộng 95% vào tổng số tiền
                if (bookingDetail.DatTour.ThanhToan50 == true) // Thanh toán sau lần đầu tiên
                {
                    totalPaidAmount += paymentAmount * 0.95m;
                }
                else
                {
                    // Lần đầu tiên thanh toán (ThanhToan50 == false) => không cộng tiền
                    bookingDetail.DatTour.ThanhToan50 = true; // Đánh dấu đã thanh toán lần đầu
                    db.SaveChanges(); // Cập nhật trạng thái trong cơ sở dữ liệu
                }
            }

            // Lấy danh sách thông báo chưa đọc
            var notifications = db.Notifications
                .Where(n => n.ContributorID == contributorID)
                .ToList();

            // Cập nhật ViewModel với các giá trị thống kê
            var statistics = new ContributorStatisticsViewModel
            {
                TotalApprovedPosts = totalApprovedPosts,
                TotalBookings = totalBookings,
                TotalPaidBookings = totalPaidBookings,
                TotalUnpaidBookings = totalUnpaidBookings,
                TotalPaidAmount = totalPaidAmount, // Tổng số tiền Contributor nhận
                TotalBookingDetails = paidBookings.Count, // Tổng số chi tiết booking
            };

            // Cập nhật ViewBag để hiển thị thông báo và số lượng thông báo chưa đọc
            ViewBag.Notifications = notifications;
            ViewBag.NotificationCount = notifications.Count(n => n.IsRead == false);

            return View(statistics);
        }



        // Đánh dấu thông báo là đã đọc
        public ActionResult MarkAsRead(int id)
        {
            var notification = db.Notifications.Find(id);
            if (notification != null)
            {
                notification.IsRead = true;
                db.SaveChanges(); // Lưu thay đổi vào cơ sở dữ liệu
            }

            return RedirectToAction("Index"); // Quay lại trang thống kê
        }
    




        
    }
}

    