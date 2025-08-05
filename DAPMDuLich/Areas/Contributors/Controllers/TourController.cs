using DAPMDuLich.App_Start;
using DAPMDuLich.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace DAPMDuLich.Areas.Contributors.Controllers
{
    public class TourController : Controller
    {
        private DAPMDuLichEntities db = new DAPMDuLichEntities();

        // GET: Contributors/Tour/List
        public ActionResult List()
        {
            // Lấy UserID từ session
            var contributor = Session["contributor"] as Contributor;
            if (contributor == null)
            {
                return RedirectToAction("Login", "Auth", new { Areas = "Contributors" });
            }

            // Lấy danh sách tour của người dùng
            var tours = db.TourDuLiches.Where(t => t.ContributorID == contributor.ContributorID).ToList();

            // Lấy danh sách thông báo chưa đọc và đánh dấu là đã đọc
            var notifications = db.Notifications
                                .Include(n => n.TourDuLich)
                                .Include(n => n.TaiKhoan)
                                .Where(n => n.ContributorID == contributor.ContributorID)
                                .ToList();

            foreach (var notification in notifications)
            {
                if ((notification.IsRead ?? false) == false) // Nếu IsRead là null, coi như false
                {
                    notification.IsRead = true; // Đánh dấu thông báo là đã đọc
                }
            }

            // Lưu các thay đổi vào cơ sở dữ liệu
            db.SaveChanges();

            // Cập nhật ViewBag cho thông báo đã đọc
            ViewBag.Notifications = notifications;
            ViewBag.NotificationCount = 0; // Tất cả thông báo đã đọc, đặt số lượng thông báo chưa đọc về 0

            return View(tours); // Trả về View với danh sách tour
        }
        // GET: Contributors/Tour/Create
        public ActionResult Create()
        {
            return View(new TourDuLich());
        }

        // POST: Contributors/Tour/Create
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(TourDuLich model)
        {
            var contributor = Session["contributor"] as Contributor;
            if (contributor == null)
            {
                return RedirectToAction("Login", "Auth", new { area = "Contributors" });
            }

            model.ContributorID = contributor.ContributorID; // Gán UserID cho tour
            model.Status = "N"; // Trạng thái chờ duyệt

            // Kiểm tra điều kiện cho từng thuộc tính
            if (string.IsNullOrWhiteSpace(model.TieuDe) || model.TieuDe.Length < 10)
            {
                ModelState.AddModelError("TieuDe", "Tiêu đề không được để trống và phải có ít nhất 10 ký tự.");
            }

            if (string.IsNullOrWhiteSpace(model.HinhAnh))
            {
                ModelState.AddModelError("HinhAnh", "Phải có ít nhất một hình ảnh.");
            }

            if (!model.TripStart.HasValue)
            {
                ModelState.AddModelError("TripStart", "Thời gian khởi hành là bắt buộc.");
            }

            if (!model.TripEnd.HasValue)
            {
                ModelState.AddModelError("TripEnd", "Thời gian kết thúc là bắt buộc.");
            }
            else if (model.TripStart.HasValue && model.TripEnd.Value <= model.TripStart.Value)
            {
                ModelState.AddModelError("TripEnd", "Thời gian kết thúc phải sau thời gian khởi hành.");
            }

            if (string.IsNullOrWhiteSpace(model.LichTrinh))
            {
                ModelState.AddModelError("LichTrinh", "Lịch trình không được để trống.");
            }

            if (string.IsNullOrWhiteSpace(model.DiaDiem))
            {
                ModelState.AddModelError("DiaDiem", "Địa điểm không được để trống.");
            }

            if (!model.SoNguoiToiDa.HasValue || model.SoNguoiToiDa.Value <= 0)
            {
                ModelState.AddModelError("SoNguoiToiDa", "Số người tối đa không được để trống và phải là số nguyên dương.");
            }

            if (string.IsNullOrWhiteSpace(model.PhuongTien))
            {
                ModelState.AddModelError("PhuongTien", "Phương tiện không được để trống.");
            }
            else
            {
                // Biểu thức chính quy để kiểm tra chỉ cho phép chữ cái (bao gồm chữ có dấu) và khoảng trắng
                var regex = new System.Text.RegularExpressions.Regex(@"^[a-zA-Zàáảãạêêếềểễệôôơơúùũụíìĩịóòõọùũữưáíàạ]+(\s[a-zA-Zàáảãạêêếềểễệôôơơúùũụíìĩịóòõọùũữưáíàạ]+)*$");

                if (!regex.IsMatch(model.PhuongTien))
                {
                    ModelState.AddModelError("PhuongTien", "Phương tiện chỉ được phép chứa các ký tự chữ cái (có dấu) và khoảng trắng, không được chứa các ký tự đặc biệt như !, ?, .");
                }
            }

            if (!model.GiaTour.HasValue || model.GiaTour <= 0)
            {
                ModelState.AddModelError("GiaTour", "Giá tour là không được để trống và phải là số dương.");
            }
            else if (model.GiaTour < 50)
            {
                ModelState.AddModelError("GiaTour", "Giá tour phải từ 50 USD trở lên.");
            }
            if (string.IsNullOrWhiteSpace(model.BaiViet))
            {
                ModelState.AddModelError("BaiViet", "Mô tả tour là không được để trống.");
            }
            // Nếu tất cả điều kiện hợp lệ
            if (ModelState.IsValid)
            {
                try
                {
                    db.TourDuLiches.Add(model);
                    db.SaveChanges();

                    
                    return RedirectToAction("List", "Tour", new { area = "Contributors" });
                }
                catch (DbUpdateException ex)
                {
                    string errorMessage = "Có lỗi xảy ra khi lưu dữ liệu.";
                    Exception inner = ex;
                    while (inner != null)
                    {
                        errorMessage += " --> " + inner.Message;
                        inner = inner.InnerException;
                    }
                    ModelState.AddModelError("", errorMessage);
                    System.Diagnostics.Debug.WriteLine(errorMessage);
                }
                catch (Exception ex)
                {
                    string errorMessage = ex.Message;
                    ModelState.AddModelError("", errorMessage);
                    System.Diagnostics.Debug.WriteLine($"Exception: {errorMessage}");
                }
            }

            // Trả về view với các thông báo lỗi nếu có
            return View(model);
        }


        // GET: Contributors/Tour/Update/5
        public ActionResult Update(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            TourDuLich tour = db.TourDuLiches.Find(id);
            if (tour == null)
            {
                return HttpNotFound();
            }

            return View(tour);
        }

        // POST: Contributors/Tour/Update/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(TourDuLich tour)
        {
            // Lấy UserID từ session
            var contributor = Session["contributor"] as Contributor;
            if (contributor == null)
            {
                return RedirectToAction("Login", "Auth", new { area = "Contributors" });
            }

            // Kiểm tra điều kiện cho từng thuộc tính
            if (string.IsNullOrWhiteSpace(tour.TieuDe) || tour.TieuDe.Length < 10)
            {
                ModelState.AddModelError("TieuDe", "Tiêu đề là bắt buộc và phải có ít nhất 10 ký tự.");
            }

            if (string.IsNullOrWhiteSpace(tour.HinhAnh))
            {
                ModelState.AddModelError("HinhAnh", "Phải có ít nhất một hình ảnh.");
            }

            if (!tour.TripStart.HasValue)
            {
                ModelState.AddModelError("TripStart", "Thời gian khởi hành là bắt buộc.");
            }

            if (!tour.TripEnd.HasValue)
            {
                ModelState.AddModelError("TripEnd", "Thời gian kết thúc là bắt buộc.");
            }
            else if (tour.TripStart.HasValue && tour.TripEnd.Value <= tour.TripStart.Value)
            {
                ModelState.AddModelError("TripEnd", "Thời gian kết thúc phải sau thời gian khởi hành.");
            }

            if (string.IsNullOrWhiteSpace(tour.LichTrinh))
            {
                ModelState.AddModelError("LichTrinh", "Lịch trình là bắt buộc.");
            }

            if (string.IsNullOrWhiteSpace(tour.DiaDiem))
            {
                ModelState.AddModelError("DiaDiem", "Địa điểm là bắt buộc.");
            }

            if (!tour.SoNguoiToiDa.HasValue || tour.SoNguoiToiDa.Value <= 0)
            {
                ModelState.AddModelError("SoNguoiToiDa", "Số người tối đa là bắt buộc và phải là số nguyên dương.");
            }

            if (string.IsNullOrWhiteSpace(tour.PhuongTien))
            {
                ModelState.AddModelError("PhuongTien", "Phương tiện là bắt buộc.");
            }
            else
            {
                // Biểu thức chính quy để kiểm tra chỉ cho phép chữ cái và khoảng trắng
                var regex = new System.Text.RegularExpressions.Regex(@"^[a-zA-Z\s]+$");
                if (!regex.IsMatch(tour.PhuongTien))
                {
                    ModelState.AddModelError("PhuongTien", "Phương tiện chỉ được phép chứa các ký tự chữ cái và khoảng trắng.");
                }
            }

            if (!tour.GiaTour.HasValue || tour.GiaTour <= 0)
            {
                ModelState.AddModelError("GiaTour", "Giá tour là bắt buộc và phải là số dương.");
            }
            else if (tour.GiaTour < 50)
            {
                ModelState.AddModelError("GiaTour", "Giá tour phải từ 50 USD trở lên.");
            }
            if (string.IsNullOrWhiteSpace(tour.BaiViet))
            {
                ModelState.AddModelError("BaiViet", "Mô tả tour là không được để trống.");
            }
            // Nếu tất cả điều kiện hợp lệ
            if (ModelState.IsValid)
            {
                // Tìm tour hiện tại trong cơ sở dữ liệu
                var existingTour = db.TourDuLiches.Find(tour.ID);
                if (existingTour != null)
                {
                    // Cập nhật các trường thông tin
                    existingTour.TieuDe = tour.TieuDe;
                    existingTour.BaiViet = tour.BaiViet;
                    existingTour.DiaDiem = tour.DiaDiem;
                    existingTour.GiaTour = tour.GiaTour;
                    existingTour.HinhAnh = tour.HinhAnh;
                    existingTour.idLoaiTour = tour.idLoaiTour;
                    existingTour.idTinh = tour.idTinh;
                    existingTour.PhuongTien = tour.PhuongTien;
                    existingTour.idMucGia = tour.idMucGia;
                    existingTour.LichTrinh = tour.LichTrinh;
                    existingTour.TripStart = tour.TripStart;
                    existingTour.TripEnd = tour.TripEnd;
                    existingTour.SoNguoiToiDa = tour.SoNguoiToiDa;

                    // Gán UserID cho tour
                    existingTour.ContributorID = contributor.ContributorID;

                    // Đảm bảo trạng thái là null
                    existingTour.Status = "N";

                    // Lưu thay đổi vào cơ sở dữ liệu
                    db.SaveChanges();
                    
                    
                    return RedirectToAction("List", "Tour", new { area = "Contributors" });
                }
            }

            // Trả về view với các thông báo lỗi nếu có
            return View(tour);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TourDuLich booking = db.TourDuLiches.Find(id);
            if (booking == null)
            {
                return HttpNotFound();
            }
            return View(booking);

        }
        // POST: Contributors/Tour/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            TourDuLich tour = db.TourDuLiches.Find(id);
            if (tour != null)
            {
                db.TourDuLiches.Remove(tour);
                db.SaveChanges();
            }
            return RedirectToAction("List", "Tour", new { area = "Contributors" });
        }

        public ActionResult Search(string tourName, string location, DateTime? startDate, DateTime? endDate,int? idTinh, int? idLoaiTour, int? idMucGia)
        {


            // Lấy thông tin contributor từ session
            var contributor = Session["contributor"] as Contributor;

            if (contributor == null)
            {
                // Nếu không có contributor trong session, có thể redirect hoặc trả về một thông báo
                return RedirectToAction("Login", "Account"); // Ví dụ: redirect đến trang đăng nhập
            }

            // Lấy danh sách các tour thuộc về contributor
            var tours = db.TourDuLiches
                .Where(t => t.ContributorID == contributor.ContributorID) // Lọc tour theo ContributorID
                .AsQueryable(); // Tạo truy vấn có thể mở rộng

            // Thêm các điều kiện tìm kiếm
            if (!string.IsNullOrEmpty(tourName))
            {
                tours = tours.Where(t => t.TieuDe.Contains(tourName)); // Tìm theo tên tour
            }

            if (!string.IsNullOrEmpty(location))
            {
                tours = tours.Where(t => t.DiaDiem.Contains(location)); // Tìm theo địa điểm
            }

            if (startDate.HasValue)
            {
                tours = tours.Where(t => t.TripStart >= startDate.Value); // Tìm theo ngày khởi hành
            }

            if (endDate.HasValue)
            {
                tours = tours.Where(t => t.TripEnd <= endDate.Value); // Tìm theo ngày kết thúc
            }
            if (idLoaiTour.HasValue)
            {
                tours = tours.Where(t => t.idLoaiTour == idLoaiTour.Value); // Tìm theo mức giá
            }
            if (idMucGia.HasValue)
            {
                tours = tours.Where(t => t.idMucGia == idMucGia.Value); // Tìm theo mức giá
            }

            // Gán ViewBag để giữ giá trị tìm kiếm
            ViewBag.idTinh = idTinh;
            ViewBag.idLoaiTour = idLoaiTour;
            ViewBag.idMucGia = idMucGia;

            // Trả về view với danh sách tour tìm được
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
            // Lấy contributor hiện tại từ session
            var contributor = Session["contributor"] as Contributor;
            if (contributor == null)
            {
                return RedirectToAction("Login", "Auth", new { area = "Contributors" });
            }

            // Đánh dấu tất cả thông báo của contributor hiện tại là đã đọc
            var notifications = db.Notifications
                                    .Where(n => n.ContributorID == contributor.ContributorID && !(n.IsRead ?? false))
                                    .ToList();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }

            db.SaveChanges(); // Lưu thay đổi vào cơ sở dữ liệu

            return RedirectToAction("List", "Tour", new { area = "Contributors" }); // Quay lại danh sách tour
        }

    }
}
