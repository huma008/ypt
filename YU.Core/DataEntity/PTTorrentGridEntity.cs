﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using YU.Core.Attributes;

namespace YU.Core.DataEntity
{
    [Serializable]
    public class PTTorrentGridEntity
    {
        /// <summary>
        /// 站点
        /// </summary>
        [GridView("站点", false, 100)]
        public int SiteId { get; set; }

        /// <summary>
        /// 站点
        /// </summary>
        [GridView("站点", false, 100)]
        public string SiteName { get; set; }

        /// <summary>
        /// 版块
        /// </summary>
        [GridView("版块", true, 100)]
        public string ForumName { get; set; }

        /// <summary>
        /// 种子Id
        /// </summary>
        [GridView("种子Id", false, 100)]
        public string Id { get; set; }

        /// <summary>
        /// 资源类型
        /// </summary>
        [GridView("资源", true, 100)]
        public string ResourceType { get; set; }

        /// <summary>
        /// 种子详情页
        /// </summary>
        [GridView("种子详情页", false, 100)]
        public string LinkUrl { get; set; }

        /// <summary>
        /// 下载链接
        /// </summary>
        [GridView("下载链接", false, 100)]
        public string DownUrl { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        [GridView("标题", true, 700)]
        public string Title { get; set; }

        /// <summary>
        /// 促销类型
        /// </summary>
        [GridView("促销类型", false, 80)]

        public YUEnums.PromotionType PromotionType { get; set; }

        /// <summary>
        /// 促销类型
        /// </summary>
        [GridView("促销", true, 80, -1, typeof(DataGridViewImageColumn))]

        public Image PromotionType_Display { get; set; } 

        /// <summary>
        /// 大小
        /// </summary>
        [GridView("大小", true, 100)]
        public string Size_Display { get; set; }

        /// <summary>
        /// 大小
        /// </summary>
        [GridView("大小", false, 100)]
        public double Size { get; set; }

        /// <summary>
        /// 发布时间
        /// </summary>
        [GridView("时间", true, 140)]
        public DateTime UpLoadTime { get; set; }

        /// <summary>
        /// 做种人数
        /// </summary>
        [GridView("做种", true, 80)]
        public int SeederNumber { get; set; }

        /// <summary>
        /// 下载人数
        /// </summary>
        [GridView("下载", true, 80)]
        public int LeecherNumber { get; set; }

        /// <summary>
        /// 完成人数
        /// </summary>
        [GridView("完成", true, 80)]
        public int SnatchedNumber { get; set; }

        /// <summary>
        /// 发布者
        /// </summary>
        [GridView("发布", true, 100)]
        public string UpLoader { get; set; }

        /// <summary>
        /// HR
        /// </summary>
        [GridView("HR", false, 80)]

        public YUEnums.HRType IsHR { get; set; }

        /// <summary>
        /// HR
        /// </summary>
        [GridView("HR", true, 80, -1, typeof(DataGridViewImageColumn))]

        public Image IsHR_Display { get; set; }
    }
}
