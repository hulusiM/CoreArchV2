using CoreArchV2.Core.Util.Result;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CoreArchV2.Dto.EApiDto
{
    public class AResponseDto : IActionResult
    {
        public bool Success { get; set; } = false;
        public HttpStatusCode StatusCode { get; set; }
        public dynamic Data { get; set; }

        public AResponseDto(bool success, HttpStatusCode statusCode, dynamic data)
        {
            this.Success = success;
            this.StatusCode = statusCode;
            this.Data = data;
        }

        public async Task ExecuteResultAsync(ActionContext context)
        {
            var objectResult = new ObjectResult(new ResultWrapper { Success = this.Success, StatusCode = this.StatusCode, Data = this.Data })
            {
                StatusCode = (int)StatusCode
            };

            await objectResult.ExecuteResultAsync(context);
        }
    }
}
