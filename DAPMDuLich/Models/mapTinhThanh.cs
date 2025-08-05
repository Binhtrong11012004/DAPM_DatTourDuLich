using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DAPMDuLich.Models
{
    public class mapTinhThanh
    {
        public List<TinhThanh> DanhSach()
        {
            DAPMDuLichEntities db = new DAPMDuLichEntities();
            // Sử dụng query linq
            List<TinhThanh> data2 = (from tinh in db.TinhThanhs
                                     select tinh
                                     ).ToList();

            return data2;
        }
        // Lấy 2 tỉnh thành Có ID lớn nhất 
        public List<TinhThanh> Top2TinhThanh()
        {
            DAPMDuLichEntities db = new DAPMDuLichEntities();
            // Sử dụng query linq
            List<TinhThanh> data2 = (from tinh in db.TinhThanhs
                                     orderby tinh.ID_Tinh descending
                                     select tinh
                                     ).ToList().Take(2).ToList();

            return data2;
        }
        // Lọc dữ liệu với where
        public List<TinhThanh> DanhSachTheoTen(string ten)
        {
            DAPMDuLichEntities db = new DAPMDuLichEntities();
            // Sử dụng query linq
            List<TinhThanh> data2 = (from tinh in db.TinhThanhs
                                     where tinh.Ten.ToLower().Contains(ten.ToLower()) == true
                                     select tinh
                                     ).ToList();

            return data2;
        }
        //    public bool ThemMoi(TinhThanh model)
        //    {

        //        try
        //        {
        //            DuLichDBEntities db = new DuLichDBEntities();
        //            db.TinhThanhs.Add(model);
        //            db.SaveChanges();
        //            return true;
        //        }
        //        catch
        //        {
        //            return false;
        //        }
        //        //
        //    }

        //    public TinhThanh ChiTiet(int idTinh)
        //    {
        //        DuLichDBEntities db = new DuLichDBEntities();
        //        return db.TinhThanhs.Find(idTinh);
        //    }
        //}
    }
}
