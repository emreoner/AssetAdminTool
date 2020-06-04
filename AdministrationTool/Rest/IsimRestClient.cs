using AdministrationTool.Helper;
using iSIM.Core.Common.Constant;
using iSIM.Core.Common.Model;
using iSIM.Core.Common.Security;
using iSIM.Core.Entity.Dto;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AdministrationTool.Rest
{
    public class IsimRestClient
    {
        /// <summary>
        /// The log
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The lock
        /// </summary>
        private static readonly object _lock = new Object();

        /// <summary>
        /// The instance
        /// </summary>
        private static IsimRestClient _instance;

        /// <summary>
        /// Timespan for rest client timeout
        /// </summary>
        private static TimeSpan _restTimeoutTimeSpan = TimeSpan.FromMinutes(2);

        /// <summary>
        /// The client
        /// </summary>
        private static HttpClient _client = new HttpClient();

        /// <summary>
        /// Rest url address of iSIM server
        /// </summary>
        private static string _isimRestUrlAddress;


        static private Thread _pollSession = null;

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static IsimRestClient Instance
        {
            get
            {
                if (_instance != null) return _instance;
                lock (_lock)
                {
                    {
                        _instance = new IsimRestClient();
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="IsimRestClient"/> class from being created.
        /// </summary>
        private IsimRestClient()
        {

        }

        /// <summary>
        /// Set required information for initializing isim rest client.
        /// </summary>
        /// <param name="isim_rest_url">Url address of isim server rest service</param>
        public bool SetConnectionInfo(string isim_rest_url)
        {
            try
            {
                _isimRestUrlAddress = isim_rest_url;
                _client.BaseAddress = new Uri(_isimRestUrlAddress);
                _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                _client.Timeout = _restTimeoutTimeSpan;
            }
            catch (Exception ex)
            {
                Log.Error("Error occured while creating HttpClient. Check url addresses in the App.config file!", ex);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the unified asset list
        /// </summary>
        /// <returns></returns>
        /// 
        public async Task<List<UnifiedAssetDto>> GetAssetListByUserIdAsync(long userId)
        {
            List<UnifiedAssetDto> assets = new List<UnifiedAssetDto>();
            try
            {
               
                string uri = string.Format("{0}{1}?token={2}", "Asset/Unified/User/",userId, IsimConstant.IsimInternalCommunicationTokenId);
                var response = await _client.GetAsync(WebUtility.UrlDecode(uri)).Result.Content.ReadAsStringAsync();
                IsimResult<List<UnifiedAssetDto>> requestAssetResult = JsonConvert.DeserializeObject<IsimResult<List<UnifiedAssetDto>>>(response);
                if (requestAssetResult.IsSuccess)
                {
                    assets.AddRange(requestAssetResult.Data);
                }
            }
            catch (Exception ex)
            {
                
            }

            return assets;
        }

        //public List<UnifiedAssetDto> GetAssetList()
        //{
        //    List<UnifiedAssetDto> assets = new List<UnifiedAssetDto>();
        //    string uri = string.Format("{0}?token={1}", "Asset/Unified", IsimConstant.IsimInternalCommunicationTokenId);
        //    var response =  _client.GetAsync(WebUtility.UrlDecode(uri)).Result.Content.ReadAsStringAsync();
        //    IsimResult<List<UnifiedAssetDto>> requestAssetResult = JsonConvert.DeserializeObject<IsimResult<List<UnifiedAssetDto>>>(response);
        //    if (requestAssetResult.IsSuccess)
        //    {
        //        assets.AddRange(requestAssetResult.Data);
        //    }
        //    return assets;
        //}

        public async Task<IsimResult<SessionUser>> Login(string username, string password)
        {
            SessionUserBase user = new SessionUserBase()
            {
                Client = new SessionClientBase
                {
                    PcName = AuthorizationHelper.Client.PcName,
                    Id = AuthorizationHelper.Client.Id,
                    Uuid = AuthorizationHelper.Client.Uuid
                },
                Password = IsimCrypto.Encrypt(password),
                Username = username,
            };

            string uri = WebUtility.UrlDecode(string.Format("Session?token={0}", IsimConstant.IsimCommunicationFakeTokenId));
            var content = new StringContent(user.ToJson(), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _client.PostAsync(uri, content);
            var res = response.Content.ReadAsStringAsync().Result;
            var isimRes = JsonConvert.DeserializeObject<IsimResult<SessionUser>>(res);

            if (!isimRes.IsSuccess) return isimRes;

            //if (_pollSession == null)
            //{
            //    _pollSession = new Thread(() => PollSession(isimRes.Data.Id));
            //    _pollSession.Start();
            //}

            return isimRes;
        }

        //static private void PollSession(string sessionId)
        //{
        //    var route = string.Format("Session?SessionId={0}", sessionId);
        //    var timeoutConfig = "1500";
        //    var sessionTimeout = timeoutConfig != null ? Int32.Parse(timeoutConfig) : 300;

        //    for (; ; )
        //    {
              
        //        var content = new StringContent(user.ToJson(), Encoding.UTF8, "application/json");
        //        HttpResponseMessage response = await _client.PostAsync(uri, content);
        //        //var timeout = 
        //        _client
        //        .SetRoute(route)
        //        .Execute<SessionUser>(HttpMethod.Put);
        //        Thread.Sleep(sessionTimeout * 1000);
        //    }
        //}

        /// <summary>
        /// Gets the asset by id
        /// </summary>
        /// <returns></returns>
        public async Task<ClientDto> GetClientByKeyAsync(string id)
        {
            ClientDto clientDto;
            try
            {
                string uri = string.Format("{0}{1}?token={2}", "Client/IsExist/", id, IsimConstant.IsimCommunicationFakeTokenId);
                var response = await _client.GetAsync(WebUtility.UrlDecode(uri)).Result.Content.ReadAsStringAsync();
                clientDto = JsonConvert.DeserializeObject<IsimResult<ClientDto>>(response).Data;
            }
            catch (Exception ex)
            {
                clientDto = null;
            }
            return clientDto;

        }



        /// <summary>
        /// Gets the asset by id
        /// </summary>
        /// <returns></returns>
        public async Task<AssetDto> GetAssetById(long id)
        {
            string uri = string.Format("{0}{1}?token={2}", "Asset/", id, IsimConstant.IsimInternalCommunicationTokenId);
            var response = await _client.GetAsync(WebUtility.UrlDecode(uri)).Result.Content.ReadAsStringAsync();
            IsimResult<List<AssetDto>> requestAssetResult = JsonConvert.DeserializeObject<IsimResult<List<AssetDto>>>(response);
            if (requestAssetResult.IsSuccess && requestAssetResult.Data != null)
            {
                return requestAssetResult.Data.FirstOrDefault();
            }
            return null;
        }

        /// <summary>
        /// Returns the value of specified configuration key from database.
        /// </summary>
        /// <param name="key">configuration key</param>
        /// <returns></returns>
        public string GetConfiguration(string key)
        {
            try
            {
                string uri = string.Format("{0}{1}?token={2}", "api/Configuration/Key/", key, IsimConstant.IsimCommunicationFakeTokenId);
                var response = _client.GetAsync(WebUtility.UrlDecode(uri));
                var res = response.Result.Content.ReadAsStringAsync().Result;
                IsimResult<List<ConfigurationDto>> requestConfigurationKeyResult = JsonConvert.DeserializeObject<IsimResult<List<ConfigurationDto>>>(res);
                if (requestConfigurationKeyResult != null && requestConfigurationKeyResult.IsSuccess)
                {
                    if (requestConfigurationKeyResult.Data == null)
                    {
                        Log.Info($"Configuration could not be found! Key: {key}");
                        return "";
                    }

                    return requestConfigurationKeyResult.Data.FirstOrDefault()?.Value;
                }
                else
                {
                    Log.Info($"Request configuration result is not successfull! Key: {key}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"An error occured in GetConfiguration()! Key: {key}", ex);
                return null;
            }
        }

        /// <summary>
        /// Returns the decrypted value of specified configuration key from database.
        /// </summary>
        /// <param name="key">configuration key</param>
        /// <returns></returns>
        public string GetEncryptedConfiguration(string key)
        {
            try
            {
                string uri = string.Format("{0}{1}?token={2}", "api/Configuration/Key/", key, IsimConstant.IsimCommunicationFakeTokenId);
                var response = _client.GetAsync(WebUtility.UrlDecode(uri));
                var res = response.Result.Content.ReadAsStringAsync().Result;
                IsimResult<List<ConfigurationDto>> requestConfigurationKeyResult = JsonConvert.DeserializeObject<IsimResult<List<ConfigurationDto>>>(res);
                if (requestConfigurationKeyResult != null && requestConfigurationKeyResult.IsSuccess)
                {
                    if (requestConfigurationKeyResult.Data == null)
                    {
                        Log.Info($"Configuration could not be found! Key: {key}");
                        return "";
                    }

                    return IsimCrypto.Decrypt(requestConfigurationKeyResult.Data.FirstOrDefault()?.Value);
                }
                else
                {
                    Log.Info($"Request configuration result is not successfull! Key: {key}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"An error occured in GetConfiguration()! Key: {key}", ex);
                return null;
            }
        }

        /// <summary>
        /// Gets the milestone vms.
        /// </summary>
        /// <returns></returns>
        public async Task<List<VmsDto>> GetMilestoneVmses()
        {
            List<VmsDto> vms = new List<VmsDto>();
            string uri = string.Format("{0}?token={1}", "Vms", IsimConstant.IsimInternalCommunicationTokenId);
            var response = await _client.GetAsync(WebUtility.UrlDecode(uri)).Result.Content.ReadAsStringAsync();
            IsimResult<List<VmsDto>> allVms = JsonConvert.DeserializeObject<IsimResult<List<VmsDto>>>(response);
            if (allVms.IsSuccess)
            {
                vms.AddRange(allVms.Data/*.Where(l => l.BrandDto.Brand == Brand.Milestone)*/);
            }
            return vms;
        }

        /// <summary>
        /// Gets the identifier by UUID.
        /// </summary>
        /// <param name="uuid">The UUID.</param>
        /// <returns></returns>
        public long GetIdByUuid(string uuid)
        {
            try
            {
                string uri = string.Format("{0}{1}?token={2}", "Camera/Uuid/", uuid, IsimConstant.IsimCommunicationFakeTokenId);
                var response = _client.GetAsync(WebUtility.UrlDecode(uri)).Result.Content.ReadAsStringAsync().Result;
                IsimResult<long> cameraId = JsonConvert.DeserializeObject<IsimResult<long>>(response);
                return cameraId.Data;
            }
            catch (Exception ex)
            {
                Log.Error("An error occured on GetIdByUuid()!", ex);
                return 0;
            }
        }

        /// <summary>
        /// Posts the asynchronous.
        /// </summary>
        /// <param name="dto">The dto.</param>
        public async Task<bool> PostAsync(EventDto dto)
        {
            try
            {
                string uri = WebUtility.UrlDecode(string.Format("Event?token={0}", IsimConstant.IsimCommunicationFakeTokenId));
                var content = new StringContent(dto.ToJson(), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await _client.PostAsync(uri, content);
                var res = response.Content.ReadAsStringAsync().Result;
                IsimResult isimRes = JsonConvert.DeserializeObject<IsimResult>(res);
                if (!response.IsSuccessStatusCode)
                {
                    Log.Error($"Event data could not be posted to isim server! CHKSUM: {dto.Checksum}");
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Error("An error occured on post to Isim server!", ex);
                return false;
            }
        }
    }
}
