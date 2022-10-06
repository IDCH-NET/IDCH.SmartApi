using Microsoft.AspNetCore.Mvc;
using IDCH.SmartApi.Helpers;
using Models.Yolo;
using Microsoft.AspNetCore.Http.Extensions;
using Models;

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
        public async Task<IActionResult> DetectObjectAsync(List<IFormFile> files)
        {
            long size = files.Sum(f => f.Length);
            OutputCls res = new OutputCls() { IsSucceed = false };
            
            foreach (var formFile in files)
            {
                if (formFile.Length > 0)
                {
                    
                    using (var stream = new MemoryStream())
                    {
                        await formFile.CopyToAsync(stream);


                        var bytes = stream.ToArray();
                        res = await yolo.Predict(bytes);

                        var uri = new Uri( Request.GetDisplayUrl());

                        var baseUrl = uri.GetLeftPart(System.UriPartial.Authority);
                        if (res.IsSucceed)
                        {
                            for (var i = 0; i < res.FileUrls.Count; i++)
                            {
                                res.FileUrls[i] = $"{baseUrl}{AppConstants.UploadUrlPrefix}{res.FileUrls[i]}";
                            }
                        }
                        return Ok(res);
                    }
                }
            }

            // Process uploaded files
            // Don't rely on or trust the FileName property without validation.

            return Ok(res);
        }
    }
}
