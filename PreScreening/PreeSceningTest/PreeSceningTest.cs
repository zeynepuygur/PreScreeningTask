using Newtonsoft.Json.Linq;
using PreScreening.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PreeSceningTest
{
    public class PreeSceningTest
    {
        private  NessusServices _nessusService=new NessusServices();

        public PreeSceningTest()
        {
        }

        public async void TestMain()
        {
            // Ignore SSL validation
            System.Net.ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            System.Net.ServicePointManager.SecurityProtocol |= System.Net.SecurityProtocolType.Tls12;

            string scanname = "NessusTest_" + DateTime.Now.ToString("yyyyMMddHHmmss");

            var token = await LoginAsync("admin", "admin");

            var policies = await GetPolicyTemplatesAsync();
            var basic_network_scan_uuid = policies["Basic Network Scan"];

            var scan_id = await CreateScanAsync(scanname, basic_network_scan_uuid, "127.0.0.1");
            var scan_uuid = await LaunchScanAsync(scan_id);

            var fidhtml = await ExportScanAsync(scan_id, "html");
        }

        public async Task<string> LoginAsync(string username, string password)
        {
            JObject param = new JObject(
                new JProperty("username", username),
                new JProperty("password", password)
                );
            var json = await _nessusService.ConnectNessusAsync("POST", "/session", param);

            return (string)json["token"];
        }

        public async Task<Dictionary<string, string>> GetPolicyTemplatesAsync()
        {
            var response = await _nessusService.ConnectNessusAsync("GET", "/editor/policy/templates");
            var data = response.SelectToken("templates").ToDictionary(_ => (string)_["title"], _ => (string)_["uuid"]);
            return data;
        }

        public async Task<int> CreateScanAsync(string name, string policyId, string targets, string description = "")
        {
            JObject param = new JObject(
                new JProperty("uuid", policyId),
                new JProperty("settings", new JObject(
                    new JProperty("name", name),
                    new JProperty("description", description),
                    new JProperty("text_targets", targets))));

            var response = await _nessusService.ConnectNessusAsync("POST", "/session", param);
            return (int)response["scan"]["id"];
        }

        public async Task<string> LaunchScanAsync(int scan_id)
        {
            string resource = string.Format("/scans/{0}/launch", scan_id);
            var response = await _nessusService.ConnectNessusAsync("POST", resource);
            return (string)response["scan_uuid"];
        }

        public async Task<Dictionary<string, int>> GetScanAsync(int scan_id)
        {
            //var client = new RestClient("https://cloud.tenable.com/scans");
            //var request = new RestRequest(Method.Get);
            //request.AddHeader("Accept", "application/json");
            //request.AddHeader("X-ApiKeys", "2b77212ffcde6d58f795154edd9cbbe7af690fec4d4ff9a09ee3ccd8b0d63d20");
            //IRestResponse response = client.ExecuteAsync(request);

            string resource = string.Format("/scans/{0}", scan_id);
            var response = await _nessusService.ConnectNessusAsync("GET", resource);
            var data = response.SelectToken("history").ToDictionary(_ => (string)_["uuid"], _ => (int)_["history_id"]);
            return data;
        }

        public async Task<int> ExportScanAsync(int scan_id, string format)
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
    }
}
