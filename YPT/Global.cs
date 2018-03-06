﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using YPT.PT;
using YU.Core;
using YU.Core.DataEntity;
using YU.Core.Log;
using YU.Core.Utils;

namespace YPT
{
    /// <summary>
    /// 全局静态类
    /// </summary>
    public static class Global
    {
        /// <summary>
        /// 全局配置
        /// </summary>
        public static Config Config { get; set; }

        public static List<PTSite> Sites { get { return _sites; } }

        private static List<PTSite> _sites;

        public static List<PTUser> Users { get; set; }


        public static void Init()
        {
            InitSites();
            Config = new Config();
            string dbName = ConfigUtil.GetConfigValue(YUConst.SETTING_SQLLITEDB);
            string dbPassWord = ConfigUtil.GetConfigValue(YUConst.SETTING_SQLLITEDBPASSWORD);
            if (dbName.IsNullOrEmptyOrWhiteSpace())
                throw new Exception("配置数据库名称不能为空。");
            else
            {
                DBUtils.SetConnectionString(dbName, dbPassWord);
                if (!File.Exists(ConfigUtil.GetConfigValue(YUConst.SETTING_SQLLITEDB)))
                {
                    InitDB(dbName);
                    Config.SignTime = new DateTime(1970, 1, 1, 0, 0, 0);
                    Config.IsAutoSign = true;
                    Users = new List<PTUser>();
                    Config.IsEnablePostFileName = true;
                    Config.IsSyncTiming = true;
                    Config.IsFirstOpen = true;
                    Config.IsMiniWhenClose = false;
                }
                else
                {
                    Config.SignTime = GetConfig(YUConst.CONFIG_SIGN_TIME, new DateTime(1970, 1, 1, 0, 0, 0));
                    Config.IsAutoSign = GetConfig(YUConst.CONFIG_SIGN_AUTO, true);
                    Config.IsEnablePostFileName = GetConfig(YUConst.CONFIG_ENABLEPOSTFILENAME, true);
                    Config.IsSyncTiming = GetConfig(YUConst.CONFIG_SYNC_AUTO, true);
                    Config.IsFirstOpen = GetConfig(YUConst.CONFIG_ISFIRSTOPEN, true);
                    Config.IsMiniWhenClose = GetConfig(YUConst.CONFIG_ISMINIWHENCLOSE, false);
                    InitUser();
                }
            }
        }

        public static void InitSites()
        {
            _sites = ObjectUtils.CreateCopy<List<PTSite>>(PTSite.Sites);
            if (File.Exists(PTSiteConst.RESOURCE_SITES))
            {
                string siteJson = File.ReadAllText(PTSiteConst.RESOURCE_SITES);                
                try
                {
                    var extendSites = JsonConvert.DeserializeObject<List<PTSite>>(siteJson);
                    if (extendSites != null && extendSites.Count > 0)
                    {
                        //如果扩展站点中是预置站点中的内容，则删除预置站点，以扩展站点为准。
                        _sites.RemoveAll(x => extendSites.Select(g => g.Id).Contains(x.Id));
                        foreach (var extendSite in extendSites)
                        {
                            _sites.Add(extendSite);
                        }
                    }
                }
                catch
                {
                    Logger.Info(string.Format("序列化扩展站点失败 ，请检查[{0}]文件。", PTSiteConst.RESOURCE_SITES));
                }
            }
            else
            {
                Logger.Info(string.Format("站点配置文件不存在，请检查[{0}]文件。", PTSiteConst.RESOURCE_SITES));
            }
        }


        /// <summary>
        /// 初始化用户数据
        /// </summary>
        public static void InitUser()
        {
            Users = new List<PTUser>();

            IDataReader dr = DBUtils.ExecuteReader("SELECT * FROM USER");
            while (dr.Read())
            {
                int siteId = dr["PTSITEID"].TryPareValue(0);
                PTUser user = new PTUser();
                user.Id = dr["USERID"].TryPareValue<int>();
                user.UserName = dr["USERNAME"].TryPareValue<string>();
                user.PassWord = dr["PASSWORD"].TryPareValue<string>();
                user.isEnableTwo_StepVerification = dr["ISENABLETWO_STEPVERIFICATION"].TryPareValue(false);
                user.SecuityAnswer = dr["SECUITYANSWER"].TryPareValue(string.Empty);
                user.SecurityQuestionOrder = dr["SECURITYQUESTIONORDER"].TryPareValue(-1);
                user.Site = Sites.Where(x => (int)x.Id == siteId).FirstOrDefault();
                //如果找不到相应站点，这里就直接跳过该用户了。
                if (user.Site == null)
                    continue;
                Users.Add(user);
            }
        }


        /// <summary>
        /// 初始化数据库
        /// </summary>
        /// <param name="dbName"></param>
        public static void InitDB(string dbName)
        {
            DBUtils.CreateDB(dbName);

            List<KeyValuePair<string, SQLiteParameter[]>> sqlList = new List<KeyValuePair<string, SQLiteParameter[]>>();

            //Config表
            string sql = "CREATE TABLE CONFIG (FID VARCAHR(16),VALUE VARCHAR(2000))";
            sqlList.Add(new KeyValuePair<string, SQLiteParameter[]>(sql, null));

            //User表
            sql = "CREATE TABLE USER (PTSITEID INT,USERID INT,USERNAME VARCAHR(100),PASSWORD VARCHAR(100),SECURITYQUESTIONORDER INT,SECUITYANSWER VARCHAR(200),ISENABLETWO_STEPVERIFICATION INT)";
            sqlList.Add(new KeyValuePair<string, SQLiteParameter[]>(sql, null));

            DBUtils.ExecuteNonQueryBatch(sqlList);
        }

        /// <summary>
        /// 获取配置
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="configKey"></param>
        /// <returns></returns>
        public static T GetConfig<T>(string configKey, T defaultValue = default(T))
        {
            string selectSql = " SELECT VALUE FROM CONFIG WHERE FID = @FID ";
            SQLiteParameter parm = new SQLiteParameter("@FID", DbType.String);
            parm.Value = configKey;
            string dbValue = DBUtils.ExecuteScalar(selectSql, "", parm);
            if (dbValue.IsNullOrEmptyOrWhiteSpace())
                return defaultValue;
            else
                return JsonConvert.DeserializeObject<T>(dbValue);
        }

        /// <summary>
        /// 设置配置
        /// </summary>
        /// <param name="configKey"></param>
        /// <param name="configValue"></param>
        public static void SetConfig(string configKey, object configValue)
        {
            string selectSql = " SELECT 1 FROM CONFIG WHERE FID = @FID ";
            SQLiteParameter parm = new SQLiteParameter("@FID", DbType.String);
            parm.Value = configKey;
            string sql = string.Empty;

            if (DBUtils.ExecuteScalar<int>(selectSql, -1, parm) == -1)
                sql = @" INSERT INTO CONFIG(FID,VALUE) VALUES(@FID,@VALUE) ";
            else
                sql = @" UPDATE CONFIG SET VALUE = @VALUE WHERE FID = @FID ";

            SQLiteParameter[] parms = new SQLiteParameter[]
                              {
                        new SQLiteParameter("@FID", DbType.String),
                        new SQLiteParameter("@VALUE", DbType.String)
                              };
            parms[0].Value = configKey;
            parms[1].Value = JsonConvert.SerializeObject(configValue);

            DBUtils.ExecuteNonQuery(sql, parms);
        }
    }
}
