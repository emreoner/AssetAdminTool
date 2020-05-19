using iSIM.Core.Common.Enum;
using iSIM.Core.Common.Model;
using iSIM.Core.Entity.Dto;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AdministrationTool.Helper
{
    public static class AuthorizationHelper
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        

        //private static List<RoleDto> RoleList;


        public static bool IsLoggedIn { get; set; }
        public static ELoginType LoginType { get; set; }
        public static SessionUser User { get; set; }
        public static LayoutDto CurrentLayout { get; set; }
        public static ClientDto Client { get; set; }
        private static DateTime _serverTime;
        private static DateTime _clientTime;
        private static double? _exportTimeLimit = null;
        public static DateTime ServerTime
        {
            get
            {
                return _serverTime + (DateTime.Now - _clientTime);
            }
            set
            {
                _serverTime = value;
                _clientTime = DateTime.Now;
            }
        }

        public static string AMPM()
        {
            if (ServerTime.Hour < 12)
            {
                return "AM";
            }
            else
            {
                return "PM";
            }
        }
        public static bool IsAuthorized(Role role)
        {
            return User.Roles != null && User.Roles.Contains(role);
        }
        //public static bool IsAuthorizedForAsset(Role role, long subjectId)
        //{
        //    var unifiedAssetDto = AssetCache.Instance.GetUnifiedAssetByAssetId(subjectId);

        //    if (unifiedAssetDto == null)
        //        return false;
        //    if (IsPassiveAsset(subjectId))
        //    {
        //        return false;
        //    }

        //    if (unifiedAssetDto.Roles == null)
        //        return false;
        //    return unifiedAssetDto.Roles.Contains(role);
        //}

        //public static bool IsPassiveAsset(long subjectId)
        //{
        //    var unifiedAssetDto = AssetCache.Instance.GetUnifiedAssetByAssetId(subjectId);

        //    if (unifiedAssetDto != null && !unifiedAssetDto.IsActive)
        //    {
        //        return true;
        //    }
        //    else
        //    {
        //        while (unifiedAssetDto.ParentId != 0)
        //        {
        //            unifiedAssetDto = AssetCache.Instance.GetUnifiedAssetByAssetId(unifiedAssetDto.ParentId);

        //            if (unifiedAssetDto != null && !unifiedAssetDto.IsActive)
        //            {
        //                return true;
        //            }
        //        }
        //    }

        //    return false;
        //}
        ////To do
        public static bool IsAuthorized(List<Role> roles, string subjectId = null)
        {
            try
            {
                foreach (Role role in roles)
                {
                    bool a = User.Roles.Contains(role);
                    if (a == false)
                        return false;
                }
            }
            catch (Exception ee)
            {
                Log.Error(ee.Message);
                return false;
            }
            return true;
        }

        public static IPAddress GetLocalIpAddress()
        {
            if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                return null;
            }

            var host = Dns.GetHostEntry(Dns.GetHostName());
            return host.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
        }


        //public static CultureInfo DefaultCulture
        //{
        //    get
        //    {

        //        var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

        //        var Language = config.AppSettings.Settings["DefaultLanguage"];
        //        if (Language != null)
        //        {
        //            if (Language.Value == "miEngland" || Language.Value == "English")
        //                Language.Value = "en-US";
        //            else if (Language.Value == "miTurkey" || Language.Value == "Turkish")
        //                Language.Value = "tr-TR";
        //            else
        //                Language.Value = "ar-QA";
        //            return new CultureInfo(Language.Value);
        //        }
        //        else
        //        {
        //            config.AppSettings.Settings.Add("DefaultLanguage", "English");
        //            config.Save(ConfigurationSaveMode.Modified);
        //            //todo get default lang
        //            return new CultureInfo("en-US");
        //        }
        //        //var defaultLanguage = ConfigurtionHelper.GetKeyValuePair("DefaultLanguage");
        //        //return string.IsNullOrEmpty(defaultLanguage.Value) ? new CultureInfo("en-US") : new CultureInfo(defaultLanguage.Value);
        //    }
        //}

        public static double? ExportTimeLimit
        {
            get
            {
                return _exportTimeLimit;
            }
            set
            {
                _exportTimeLimit = value;
            }
        }

        //internal static bool AssetHasEditRole(long assetId)
        //{
        //    if (AuthorizationHelper.User != null)
        //    {
        //        var assetRoles = IsimServer.Instance.AuthorizationService().GetUserAssetRoles(AuthorizationHelper.User.UserId, assetId);
        //        if (assetRoles != null && assetRoles.Data != null)
        //        {
        //            if (assetRoles.Data.Any(k => k == Role.AssetEdit))
        //            {
        //                return true;
        //            }
        //            else
        //            {
        //                return false;
        //            }
        //        }
        //    }
        //    return false;
        //}
    }

    public enum ELoginType
    {
        AsUser = 0,
        AsClient = 1
    }
}
