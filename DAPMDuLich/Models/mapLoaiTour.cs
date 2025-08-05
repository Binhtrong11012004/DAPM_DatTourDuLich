using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DAPMDuLich.Models
{
    public class mapLoaiTour
    {
        DAPMDuLichEntities db = new DAPMDuLichEntities();

        public List<LoaiTour> DanhSach()
        {
            return db.LoaiTours.ToList();
        }
        // Lấy danh sách theo id cấp cha: Lấy danh sách con
        // id cấp cha = null => Lấy ra danh sách gốc
        public List<LoaiTour> DanhSachCapDuoi(int? idCapCha)
        {
            return db.LoaiTours.Where(m => m.idCapCha == idCapCha).ToList();
        }

        public LoaiTour ChiTiet(int idLoaiTour)
        {
            // tìm kiếm đổi tượng nếu có khóa là kiểu số, 1 khóa
            return db.LoaiTours.Find(idLoaiTour);
        }
    }
}