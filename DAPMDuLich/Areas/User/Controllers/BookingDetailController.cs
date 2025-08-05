using DAPMDuLich.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using DAPMDuLich.Models.ViewModel;
using PayPal.Api;
using System.IO;
namespace DAPMDuLich.Areas.User.Controllers
{
    public class BookingDetailController : Controller
    {
        // GET: User/BookingDetail
        private DAPMDuLichEntities db = new DAPMDuLichEntities();
        // GET: DatTourChiTIet
        public ActionResult Index()
        {
            var bookingDetail = db.DatTourChiTiets.Include(b => b.DatTour);
            return View(bookingDetail.ToList());
        }
        // GET: User/BookingDetails/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DatTourChiTiet bookingDetail = db.DatTourChiTiets.SingleOrDefault(x => x.BookingID == id);
            DatTour booking = db.DatTours.SingleOrDefault(b => b.BookingID == bookingDetail.BookingID);
            TourDuLich tour = db.TourDuLiches.SingleOrDefault(t => t.ID == booking.ID);

            if (bookingDetail == null || booking == null || tour == null)
            {
                return HttpNotFound();
            }

            ViewBag.TourName = tour.TieuDe;
            ViewBag.TripStart = tour.TripStart;
            ViewBag.TripEnd = tour.TripEnd;
            ViewBag.TripType = tour.PhuongTien;
            ViewBag.TravelerCount = bookingDetail.TravelerCount;
            return View(bookingDetail);
        }
        public ActionResult ThanhToan(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            DatTourChiTiet bookingDetail = db.DatTourChiTiets.SingleOrDefault(x => x.BookingDetailID == id);
            if (bookingDetail == null)
            {
                return HttpNotFound();
            }

            DatTour booking = db.DatTours.SingleOrDefault(b => b.BookingID == bookingDetail.BookingID);
            if (booking == null)
            {
                return HttpNotFound();
            }

            // lay id khach hang tu bang TaiKhoanKh neu null thi no se bao k tim thay http
            TaiKhoan customer = db.TaiKhoans.SingleOrDefault(c => c.UserID == booking.UserID);
            if (customer == null)
            {
                return HttpNotFound();
            }

            if (customer.Tien >= bookingDetail.Price)
            {
                customer.Tien -= bookingDetail.Price.Value;
                booking.ThanhToan = true; // Cập nhật trạng thái thành đã thanh toán
                booking.MaThanhToan = GeneratePaymentCode(); // Tạo và lưu mã thanh toán
                booking.Status = true;
                // Lấy thông tin Contributor (người đóng góp)
                // Tính toán 95% cho Contributor và 5% cho Admin
                decimal contributorShare = bookingDetail.Price.Value * 0.95m; // 95% cho Contributor
                decimal adminShare = bookingDetail.Price.Value * 0.05m; // 5% cho Admin

                // Cập nhật tài khoản của Contributor
                var contributor = db.Contributors.SingleOrDefault(c => c.ContributorID == booking.ContributorID);
                if (contributor != null)
                {
                    contributor.Tien += contributorShare; // Cộng vào tài khoản Contributor
                }
                // Cập nhật tài khoản Admin (tìm Admin từ bảng TaiKhoan có Role = "Admin")
                var admin = db.TaiKhoans.SingleOrDefault(c => c.Role == "admin");
                if (admin != null)
                {
                    admin.Tien += adminShare; // Cộng vào tài khoản Admin
                }
                db.SaveChanges(); // Lưu thay đổi vào cơ sở dữ liệu
                ViewBag.Message = "Thanh toán thành công";
            }
            else
            {
                ViewBag.Message = "Không đủ tiền để thanh toán";
            }

            // Truyền thông tin cần thiết đến View
            ViewBag.TenDangNhap = customer.TenDangNhap;
            ViewBag.CreateAt = booking.CreateAt;
            ViewBag.ThanhToan = booking.ThanhToan;
            ViewBag.MaThanhToan = booking.MaThanhToan;

            return View("ThanhToan", bookingDetail);
        }
        //Paypal
        public ActionResult FailureView()
        {
            return View();
        }
        public ActionResult SuccessView()
        {
            return View();
        }

