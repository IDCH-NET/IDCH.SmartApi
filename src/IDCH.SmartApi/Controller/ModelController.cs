using Microsoft.AspNetCore.Mvc;
using IDCH.SmartApi.Helpers;
using Models.Yolo;
using Microsoft.AspNetCore.Http.Extensions;
using Models;
using System.Net;

namespace IDCH.SmartApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ModelController : Controller
    {
        YoloModel yolo;
        public ModelController(YoloModel yolo)
        {
            this.yolo = yolo;
        }
        [HttpPost("[action]")]
        public async Task<IActionResult> DetectObjectAsync(IFormFile file)
        {
            //var files = new List<IFormFile>();
            //files.Add(file);
            //long size = files.Sum(f => f.Length);

            OutputCls res = new OutputCls() { IsSucceed = false };

            //foreach (var formFile in files)
            //{
            if (file.Length > 0)
            {

                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);


                    var bytes = stream.ToArray();
                    res = await yolo.Predict(bytes);

                    var uri = new Uri(Request.GetDisplayUrl());

                    var baseUrl = uri.GetLeftPart(System.UriPartial.Authority);
                    if (res.IsSucceed)
                    {
                        for (var i = 0; i < res.FileUrls.Count; i++)
                        {
                            res.FileUrls[i] = $"{baseUrl}{AppConstants.UploadUrlPrefix}{res.FileUrls[i]}";
                        }
                        res.Message = "processed ok";
                    }
                    return Ok(res);
                }
            }
            else
            {
                res.Message = "no file to be processed";
            }
            //}

            // Process uploaded files
            // Don't rely on or trust the FileName property without validation.

            return Ok(res);
        }
    }
}
