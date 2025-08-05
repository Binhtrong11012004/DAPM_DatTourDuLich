using DAPMDuLich.Areas.Admin.Data.ViewModel;
using DAPMDuLich.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace DAPMDuLich.Areas.Admin.Controllers
{
    public class StatisticsController : Controller
    {
        // GET: Admin/Statistics
        private DAPMDuLichEntities db = new DAPMDuLichEntities();

        // GET: Statistics/Index
        // GET: Admin/Statistics
        public ActionResult Index()
        {
            var admin = (TaiKhoan)Session["admin"];
            if (admin == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Admin" });
            }

            var adminStatistics = new AdminStatisticsViewModel();

            // Tổng quan về bài viết
            adminStatistics.TotalPosts = db.TourDuLiches.Count();
            adminStatistics.TotalApprovedPosts = db.TourDuLiches.Count(t => t.Status == "A");
            adminStatistics.TotalPendingPosts = db.TourDuLiches.Count(t => t.Status == "N");
            adminStatistics.TotalRejectedPosts = db.TourDuLiches.Count(t => t.Status == "R");

            // Hiệu suất của contributors
            adminStatistics.TopContributors = db.Contributors
                .Select(c => new AdminStatisticsViewModel.TopContributor
                {
                    ContributorID = c.ContributorID,
                    ContributorName = c.ContributorName,
                    PostCount = db.TourDuLiches.Count(t => t.ContributorID == c.ContributorID),
                    ApprovedPostCount = db.TourDuLiches.Count(t => t.ContributorID == c.ContributorID && t.Status == "A")
                })
                .OrderByDescending(c => c.PostCount)
                .ToList();

            adminStatistics.RejectedPostsByContributor = db.Contributors.ToDictionary(
                c => c.ContributorID,
                c => db.TourDuLiches.Count(t => t.ContributorID == c.ContributorID && t.Status == null)
            );

            adminStatistics.PostsByTourType = db.TourDuLiches
                .GroupBy(t => t.LoaiTour.Ten)
                .ToDictionary(g => g.Key, g => g.Count());

            // Đặt tour
            adminStatistics.TotalBookings = db.DatTours.Count();
            adminStatistics.TotalPaidBookings = db.DatTours.Count(d => d.ThanhToan == true);
            adminStatistics.TotalUnpaidBookings = db.DatTours.Count(d => d.ThanhToan == false);

            // Tổng doanh thu từ các chi tiết đặt tour, kiểm tra null
            adminStatistics.TotalRevenue = db.DatTourChiTiets.Sum(d => d.Price) ?? 0;

            // Tính hoa hồng admin nhận được (5% của tổng doanh thu từ các lần thanh toán thứ 2 trở đi)
            adminStatistics.TotalAdminCommission = db.DatTourChiTiets
                .Where(d => d.DatTour.ThanhToan50 == true)  // Chỉ tính từ lần thanh toán thứ 2 trở đi
                .Sum(d => d.Price.Value) * 0.05m;  // 5% từ tổng doanh thu của các lần thanh toán thứ 2

            return View(adminStatistics);
        }


    }
}
