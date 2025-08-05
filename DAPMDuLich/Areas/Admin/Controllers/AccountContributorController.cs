using DAPMDuLich.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace DAPMDuLich.Areas.Admin.Controllers
{
    public class AccountContributorController : Controller
    {
        // GET: Admin/AccountContributor
        private DAPMDuLichEntities db = new DAPMDuLichEntities();
        public ActionResult List()
        {
            var admin = (TaiKhoan)Session["admin"];
            if (admin == null)
            {
                // Nếu không tồn tại admin session, CheckSessionAdmin sẽ tự động chuyển hướng
                // nên đoạn này sẽ không bao giờ chạy
                return RedirectToAction("Login", "Account", new { area = "Admin" });
            }
            return View(db.Contributors.ToList());
        }
        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ContributorID,ContributorName,ContributorPassword,ContributorNickName,ContributorEmail,ContributorPhone,ContributorAddress,CreateAt,Role,Active,Tien")] Contributor taiKhoanContributor)
        {
            if (ModelState.IsValid)
            {
                taiKhoanContributor.CreateAt = DateTime.Now;

                db.Contributors.Add(taiKhoanContributor);
                db.SaveChanges();
                return RedirectToAction("List");
            }

            return View(taiKhoanContributor);
        }
        // GET: Admin/TaiKhoan/Edit/5
        //Sửa tài khoản
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Contributor taiKhoanContributor = db.Contributors.Find(id);
            if (taiKhoanContributor == null)
            {
                return HttpNotFound();
            }
            return View(taiKhoanContributor);
        }
        // POST: Admin/TaiKhoanKH/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ContributorID,ContributorName,ContributorPassword,ContributorNickName,ContributorEmail,ContributorPhone,ContributorAddress,CreateAt,Role,Active,Tien")] Contributor taiKhoanContributor)
        {
            if (ModelState.IsValid)
            {
                var existingAccount = db.Contributors.Find(taiKhoanContributor.ContributorID);
                if (existingAccount == null)
                {
                    return HttpNotFound();
                }

                // Giữ giá trị TenDangNhap từ cơ sở dữ liệu
                taiKhoanContributor.ContributorName = existingAccount.ContributorName;
                taiKhoanContributor.CreateAt = existingAccount.CreateAt;

                existingAccount.ContributorPassword = taiKhoanContributor.ContributorPassword;
                existingAccount.ContributorNickName = taiKhoanContributor.ContributorNickName;
                existingAccount.ContributorEmail = taiKhoanContributor.ContributorEmail;
                existingAccount.ContributorPhone = taiKhoanContributor.ContributorPhone;
                existingAccount.ContributorAddress = taiKhoanContributor.ContributorAddress;
                existingAccount.Active = taiKhoanContributor.Active;
                existingAccount.Tien = taiKhoanContributor.Tien;

                db.Entry(existingAccount).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("List");
            }
            return View(taiKhoanContributor);
        }

        // GET: Admin/AccountContributor/Search
        public ActionResult Search(string searchQuery)
        {
            var contributors = db.Contributors.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                contributors = contributors.Where(c => c.ContributorName.Contains(searchQuery)
                                                   || c.ContributorNickName.Contains(searchQuery)
                                                   || c.ContributorEmail.Contains(searchQuery));
            }

            return View("List", contributors.ToList());
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}