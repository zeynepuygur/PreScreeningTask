using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PreScreening.Models;
using PreScreening.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PreScreening.Controllers
{
    public class NessusController
    {
        string token = null;
        private readonly NessusServices _nessusService;

        public NessusController(NessusServices nessusService)
        {
            _nessusService = nessusService;
        }

        public async Task<string> LoginAsync(LoginModel request)
        {
            JObject param = new JObject(
                new JProperty("username", request.UserName),
                new JProperty("password", request.Password)
                );
            var json = await _nessusService.ConnectNessusAsync("POST", "/session", param);

            this.token = (string)json["token"];
            return token;
        }

        public async Task<Dictionary<string, int>> GetScanAsync(int scan_id)
        {
            string resource = string.Format("/scans/{0}", scan_id);
            var response = await _nessusService.ConnectNessusAsync("GET", resource);
            var data = response.SelectToken("history").ToDictionary(_ => (string)_["uuid"], _ => (int)_["history_id"]);
            return data;
        }

        public async Task<int> CreateScanAsync(ScanModel request)
        {
            JObject param = new JObject(
                new JProperty("uuid", request.PolicyId),
                new JProperty("settings", new JObject(
                    new JProperty("name", request.Name),
                    new JProperty("description", request.Description),
                    new JProperty("text_targets", request.Targets))));

            var response = await _nessusService.ConnectNessusAsync("POST", "/session", param);
            return (int)response["scan"]["id"];
        }

        public async Task<int> ExportScanAsync(int scan_id,  string format)
        {
            JObject param = new JObject(
                new JProperty("format", format),
                new JProperty("chapters", "vuln_hosts_summary")
                );

            var response = await _nessusService.ConnectNessusAsync("POST", string.Format("/scans/{0}/export", scan_id), param);
            var file = (int)response["file"];

            while (true)
            {
                var statusresponse = await _nessusService.ConnectNessusAsync("GET", string.Format("/scans/{0}/export/{1}/status", scan_id, file));
                if ((string)statusresponse["status"] != "loading") break;
                await Task.Delay(1000);
            }

            return file;
        }

        public async Task<Dictionary<string, string>> GetPolicyTemplatesAsync()
        {
            var response = await _nessusService.ConnectNessusAsync("GET", "/editor/policy/templates");
            var data = response.SelectToken("templates").ToDictionary(_ => (string)_["title"], _ => (string)_["uuid"]);
            return data;
        }
    }
}
