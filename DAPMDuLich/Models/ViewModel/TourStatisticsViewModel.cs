using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace DAPMDuLich.Models.ViewModel
{
    public class TourStatisticsViewModel
    {
        public int TongSoDatTour { get; set; }
        public int TongSoDatTourDaThanhToan { get; set; }
        public int TongSoDatTourChuaThanhToan { get; set; }
        public decimal TongSoTienDaThanhToan { get; set; }
    }
}