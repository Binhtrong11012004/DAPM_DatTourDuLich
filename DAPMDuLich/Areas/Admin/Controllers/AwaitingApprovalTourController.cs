using DAPMDuLich.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DAPMDuLich.Areas.Admin.Controllers
{
    public class AwaitingApprovalTourController : Controller
    {
        // GET: Admin/AwaitingApprovalTour
        private DAPMDuLichEntities db = new DAPMDuLichEntities();

        public ActionResult ListPendingTours()
        {
            // Kiểm tra xem session admin có tồn tại không
            var admin = (TaiKhoan)Session["admin"];
            if (admin == null)
            {
                // Nếu không tồn tại admin session, CheckSessionAdmin sẽ tự động chuyển hướng
                // nên đoạn này sẽ không bao giờ chạy
                return RedirectToAction("Login", "Account", new { area = "Admin" });
            }
            // Retrieve all tours with Status set to "N"
            var pendingTours = db.TourDuLiches.Where(t => t.Status == "N").ToList();
            // Truyền danh sách thông báo vào ViewBag
            ViewBag.Notifications = db.Notifications
                                     .Include("Contributor") // Đảm bảo include để lấy dữ liệu Contributor
                                     .Where(n => n.IsRead == false)  // chỉ lấy thông báo chưa đọc
                                     .OrderByDescending(n => n.CreateAt)
                                     .ToList();

            
            // Return the view with only the pending tours
            return View(pendingTours);
        }
        // GET: Admin/AwaitingApprovalTour/Details
        public ActionResult Details(int id)
        {
            var tour = db.TourDuLiches.Find(id);
            if (tour == null)
            {
                return HttpNotFound();
            }
            return View(tour);
        }
        [HttpPost]
        public ActionResult UpdateStatusTour(int id, bool? status, string rejectionNote)
        {
            // Find the tour to update
            var tour = db.TourDuLiches.Find(id);
            if (tour != null)
            {
                // Update the status based on the input value
                if (status == true)
                {
                    tour.Status = "A"; // 'A' for Approved
                    TempData["Message"] = "Duyệt bài thành công!";
                }
                else if (status == false)
                {
                    tour.Status = "R"; // 'R' for Rejected
                    TempData["Message"] = "Đã từ chối duyệt bài!";
                    tour.RejectionNote = rejectionNote; // Save rejection note
                }
                else
                {
                    tour.Status = "N"; // 'N' for Pending
                    TempData["Message"] = "Bài viết đang chờ duyệt.";
                }

                db.SaveChanges();
            }
            return RedirectToAction("ListPendingTours", "AwaitingApprovalTour", new { id = id }); // Redirect back to the tour details page
        }
        
        


    }
}