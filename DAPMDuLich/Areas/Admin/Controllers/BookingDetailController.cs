using DAPMDuLich.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace DAPMDuLich.Areas.Admin.Controllers
{
    public class BookingDetailController : Controller
    {
        private DAPMDuLichEntities db = new DAPMDuLichEntities();

        // GET: Admin/BookingDetails/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DatTourChiTiet bookingDetail = db.DatTourChiTiets.SingleOrDefault(x => x.BookingID == id);
            if (bookingDetail == null)
            {
                return HttpNotFound();
            }
            return View(bookingDetail);
        }
    }
}