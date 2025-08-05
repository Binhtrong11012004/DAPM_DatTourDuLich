using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DAPMDuLich.Models
{
    public class mapTour
    {
        public string message = "";
        // Danh sách theo tỉnh thành
        public List<TourDuLich> DanhSachTheoTinh(int? idTinh)
        {
            DAPMDuLichEntities db = new DAPMDuLichEntities();
            var data = (from tour in db.TourDuLiches
                        join tinh in db.TinhThanhs on tour.idTinh equals tinh.ID_Tinh
                        into tinh2
                        from tinh in tinh2.DefaultIfEmpty()

                        where tour.idTinh == idTinh
                        select tour
                        ).ToList();

            return data;
        }
        // Danh sách theo loại tour
        public List<TourDuLich> DanhSachTheoLoai(int? idLoaiTour)
        {
            DAPMDuLichEntities db = new DAPMDuLichEntities();
            var data = (from tour in db.TourDuLiches
                        where tour.idLoaiTour == idLoaiTour
                        select tour
                        ).ToList();

            return data;
        }
        // Danh sách theo mức giá
        public List<TourDuLich> DanhSachTheoMucGia(int? idMucGia)
        {
            DAPMDuLichEntities db = new DAPMDuLichEntities();
            var data = (from tour in db.TourDuLiches
                        where tour.idMucGia == idMucGia
                        select tour
                        ).ToList();
            return data;
        }



        // Danh sách theo tỉnh, loại tour, mức giá
        public List<TourDuLich> DanhSach(int? idTinh, int? idLoaiTour, int? idMucGia)
        {
            DAPMDuLichEntities db = new DAPMDuLichEntities();
            var data = db.TourDuLiches.AsQueryable();// doi duoi dang du lieu IQueryable de truy van linh hoat hon

            if (idTinh.HasValue)
            {
                data = data.Where(t => t.idTinh == idTinh.Value);
                Console.WriteLine("Filter by Tinh: " + idTinh.Value);
            }

            if (idLoaiTour.HasValue)
            {
                data = data.Where(t => t.idLoaiTour == idLoaiTour.Value);
                Console.WriteLine("Filter by Loai Tour: " + idLoaiTour.Value);
            }

            if (idMucGia.HasValue)
            {
                data = data.Where(t => t.idMucGia == idMucGia.Value);
                Console.WriteLine("Filter by Muc Gia: " + idMucGia.Value);
            }

            var result = data.ToList();
            Console.WriteLine("Result count: " + result.Count);

            return result;
        }


        public bool ThemMoi(TourDuLich model)
        {
            DAPMDuLichEntities db = new DAPMDuLichEntities();
            try
            {
                if (string.IsNullOrEmpty(model.TieuDe) == true)// neu nhu TieuDe bi trong thi bao loi
                {
                    message = "Thiếu thông tin tiêu đề";
                    return false;
                }
                db.TourDuLiches.Add(model);
                db.SaveChanges();
                return true;

            }
            catch
            {
                return false;
            }
        }
        public bool CapNhat(TourDuLich model)
        {
            try
            {
                DAPMDuLichEntities db = new DAPMDuLichEntities();
                var updateModel = db.TourDuLiches.Find(model.ID);
                // Kiểm tra null 
                if (updateModel == null)
                {
                    return false;
                }
                // Cập nhật giá trị cho các trường thông tin 
                updateModel.TieuDe = model.TieuDe;
                updateModel.BaiViet = model.BaiViet;
                updateModel.DiaDiem = model.DiaDiem;
                updateModel.GiaTour = model.GiaTour;
                updateModel.HinhAnh = model.HinhAnh;
                updateModel.idLoaiTour = model.idLoaiTour;
                updateModel.idTinh = model.idTinh;
                updateModel.PhuongTien = model.PhuongTien;
                updateModel.idMucGia = model.idMucGia;
                updateModel.LichTrinh = model.LichTrinh;
                updateModel.TripStart = model.TripStart;
                updateModel.TripEnd = model.TripEnd;
                updateModel.SoNguoiToiDa = model.SoNguoiToiDa;
                // Gán UserID từ model và set Status là null
                updateModel.ContributorID = model.ContributorID; // Gán ContributorID
                updateModel.Status = null; // Trạng thái chờ duyệt
                db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public TourDuLich ChiTiet(int idTour)
        {
            DAPMDuLichEntities db = new DAPMDuLichEntities();
            //  return db.TourDuLiches.Find(idTour);
            return db.TourDuLiches.SingleOrDefault(item => item.ID == idTour);
        }
        public bool Xoa(int idTour)
        {
            DAPMDuLichEntities db = new DAPMDuLichEntities();
            var model = db.TourDuLiches.Find(idTour);
            if (model != null)
            {
                db.TourDuLiches.Remove(model);
                db.SaveChanges();
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}