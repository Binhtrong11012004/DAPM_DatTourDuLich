using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DAPMDuLich.Areas.Admin.Data.ViewModel
{
    public class AdminStatisticsViewModel
    {
        // Tổng quan về bài viết
        public int TotalPosts { get; set; }
        public int TotalApprovedPosts { get; set; }
        public int TotalPendingPosts { get; set; }
        public int TotalRejectedPosts { get; set; }

        // Hiệu suất của contributors
        public List<TopContributor> TopContributors { get; set; }
        public Dictionary<int, int> RejectedPostsByContributor { get; set; }
        public Dictionary<string, int> PostsByTourType { get; set; }

        // Đặt tour
        public int TotalBookings { get; set; }
        public int TotalPaidBookings { get; set; }
        public int TotalUnpaidBookings { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalAdminCommission { get; set; }
        public class TopContributor
        {
            public int ContributorID { get; set; }
            public string ContributorName { get; set; }
            public int PostCount { get; set; }
            public int ApprovedPostCount { get; set; }
        }
    }
}