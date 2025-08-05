using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DAPMDuLich.Models.ViewModel
{
    public class BookingViewModel
    {
        public string HoTen { set; get; }
        public string Email { set; get; }
        public string Ten { get; set; }
        public int ID { set; get; }
        public string TieuDe { set; get; }
        public string BaiViet { get; set; }
        public string PhuongTien { get; set; }
        public DateTime? TripStart { set; get; }
        public DateTime? TripEnd { set; get; }

        public decimal? GiaTour { get; set; }
        public Nullable<int> SoNgayDiTour { get; set; }
        public string LichTrinh { get; set; }
        public string DiaDiem { get; set; }
        public Nullable<int> SoNguoiToiDa { get; set; }
        public DateTime? CreateAt { set; get; }
        [Required(ErrorMessage = "Vui lòng nhập số lượng người.")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng người phải lớn hơn 0.")]
        public int? TravelerCount { set; get; }
        public int ContributorID { get; set; } // Thêm ContributorID
        public string ContributorName { get; set; } // Thêm ContributorName
    }
}