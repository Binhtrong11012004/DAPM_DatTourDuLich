using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DAPMDuLich.Models
{
    public class mapMucGia
    {
        public List<MucGia> DanhSach()
        {
            return new DAPMDuLichEntities().MucGias.ToList();
        }
    }
}