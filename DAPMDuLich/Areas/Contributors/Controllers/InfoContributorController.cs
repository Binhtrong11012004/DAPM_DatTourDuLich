using DAPMDuLich.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace DAPMDuLich.Areas.Contributors.Controllers
{
    public class InfoContributorController : Controller
    {
        private DAPMDuLichEntities db = new DAPMDuLichEntities();

        // GET: Contributors/InfoContributor
        public ActionResult ViewProfile()
        {
            // Lấy thông tin Contributor từ session
            var contributor = Session["contributor"] as Contributor;

            if (contributor == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            return View(contributor);
        }

        // GET: InfoContributor/Edit
        public async Task<ActionResult> Edit()
        {
            var contributor = (Contributor)HttpContext.Session["contributor"];
            if (contributor == null)
            {
                return Content("Không tìm thấy thông tin Contributor trong session.");
            }

            var contributorInDb = await db.Contributors.SingleOrDefaultAsync(x => x.ContributorID == contributor.ContributorID);
            if (contributorInDb == null)
            {
                return Content("Contributor không tồn tại trong cơ sở dữ liệu.");
            }

            return View(contributorInDb);
        }

        // POST: InfoContributor/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "ContributorID,ContributorNickName,ContributorEmail,ContributorPhone,ContributorAddress,ContributorPassword,Tien")] Contributor contributorFromForm)
        {
            var contributorSession = (Contributor)HttpContext.Session["contributor"];
            if (contributorSession == null)
            {
                return Content("Không tìm thấy thông tin Contributor trong session.");
            }

            if (ModelState.IsValid)
            {
                var existingContributor = await db.Contributors.SingleOrDefaultAsync(x => x.ContributorID == contributorSession.ContributorID);
                if (existingContributor == null)
                {
                    return Content("Contributor không tồn tại trong cơ sở dữ liệu.");
                }

                // Giữ các giá trị không thay đổi
                contributorFromForm.ContributorID = existingContributor.ContributorID;
                contributorFromForm.Tien = existingContributor.Tien;
                contributorFromForm.ContributorNickName = existingContributor.ContributorNickName;
                contributorFromForm.ContributorEmail = existingContributor.ContributorEmail;
                contributorFromForm.ContributorPhone = existingContributor.ContributorPhone;
                contributorFromForm.ContributorAddress = existingContributor.ContributorAddress;
                // Cập nhật các thuộc tính khác từ form


                // Cập nhật mật khẩu nếu có
                if (!string.IsNullOrEmpty(contributorFromForm.ContributorPassword))
                {
                    existingContributor.ContributorPassword = contributorFromForm.ContributorPassword; // Cập nhật mật khẩu mới
                }

                db.Entry(existingContributor).State = EntityState.Modified;
                await db.SaveChangesAsync();

                // Cập nhật lại thông tin trong session
                HttpContext.Session["contributor"] = existingContributor;

                return RedirectToAction("ViewProfile");
            }

            return View(contributorFromForm);
        }
    }
}