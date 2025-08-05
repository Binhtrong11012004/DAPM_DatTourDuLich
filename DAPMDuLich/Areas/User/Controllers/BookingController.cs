using DAPMDuLich.Models.ViewModel;
using DAPMDuLich.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using DAPMDuLich.App_Start;
using System.Data.Entity.Validation;

namespace DAPMDuLich.Areas.User.Controllers
{
    public class BookingController : Controller
    {
        // GET: User/Booking
        // GET: DatTour
        DAPMDuLichEntities database = new DAPMDuLichEntities();

        [CheckPermissions(ChucNang = "TaiKhoan_ChiTiet")]
        public async Task<ActionResult> Create(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "ID is required.");
            }

            var user = (TaiKhoan)HttpContext.Session["user"];
            var name = user.TenDangNhap;

            var taiKhoanKH = await database.TaiKhoans.SingleOrDefaultAsync(x => x.TenDangNhap == name);
            var tourDuLich = await database.TourDuLiches.Include(t => t.Contributor) // Bao gồm thông tin Contributor
                                               .SingleOrDefaultAsync(t => t.ID == id);
            if (tourDuLich == null)
            {
                return HttpNotFound("Tour không tồn tại.");
            }

            var bookingViewModel = new BookingViewModel()
            {
                HoTen = taiKhoanKH.TenHienThi,
                Email = taiKhoanKH.Email,
                ID = tourDuLich.ID,
                TieuDe = tourDuLich.TieuDe,
                TripStart = tourDuLich.TripStart,
                TripEnd = tourDuLich.TripEnd,
                BaiViet = tourDuLich.BaiViet,
                PhuongTien = tourDuLich.PhuongTien,
                SoNguoiToiDa = tourDuLich.SoNguoiToiDa,
                GiaTour = tourDuLich.GiaTour,
            };

