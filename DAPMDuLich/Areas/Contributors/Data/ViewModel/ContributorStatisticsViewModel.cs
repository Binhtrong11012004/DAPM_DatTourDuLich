using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DAPMDuLich.Areas.Contributors.Data.ViewModel
{
    public class ContributorStatisticsViewModel
    {
        public int TotalApprovedPosts { get; set; }
        public int TotalBookings { get; set; }
        public int TotalPaidBookings { get; set; }
        public int TotalUnpaidBookings { get; set; }
        public decimal TotalPaidAmount { get; set; }
        public int TotalBookingDetails { get; set; }
    }
}