        public ActionResult PaymentWithPaypal(int? bookingDetailID, string Cancel = null)
        {
            // Lấy APIContext từ cấu hình PayPal
            APIContext apiContext = PaypalConfiguration.GetAPIContext();
            try
            {
                if (bookingDetailID.HasValue && string.IsNullOrEmpty(Request.Params["PayerID"]))
                {
                    // Lấy thông tin chi tiết đặt tour từ cơ sở dữ liệu
                    DatTourChiTiet bookingDetail = db.DatTourChiTiets.SingleOrDefault(x => x.BookingDetailID == bookingDetailID.Value);
                    if (bookingDetail == null)
                    {
                        return HttpNotFound();
                    }

                    // Lấy thông tin đặt tour
                    DatTour booking = db.DatTours.SingleOrDefault(b => b.BookingID == bookingDetail.BookingID);
                    if (booking == null)
                    {
                        return HttpNotFound();
                    }

                    // Lấy thông tin tour du lịch
                    TourDuLich tour = db.TourDuLiches.SingleOrDefault(t => t.ID == booking.ID);
                    if (tour == null)
                    {
                        return HttpNotFound();
                    }

                    // Tạo ViewModel từ dữ liệu đặt tour
                    var viewDetail = new OrderHistoryDetailViewModel()
                    {
                        BookingDetailID = bookingDetail.BookingDetailID,
                        BookingID = bookingDetail.BookingID,
                        Price = (double)bookingDetail.Price.GetValueOrDefault(),
                        TravelerCount = bookingDetail.TravelerCount,
                        CreateAt = bookingDetail.CreateAt,
                        ThanhToan = booking.ThanhToan.GetValueOrDefault(),
                        MaThanhToan = booking.MaThanhToan,
                        UserID = booking.UserID.GetValueOrDefault(),
                        ID = booking.ID.GetValueOrDefault(),
                    };

                    // Lưu thông tin đặt tour vào Session
                    Session["BookingDetail"] = viewDetail;

                    // Log để xác nhận Session đã được thiết lập
                    Console.WriteLine("Session['BookingDetail'] set successfully.");
                }

                // Xử lý thanh toán
                string payerId = Request.Params["PayerID"];

                if (string.IsNullOrEmpty(payerId))
                {
                    // Tạo một thanh toán mới
                    string baseURI = Url.Action("PaymentWithPaypal", "BookingDetail", new { area = "User" }, Request.Url.Scheme);
                    var guid = Convert.ToString((new Random()).Next(100000));

                    // Sử dụng UriBuilder để thêm tham số query một cách an toàn
                    UriBuilder uriBuilder = new UriBuilder(baseURI);
                    var query = HttpUtility.ParseQueryString(uriBuilder.Query);
                    query["guid"] = guid;
                    uriBuilder.Query = query.ToString();
                    string redirectUrl = uriBuilder.ToString();

                    // Tạo Payment object từ PayPal
                    var createdPayment = this.CreatePayment(apiContext, redirectUrl);

                    // Tìm URL redirect tới PayPal
                    var links = createdPayment.links.GetEnumerator();
                    string paypalRedirectUrl = null;

                    while (links.MoveNext())
                    {
                        Links lnk = links.Current;
                        if (lnk.rel.ToLower().Trim().Equals("approval_url"))
                        {
                            paypalRedirectUrl = lnk.href;
                        }
                    }

                    if (string.IsNullOrEmpty(paypalRedirectUrl))
                    {
                        throw new Exception("No approval_url found in PayPal payment creation response.");
                    }

                    // Lưu Payment ID và BookingDetailID vào Session với key là guid
                    Session.Add(guid, createdPayment.id);
                    if (Session["BookingDetail"] is OrderHistoryDetailViewModel sessionBookingDetail)
                    {
                        Session.Add($"BookingDetail_{guid}", sessionBookingDetail.BookingDetailID);
                    }

                    return Redirect(paypalRedirectUrl);
                }
                else
                {
                    // Thực thi thanh toán sau khi nhận được PayerID từ PayPal
                    var guid = Request.Params["guid"];
                    var executedPayment = ExecutePayment(apiContext, payerId, Session[guid] as string);

                    if (executedPayment.state.ToLower() != "approved")
                    {
                        Console.WriteLine("Payment state: " + executedPayment.state);
                        return View("FailureView");
                    }

                    // Truy xuất BookingDetailID từ Session
                    var bookingDetailIdFromSession = Session[$"BookingDetail_{guid}"] as int?;

                    if (bookingDetailIdFromSession.HasValue)
                    {
                        DatTourChiTiet bookingDetail = db.DatTourChiTiets.SingleOrDefault(x => x.BookingDetailID == bookingDetailIdFromSession.Value);
                        if (bookingDetail != null)
                        {
                            DatTour booking = db.DatTours.SingleOrDefault(b => b.BookingID == bookingDetail.BookingID);
                            if (booking != null)
                            {
                                // Cập nhật trạng thái thanh toán
                                booking.ThanhToan = true;
                                booking.MaThanhToan = executedPayment.id;                             
                                db.SaveChanges();

                            }

                        }
                    }
                    else
                    {
                        // Log lỗi hoặc xử lý khi BookingDetailID không tồn tại trong Session
                        Console.WriteLine("BookingDetailID not found in Session.");
                        return View("FailureView");
                    }

                    // Truy xuất thông tin đặt tour từ Session để điều hướng
                    var sessionBookingDetailRedirect = Session["BookingDetail"] as OrderHistoryDetailViewModel;

                    // Điều hướng người dùng đến trang lịch sử đơn hàng
                    return RedirectToAction("History", "Booking", new { id = sessionBookingDetailRedirect?.ID, userID = sessionBookingDetailRedirect?.UserID });
                }
            }
            catch (Exception ex)
            {
                // Ghi log lỗi vào file
                string filePath = Server.MapPath("~/Logs/ErrorLog.txt");
                using (StreamWriter writer = new StreamWriter(filePath, true))
                {
                    writer.WriteLine("Date: " + DateTime.Now.ToString());
                    writer.WriteLine("Message: " + ex.Message);
                    writer.WriteLine("StackTrace: " + ex.StackTrace);
                    writer.WriteLine();
                }
                return View("FailureView");
            }
        }

