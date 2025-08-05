using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DAPMDuLich.Models
{
    public class mapTaiKhoan
    {
        public string message = "";
        public List<TaiKhoan> DanhSach()
        {
            try
            {
                DAPMDuLichEntities db = new DAPMDuLichEntities();
                return db.TaiKhoans.ToList();
            }
            catch
            {
                return new List<TaiKhoan>();
            }
        }

        public bool CapNhat(TaiKhoan model)
        {
            try
            {
                DAPMDuLichEntities db = new DAPMDuLichEntities();
                var updateModel = db.TaiKhoans.Find(model.TenDangNhap);
                // Kiểm tra null 
                if (updateModel == null)
                {
                    return false;
                }
                // Cập nhật giá trị cho các trường thông tin 
                updateModel.TenDangNhap = model.TenDangNhap;
                updateModel.MatKhau = model.MatKhau;
                updateModel.TenHienThi = model.TenHienThi;
                updateModel.Email = model.Email;
                updateModel.SoDienThoai = model.SoDienThoai;
                updateModel.Active = model.Active;
                db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public TaiKhoan ChiTiet(String TenDangNhap)
        {
            DAPMDuLichEntities db = new DAPMDuLichEntities();
            //  
            return db.TaiKhoans.SingleOrDefault(item => item.TenDangNhap == TenDangNhap);
        }

        public bool AdminCapNhat(TaiKhoan model)
        {
            //1. Tìm đối tượng
            DAPMDuLichEntities db = new DAPMDuLichEntities();
            var updateModel = db.TaiKhoans.SingleOrDefault(m => m.TenDangNhap.ToLower() == model.TenDangNhap.ToLower());
            //2. Kiểm tra tồn tại
            if (updateModel == null)
            {
                return false;
            }
            //3. Cập nhật
            updateModel.SoDienThoai = model.SoDienThoai;
            updateModel.TenHienThi = model.TenHienThi;
            updateModel.Email = model.Email;
            db.SaveChanges();
            return true;
        }
        public bool DoiMatKhau(TaiKhoan model)
        {
            //1. Tìm đối tượng
            DAPMDuLichEntities db = new DAPMDuLichEntities();
            var updateModel = db.TaiKhoans.SingleOrDefault(m => m.TenDangNhap.ToLower() == model.TenDangNhap.ToLower());
            //2. Kiểm tra tồn tại
            if (updateModel == null)
            {
                return false;
            }
            //3. Cập nhật
            updateModel.MatKhau = model.MatKhau;
            db.SaveChanges();
            return true;
        }
    }
}