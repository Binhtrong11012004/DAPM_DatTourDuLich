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
    public class BookingDetailController : Controller
    {
        private DAPMDuLichEntities db = new DAPMDuLichEntities();
        // GET: Contributor/Booking/Details/5
        public ActionResult Details(int? id)
        {
            // Kiểm tra nếu id không hợp lệ
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Lấy Contributor từ session
            var contributor = Session["contributor"] as Contributor;
            if (contributor == null)
            {
                return RedirectToAction("Login", "Auth", new { area = "Contributors" });
            }

            // Tìm các chi tiết đặt tour dựa trên BookingID và ContributorID
            var bookingDetails = db.DatTourChiTiets
                .Where(d => d.DatTour.BookingID == id && d.DatTour.ContributorID == contributor.ContributorID)
                .Include(d => d.DatTour) // Bao gồm thông tin tour
                .ToList();

            if (bookingDetails == null || !bookingDetails.Any())
            {
                return HttpNotFound();
            }

            return View(bookingDetails); // Trả về view hiển thị chi tiết
        }
    }
}