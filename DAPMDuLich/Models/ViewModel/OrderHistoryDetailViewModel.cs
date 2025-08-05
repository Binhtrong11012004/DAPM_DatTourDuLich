using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DAPMDuLich.Models.ViewModel
{
    public class OrderHistoryDetailViewModel
    {
        public int BookingDetailID { get; set; }  // ID chi tiết đặt tour
        public int BookingID { get; set; }          // ID đặt tour
        public double Price { get; set; }           // Giá tour
        public int TravelerCount { get; set; }      // Số lượng khách
        public DateTime CreateAt { get; set; }      // Thời gian tạo đơn
        public bool ThanhToan { get; set; }            // Trạng thái thanh toán
        public string MaThanhToan { get; set; }         // Mã thanh toán 
        public int UserID { get; set; }
        public int ID { get; set; }
    }
}