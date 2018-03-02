﻿using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using YU.Core;
using YU.Core.DataEntity;
using YU.Core.Event;
using YU.Core.Utils;

namespace YPT.PT
{
    public class MTEAM : AbstractPT
    {

        public MTEAM(PTUser user)
            : base(user)
        {

        }

        protected override YUEnums.PTEnum SiteId
        {
            get
            {
                return YUEnums.PTEnum.MTEAM;
            }
        }

        public override string Login()
        {
            string postData = string.Format("username={0}&password={1}", User.UserName, User.PassWord);
            var result = HttpUtils.PostData(Site.LoginUrl, postData, _cookie);

            //这里没必要判断是否启用两步验证了，因为返回的结果Url就能知道是不是两步验证。 User.isEnableTwo_StepVerification
            if (Site.isEnableTwo_StepVerification && result.Item3 != null && result.Item3.ResponseUri.OriginalString.Contains("verify"))
            {
                OnTwoStepVerificationEventArgs e = new OnTwoStepVerificationEventArgs();
                e.Site = Site;
                string code = OnTwoStepVerification(e);
                if (code.IsNullOrEmptyOrWhiteSpace())
                    return "登录失败，无法获取正确的二级验证码。";
                else
                {
                    postData = string.Format("otp={0}", code);
                    _cookie = result.Item2.CookieContainer;
                    result = HttpUtils.PostData(result.Item3.ResponseUri.OriginalString, postData, _cookie);
                    return DoLogin(result);
                }
            }
            else
            {
                return DoLogin(result);
            }
        }

        private string DoLogin(Tuple<string, HttpWebRequest, HttpWebResponse> result)
        {
            string htmlResult = result.Item1;
            if (htmlResult.Contains(User.UserName) && (htmlResult.Contains("欢迎回来") || htmlResult.Contains("Welcome") || htmlResult.Contains("歡迎回來")))
            {
                User.Id = GetUserId(htmlResult);
                _cookie = result.Item2.CookieContainer;
                SetLocalCookie(_cookie);
                return "登录成功。";
            }
            else
            {
                HtmlDocument htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(htmlResult);//加载HTML字符串，如果是文件可以用htmlDocument.Load方法加载
                HtmlNode node = htmlDocument.DocumentNode.SelectSingleNode("//*[@id=\"nav_block\"]/table/tr/td/table/tr/td");//跟Xpath一样
                string errMsg = htmlResult;
                if (node != null)
                    errMsg = node.InnerText;
                return string.Format("登录失败，失败原因：{0}", errMsg);
            }
        }


        public override string Sign()
        {
            return "我也想签到，可是站点不支持。";
        }


        protected override bool SetTorrentSubTitle(HtmlNode node, PTTorrent torrent)
        {
            var subNode = node.SelectSingleNode(".//td[2]/br");
            if (subNode != null && subNode.NextSibling != null)
                torrent.Subtitle = HttpUtility.HtmlDecode(subNode.NextSibling.InnerText);
            return false;
        }
    }
}