        private PayPal.Api.Payment payment;
        private Payment ExecutePayment(APIContext apiContext, string payerId, string paymentId)
        {
            var paymentExecution = new PaymentExecution()
            {
                payer_id = payerId
            };
            this.payment = new Payment()
            {
                id = paymentId
            };
            return this.payment.Execute(apiContext, paymentExecution);
        }
        private Payment CreatePayment(APIContext apiContext, string redirectUrl)
        {
            var viewDetail = Session["BookingDetail"] as OrderHistoryDetailViewModel;
            if (viewDetail == null)
            {
                Console.WriteLine("No booking detail found in session.");
                throw new Exception("No booking detail found in session.");
            }
            // Create itemlist and add a single item object representing the tour booking  
            var itemList = new ItemList()
            {
                items = new List<Item>()
            };

            // Adding Item Details like name, currency, price, and traveler count  
            itemList.items.Add(new Item()
            {
                name = $"Tour Booking #{viewDetail.BookingID}", // Name of the tour
                currency = "USD",
                price = viewDetail.Price.ToString("F2"), // Tour price per traveler
                quantity = viewDetail.TravelerCount.ToString(), // Number of travelers
                sku = viewDetail.BookingDetailID.ToString() // Booking detail ID as SKU
            });

            var payer = new Payer()
            {
                payment_method = "paypal"
            };

            // Configure Redirect Urls here with RedirectUrls object  
            var redirUrls = new RedirectUrls()
            {
                cancel_url = redirectUrl + "&Cancel=true",
                return_url = redirectUrl
            };

            // Adding Tax, shipping, and Subtotal details (assuming no tax or shipping)
            var details = new Details()
            {
                tax = "0",
                shipping = "0",
                subtotal = (viewDetail.Price * viewDetail.TravelerCount).ToString("F2")
            };

            // Final amount with details  
            var amount = new Amount()
            {
                currency = "USD",
                total = (viewDetail.Price * viewDetail.TravelerCount).ToString("F2"), // Total amount based on traveler count
                details = details
            };

            var transactionList = new List<Transaction>();

            // Adding description about the transaction  
            var paypalOrderId = DateTime.Now.Ticks;
            transactionList.Add(new Transaction()
            {
                description = $"Tour Booking Invoice #{paypalOrderId}",
                invoice_number = paypalOrderId.ToString(), // Generate an Invoice No    
                amount = amount,
                item_list = itemList
            });

            this.payment = new Payment()
            {
                intent = "sale",
                payer = payer,
                transactions = transactionList,
                redirect_urls = redirUrls
            };

            // Create a payment using a APIContext  
            return this.payment.Create(apiContext);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private int GetTotalNumber()
        {
            var viewDetail = Session["BookingDetail"] as OrderHistoryDetailViewModel;
            if (viewDetail == null)
            {
                throw new Exception("BookingDetail not found.");
            }

            // Return the number of travelers from the booking detail
            return viewDetail.TravelerCount;
        }
        public ActionResult ThanhToanPartial(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            DatTourChiTiet bookingDetail = db.DatTourChiTiets.SingleOrDefault(x => x.BookingDetailID == id);
            if (bookingDetail == null)
            {
                return HttpNotFound();
            }

            DatTour booking = db.DatTours.SingleOrDefault(b => b.BookingID == bookingDetail.BookingID);
            if (booking == null)
            {
                return HttpNotFound();
            }

            TaiKhoan customer = db.TaiKhoans.SingleOrDefault(c => c.UserID == booking.UserID);
            if (customer == null)
            {
                return HttpNotFound();
            }

            decimal paymentAmount = bookingDetail.Price.Value * 0.5m; // 50% payment

            if (customer.Tien >= paymentAmount)
            {
                // Trừ tiền của khách hàng
                customer.Tien -= paymentAmount;

                // Cập nhật trạng thái ThanhToan50 cho lần thanh toán đầu tiên
                booking.ThanhToan50 = true;
                booking.Status = true;

                db.SaveChanges(); // Lưu thay đổi sau khi thanh toán

                // Lần thanh toán đầu tiên, cộng 50% vào tài khoản Contributor
                var contributor = db.Contributors.SingleOrDefault(c => c.ContributorID == booking.ContributorID);
                if (contributor != null)
                {
                    contributor.Tien += paymentAmount; // Cộng 50% vào tài khoản Contributor ngay lần 1
                    db.SaveChanges(); // Lưu thay đổi sau khi cộng tiền vào tài khoản Contributor
                }

                // Kiểm tra nếu đã thanh toán lần 2 (100%), tính hoa hồng cho Contributor và Admin
                if (booking.ThanhToan == true && booking.ThanhToan50 == true)
                {
                    decimal contributorShare = paymentAmount * 0.95m; // 95% cho Contributor
                    decimal adminShare = paymentAmount * 0.05m; // 5% cho Admin

                    // Cập nhật tài khoản Contributor
                    if (contributor != null)
                    {
                        contributor.Tien += contributorShare; // Cộng vào tài khoản Contributor
                    }

                    // Cập nhật tài khoản Admin
                    var admin = db.TaiKhoans.SingleOrDefault(c => c.Role == "admin");
                    if (admin != null)
                    {
                        admin.Tien += adminShare; // Cộng vào tài khoản Admin
                    }

                    db.SaveChanges(); // Lưu thay đổi sau khi cộng hoa hồng
                    ViewBag.Message = "Thanh toán 100% thành công";
                }
                else
                {
                    // Chỉ thông báo đã thanh toán 50% và cộng vào tài khoản Contributor lần đầu
                    ViewBag.Message = "Thanh toán 50% thành côngg";
                }
            }
            else
            {
                ViewBag.Message = "Không đủ tiền để thanh toán";
            }

            ViewBag.TenDangNhap = customer.TenDangNhap;
            ViewBag.CreateAt = booking.CreateAt;
            ViewBag.ThanhToan = booking.ThanhToan;
            ViewBag.ThanhToan50 = booking.ThanhToan50;

            return View("ThanhToan", bookingDetail);
        }


        private string GeneratePaymentCode()
        {
            return Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper(); // Tạo mã thanh toán ngẫu nhiên
        }

        public ActionResult CreatePaidNotification(int bookingID)//gui thong bao khi thanh toan thanh cong
        {
            // Lấy thông tin đặt tour từ BookingID
            var booking = db.DatTours.SingleOrDefault(d => d.BookingID == bookingID);

            if (booking != null && booking.ThanhToan == true) // Kiểm tra nếu đã thanh toán
            {
                // Tạo thông báo mới
                DAPMDuLich.Models.Notification myNotification = new DAPMDuLich.Models.Notification
                {
                    ContributorID = booking.ContributorID ?? 0, // Đảm bảo ContributorID không null
                    UserID = booking.UserID ?? 0,               // Đảm bảo UserID không null
                    ID = booking.ID ?? 0,                       // Đảm bảo ID không null
                    BookingID = booking.BookingID,
                    IsRead = false,
                    CreateAt = DateTime.Now,
                    ReceiverRole="Contributor"
                    
                };

                // Thêm thông báo vào cơ sở dữ liệu
                db.Notifications.Add(myNotification);
                db.SaveChanges();

                // Trả về thông báo thành công
                TempData["SuccessMessage"] = "Thông báo thanh toán đã được tạo.";
            }
            else
            {
                // Trả về thông báo lỗi nếu không tìm thấy hoặc chưa thanh toán
                TempData["ErrorMessage"] = "Đơn hàng chưa thanh toán hoặc không tìm thấy.";
            }

            return RedirectToAction("Index");
        }
    }
}
