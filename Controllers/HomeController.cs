using Microsoft.AspNetCore.Mvc;

using System.Xml;
using System.Xml.Linq;

using WebApplication1.Helper.Imager;
using WebApplication1.Helper.PDF;

namespace WebApplication1.Controllers;

[ApiController]
[Route("[Controller]/[Action]")]
public class HomeController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> PDFToPng(IFormFile formFile, string? format)
    {
        byte[][] bytess = formFile.OpenReadStream().PDFToPng();
        string suffix = "png";
        byte[]? bytes;
        if (bytess.Length == 1)
        {
            bytes = bytess[0];
        }
        else if (format?.Equals("gif", StringComparison.CurrentCultureIgnoreCase) == true)
        {
            suffix = "gif";
            bytes = bytess.FrameCombine(1);
        }
        else if (format?.Equals("zip", StringComparison.CurrentCultureIgnoreCase) == true)
        {
            suffix = "zip";
            bytes = bytess.CompressToZip(100);
        }
        else
        {
            bytes = await bytess.MergeImages();
        }
        return new FileContentResult(bytes, $"image/{suffix}")
        {
            FileDownloadName = $"test-image.{suffix}"
        };
        //return File(bytes, "image/png",true);
    }

}
