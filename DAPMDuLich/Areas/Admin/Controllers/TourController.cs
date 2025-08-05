using DAPMDuLich.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DAPMDuLich.Areas.Admin.Controllers
{
    public class TourController : Controller
    {
        // GET: Admin/Tour
        public ActionResult List()
        {
            mapTour map = new mapTour();
            return View(map.DanhSach(null, null, null));// null null null là vì 3 tham số kia để nó hiện theo các id bên kia trong danhSach
        }
        public ActionResult Create()
        {
            return View(new TourDuLich());
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(TourDuLich model)
        {
            mapTour map = new mapTour();
            if (map.ThemMoi(model) == true)
            {
                return RedirectToAction("List");
            }
            else
            {
                // thông báo lỗi:
                ModelState.AddModelError("", map.message);
                return View(model);
            }
        }
        public ActionResult Update(int idTour)
        {
            // Tìm đối tượng cần sửa
            var map = new mapTour();
            var tourEdit = map.ChiTiet(idTour);
            return View(tourEdit);
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Update(TourDuLich model)
        {
            mapTour map = new mapTour();
            if (map.CapNhat(model) == true)
            {
                return RedirectToAction("List");
            }
            else
            {
                // thông báo lỗi:
                ModelState.AddModelError("", map.message);
                return View(model);
            }
        }

        public ActionResult Delete(int idTour)
        {
            // Gọi ham xoa trong Map
            mapTour map = new mapTour();
            map.Xoa(idTour);
            // Xóa xong quay về trang danh sách
            return Redirect("/Admin/Tour/List");
        }
    }
}
