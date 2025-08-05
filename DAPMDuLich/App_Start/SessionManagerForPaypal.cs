using DAPMDuLich.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DAPMDuLich.App_Start
{
    public class SessionManagerForPaypal
    {
        public static void SetBookingViewModel(OrderHistoryDetailViewModel viewModel)
        {
            HttpContext.Current.Session["OrderHistoryDetail"] = viewModel;
        }
        public static OrderHistoryDetailViewModel GetBookingViewModel()
        {
            return HttpContext.Current.Session["OrderHistoryDetail"] as OrderHistoryDetailViewModel;
        }
        public static void RemoveBookingViewModel()
        {
            HttpContext.Current.Session.Remove("OrderHistoryDetail");
        }
    }
}