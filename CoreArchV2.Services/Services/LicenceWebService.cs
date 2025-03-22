using CoreArchV2.Dto.ALicenceApiDto;
using CoreArchV2.Dto.EApiDto;
using CoreArchV2.Services.Interfaces;
using CoreArchV2.Utilies;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;
using System.Text;

namespace CoreArchV2.Services.Services
{
    public class LicenceWebService : ILicenceWebService
    {
        private readonly string baseUrl = "https://api.basaranerp.com/";
        //private readonly string baseUrl = "https://localhost:44332/";
        private readonly LicenceSetting _licenceSetting;
        public LicenceWebService(IOptions<LicenceSetting> licenceSetting)
        {
            _licenceSetting = licenceSetting.Value;
        }

        public async Task<AResponseDto> GetToken()
        {
            var respData = new AResponseDto(success: false, statusCode: HttpStatusCode.BadRequest, data: null);
            using (var httpClientHandler = new HttpClientHandler())
            {
                httpClientHandler.AllowAutoRedirect = true;
                httpClientHandler.UseProxy = true;
                httpClientHandler.MaxConnectionsPerServer = 5;
                using (var client = new HttpClient(httpClientHandler))
                {
                    var url = baseUrl + "Auth/GetToken";
                    var parameters = "?username=basaranerp.mobile&password=AsDfgd82dfg5214654df6d1fg3fDD42vbnh";
                    var response = await client.GetAsync(url + parameters);

                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        respData = JsonConvert.DeserializeObject<AResponseDto>(content);
                    }
                }
            }

            return respData;
        }
        public async Task<AResponseDto> Check()
        {
            var respData = new AResponseDto(success: false, statusCode: HttpStatusCode.BadRequest, data: null);
            try
            {
                var token = await GetToken();
                using (var httpClientHandler = new HttpClientHandler())
                {
                    httpClientHandler.AllowAutoRedirect = true;
                    httpClientHandler.UseProxy = true;
                    httpClientHandler.MaxConnectionsPerServer = 5;

                    using (var client = new HttpClient(httpClientHandler))
                    {
                        var request = new ALicenceRequestDto()
                        {
                            FirmKey = _licenceSetting.Key,
                            FirmName = _licenceSetting.Name,
                            Ip = _licenceSetting.Ip,
                        };

                        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token.Data}");
                        var url = baseUrl + "Licence/Check";
                        var model = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                        var response = await client.PostAsync(url, model);

                        if (response.IsSuccessStatusCode)
                        {
                            var content = await response.Content.ReadAsStringAsync();
                            respData = JsonConvert.DeserializeObject<AResponseDto>(content);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                respData = new AResponseDto(success: false, statusCode: HttpStatusCode.BadRequest, data: "Lisans kontrolü yaparken hata oluştu. (Web->Api)");
            }

            return respData;
        }
        public async Task<AResponseDto> AddUserRole(int createdBy, string model)
        {
            var respData = new AResponseDto(success: false, statusCode: HttpStatusCode.BadRequest, data: null);
            try
            {
                var token = await GetToken();
                if (!string.IsNullOrEmpty(token.Data))
                {
                    var request = new ALicenceRequestDto()
                    {
                        FirmKey = _licenceSetting.Key,
                        FirmName = _licenceSetting.Name,
                        Ip = _licenceSetting.Ip,
                        CreatedBy = createdBy,
                        Model = model,
                    };

                    var data = new System.Net.Http.StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                    using var client = new HttpClient();
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token.Data);
                    var url = baseUrl + "Licence/AddUserRole";
                    var response = await client.PostAsync(url, data);
                    respData = JsonConvert.DeserializeObject<AResponseDto>(await response.Content.ReadAsStringAsync());
                }
            }
            catch (Exception)
            {
                respData = new AResponseDto(success: false, statusCode: HttpStatusCode.BadRequest, data: "Lisans kontrolü yaparken hata oluştu. (Web->Api)");
            }

            return respData;
        }
        public async Task<AResponseDto> DeleteUserRole(int createdBy, string model)
        {
            var respData = new AResponseDto(success: false, statusCode: HttpStatusCode.BadRequest, data: null);
            try
            {
                var token = await GetToken();
                if (!string.IsNullOrEmpty(token.Data))
                {
                    var request = new ALicenceRequestDto()
                    {
                        FirmKey = _licenceSetting.Key,
                        FirmName = _licenceSetting.Name,
                        Ip = _licenceSetting.Ip,
                        CreatedBy = createdBy,
                        Model = model,
                    };

                    var data = new System.Net.Http.StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                    using var client = new HttpClient();
                    //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token.Data);
                    var url = baseUrl + "Licence/DeleteUserRole";
                    var response = await client.PostAsync(url, data);
                    respData = JsonConvert.DeserializeObject<AResponseDto>(await response.Content.ReadAsStringAsync());
                }
            }
            catch (Exception)
            {
                respData = new AResponseDto(success: false, statusCode: HttpStatusCode.BadRequest, data: "Lisans kontrolü yaparken hata oluştu. (Web->Api)");
            }

            return respData;
        }
        public async Task<AResponseDto> AddVehicle(int createdBy, string model)
        {
            var respData = new AResponseDto(success: false, statusCode: HttpStatusCode.BadRequest, data: null);
            try
            {
                var token = await GetToken();
                if (!string.IsNullOrEmpty(token.Data))
                {
                    var request = new ALicenceRequestDto()
                    {
                        FirmKey = _licenceSetting.Key,
                        FirmName = _licenceSetting.Name,
                        Ip = _licenceSetting.Ip,
                        CreatedBy = createdBy,
                        Model = model,
                    };

                    var data = new System.Net.Http.StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                    using var client = new HttpClient();
                    //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token.Data);
                    var url = baseUrl + "Licence/AddVehicle";
                    var response = await client.PostAsync(url, data);
                    respData = JsonConvert.DeserializeObject<AResponseDto>(await response.Content.ReadAsStringAsync());
                }
            }
            catch (Exception)
            {
                respData = new AResponseDto(success: false, statusCode: HttpStatusCode.BadRequest, data: "Lisans kontrolü yaparken hata oluştu. (Web->Api)");
            }

            return respData;
        }
        public async Task<AResponseDto> DeleteVehicle(int createdBy, string model)
        {
            var respData = new AResponseDto(success: false, statusCode: HttpStatusCode.BadRequest, data: null);
            try
            {
                var token = await GetToken();
                if (!string.IsNullOrEmpty(token.Data))
                {
                    var request = new ALicenceRequestDto()
                    {
                        FirmKey = _licenceSetting.Key,
                        FirmName = _licenceSetting.Name,
                        Ip = _licenceSetting.Ip,
                        CreatedBy = createdBy,
                        Model = model,
                    };

                    var data = new System.Net.Http.StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                    using var client = new HttpClient();
                    //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token.Data);
                    var url = baseUrl + "Licence/DeleteVehicle";
                    var response = await client.PostAsync(url, data);
                    respData = JsonConvert.DeserializeObject<AResponseDto>(await response.Content.ReadAsStringAsync());
                }
            }
            catch (Exception)
            {
                respData = new AResponseDto(success: false, statusCode: HttpStatusCode.BadRequest, data: "Lisans kontrolü yaparken hata oluştu. (Web->Api)");
            }

            return respData;
        }
    }
}