            return View(bookingViewModel);
        }

        [CheckPermissions(ChucNang = "TaiKhoan_ChiTiet")]
        [HttpPost]
        public async Task<ActionResult> Create(BookingViewModel model)
        {
            try
            {
                var userSession = (TaiKhoan)HttpContext.Session["user"];
                if (userSession == null)
                {
                    return Content("Tài khoản KH session không tìm thấy");
                }

                var user = await database.TaiKhoans.SingleOrDefaultAsync(x => x.TenDangNhap == userSession.TenDangNhap);
                if (user == null)
                {
                    return Content("Tài khoản KH không tìm thấy trong database");
                }

                // Kiểm tra tổng số người đã đặt tour hiện tại
                var totalCurrentBookings = SumTraveler(model.ID);
                if (!ModelState.IsValid || !model.TravelerCount.HasValue || model.TravelerCount <= 0)
                {
                    ModelState.AddModelError("TravelerCount", "Hãy nhập số lượng người lớn hơn 0.");
                    return View(model);
                }

                // Kiểm tra nếu tổng số người đã đặt tour cộng với số người trong đơn đặt mới vượt quá số lượng người tối đa
                if (totalCurrentBookings + model.TravelerCount > model.SoNguoiToiDa)
                {
                    ModelState.AddModelError("TravelerCount", "Tour đã đầy chỗ.");
                    return View(model);
                }

                // Lấy thông tin Tour để lấy ContributorID
                var tourDuLich = await database.TourDuLiches.SingleOrDefaultAsync(t => t.ID == model.ID);
                if (tourDuLich == null)
                {
                    return Content("Tour không tồn tại.");
                }

                int bookingId;
                Random random = new Random();
                do
                {
                    bookingId = random.Next(1, int.MaxValue);
                }
                while (await database.DatTourChiTiets.AnyAsync(dt => dt.BookingID == bookingId));

                var datTour = new DatTour()
                {
                    BookingID = bookingId,
                    UserID = user.UserID,
                    Status = false,
                    ThanhToan = false,
                    ID = model.ID,
                    CreateAt = DateTime.Now,
                    ContributorID = tourDuLich.ContributorID,
                   
                };

                database.DatTours.Add(datTour);
                await database.SaveChangesAsync();

                var datTourChiTiet = new DatTourChiTiet()
                {
                    BookingID = datTour.BookingID,
                    TravelerCount = model.TravelerCount.Value,
                    CreateAt = DateTime.Now,
                    Price = model.GiaTour * model.TravelerCount,
                };
                database.DatTourChiTiets.Add(datTourChiTiet);
                await database.SaveChangesAsync();

                var notification = new Notification()
                {
                    ContributorID = tourDuLich.ContributorID,
                    UserID=user.UserID,
                    ID=tourDuLich.ID,
                    BookingID=datTour.BookingID,
                    IsRead = false,
                    CreateAt= DateTime.Now,
                    ReceiverRole="Contributor"
                };
                database.Notifications.Add(notification);
                await database.SaveChangesAsync();  
                
                return RedirectToAction("History");
            }
            catch (DbEntityValidationException ex)
            {
                // Lấy danh sách các lỗi xác thực
                var errorMessages = ex.EntityValidationErrors
                    .SelectMany(x => x.ValidationErrors)
                    .Select(x => x.ErrorMessage);

                // Nối các thông báo lỗi lại thành chuỗi
                var fullErrorMessage = string.Join("; ", errorMessages);
                var exceptionMessage = string.Concat(ex.Message, " Các lỗi xác thực: ", fullErrorMessage);

                // Trả về thông báo lỗi chi tiết
                return Content(exceptionMessage);
            }
        }
        private string GetInnermostExceptionMessage(Exception ex)
        {
            while (ex.InnerException != null)
            {
                ex = ex.InnerException;
            }
            return ex.Message;
        }
        // Phương thức để tính tổng số người đã đặt tour
        private int SumTraveler(int tourID)
        {
            var query = database.Database.SqlQuery<int>(
                "SELECT SUM(TravelerCount) " +
                "FROM DatTourChiTiet as BD " +
                "WHERE BD.BookingID IN (" +
                    "SELECT B.BookingID " +
                    "FROM DatTour as B " +
                    "WHERE B.ID = @p0 AND B.Status = 1)", tourID);

            try
            {
                var result = query.FirstOrDefault();
                return result;
            }
            catch
            {
                return 0;
            }
        }

        [CheckPermissions(ChucNang = "TaiKhoan_ChiTiet")]
        public ActionResult History()
        {
            var usermodel = (TaiKhoan)HttpContext.Session["user"];
            TaiKhoan user = database.TaiKhoans.SingleOrDefault(u => u.TenDangNhap == usermodel.TenDangNhap);

            var bookings = database.DatTours.Where(b => b.UserID == user.UserID).ToList();// lấy CustomerID của TaiKhoanKH và DatTour do khóa ngoại
            return View(bookings);
        }
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DatTour booking = database.DatTours.Find(id);
            if (booking == null)
            {
                return HttpNotFound();
            }
            return View(booking);
        }

        // POST: User/Bookings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            // Tìm booking theo ID từ bảng DatTour và DatTourChiTiet
            DatTour booking = database.DatTours.Include(b => b.DatTourChiTiets).FirstOrDefault(b => b.BookingID == id);

            if (booking != null)
            {
                // Xóa tất cả các bản ghi trong bảng Notifications có liên quan đến BookingID
                var notifications = database.Notifications.Where(n => n.BookingID == booking.BookingID).ToList();
                foreach (var notification in notifications)
                {
                    database.Notifications.Remove(notification);
                }

                // Lưu các thay đổi để xóa các thông báo trước khi xóa booking
                database.SaveChanges();

                // Xóa các chi tiết đặt tour
                foreach (var detail in booking.DatTourChiTiets.ToList())
                {
                    database.DatTourChiTiets.Remove(detail);
                }

                // Xóa booking
                database.DatTours.Remove(booking);

                // Lưu thay đổi lần nữa
                database.SaveChanges();
            }

            return RedirectToAction("History", "Booking", new { id = booking.ID });
        }
    }
}