using log4net;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AdministrationTool.Helper
{
    public static class CommonHelper
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static string _machineId;
        public static string GetMachineId()
        {

            if (!string.IsNullOrEmpty(_machineId))
                return _machineId;

            var views = new[] { RegistryView.Registry64, RegistryView.Registry32 };
            foreach (var registryView in views)
            {
                const string keyString = @"SOFTWARE\Microsoft\Cryptography";
                const string valueString = @"MachineGuid";
                {
                    using (var keyBase = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, registryView))
                    using (var key = keyBase.OpenSubKey(keyString, RegistryKeyPermissionCheck.ReadSubTree))
                    {
                        if (key == null)
                            continue;
                        var resultObj = key.GetValue(valueString, "default");
                        if (resultObj == null || resultObj.ToString() == "default")
                            continue;
                        // var codeBase = Assembly.GetExecutingAssembly().CodeBase;
                        var pathHash = HashHelper.Hash(resultObj.ToString(), "g*|*n78698C7=n*5_3--26!F", 10).Value;
                        var result = pathHash.Length > 50 ? pathHash.Substring(0, 50) : pathHash;
                        Log.Info("Creating machine id");
                        Log.InfoFormat("Using MachineGuid: {0}", resultObj);
                        Log.InfoFormat("Hash(Codebase + MAchineGuid) {0}", pathHash);
                        Log.InfoFormat("Result {0}", result);
                        _machineId = result;
                        return _machineId;
                    }
                }
            }
            var guid = Guid.NewGuid();
            _machineId = guid.ToString();
            return _machineId;
        }
    }
}
