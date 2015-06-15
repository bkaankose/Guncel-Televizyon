using GTVWin8.DataModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace GTVWin8
{
    public class GTVService
    {
        public async Task<ServiceResponse> getFromAPI(string prefix)
        {
            var nRet = new ServiceResponse();
            try
            {
                using (var htc = new HttpClient())
                {
                    nRet.StatusValid = true;
                    nRet.StatusMessage = await htc.GetStringAsync(App.APIUrl + prefix);
                }
            }
            catch(Exception ex)
            {
                nRet.StatusMessage = ex.Message;
                nRet.StatusValid = false;
            }
            return nRet;
        }
        public async Task<bool> postToAPI(string prefix,object postParam)
        {
            var jSon = await Task.Factory.StartNew(() => JsonConvert.SerializeObject(postParam));
            using (var htc = new HttpClient())
            {
                var res = await htc.PostAsync(App.APIUrl + prefix, new StringContent(jSon, Encoding.UTF8, "application/json"));
                if(res.IsSuccessStatusCode)
                    return res.IsSuccessStatusCode;
                else
                {
                    await GTVDialogs.ShowErrorAsync(res.StatusCode.ToString());
                    return false;
                }

            }
        }
    }
}
