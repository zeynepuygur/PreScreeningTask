using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PreScreening.Services
{
    public class NessusServices
    {
        HttpClient client = new HttpClient();

        public async Task<JObject> ConnectNessusAsync(string method, string resource, JObject param = null)
        {
            //nessusip
            Uri uri = new Uri(string.Format("https://localhost:8834{0}", resource));
            
            switch (method.ToUpper())
            {
                default:
                    throw new ArgumentException();
                case "GET":
                    var getjson = await client.GetStringAsync(uri);
                    return (JObject)JsonConvert.DeserializeObject(getjson);
                case "POST":
                    HttpContent postcontent;
                    if (param == null) postcontent = new StringContent("", Encoding.UTF8, "application/json");
                    else postcontent = new StringContent(param.ToString(), Encoding.UTF8, "application/json");
                    var postresponse = await client.PostAsync(uri, postcontent);
                    var postrjson = (JObject)JsonConvert.DeserializeObject(await postresponse.Content.ReadAsStringAsync());
                    return postrjson;
                case "PUT":
                    var putresponse = await client.PostAsync(uri, new StringContent(param.ToString(), Encoding.UTF8, "application/json"));
                    var putjson = (JObject)JsonConvert.DeserializeObject(await putresponse.Content.ReadAsStringAsync());
                    return putjson;
                case "DELETE":
                    var deleteresponse = await client.DeleteAsync(uri);
                    var deletejson = (JObject)JsonConvert.DeserializeObject(await deleteresponse.Content.ReadAsStringAsync());
                    return deletejson;
            }
        }
    }
}
