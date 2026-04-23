using Microsoft.AspNetCore.Mvc;

namespace UserManagementService.API.Controllers;

[ApiController]
[Route("api/files")]
public class FilesController : ControllerBase
{
    private readonly IWebHostEnvironment _env;

    public FilesController(IWebHostEnvironment env)
    {
        _env = env;
    }

    [HttpGet("{**relativePath}")]
    public IActionResult GetFile(string relativePath)
    {
        var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
        var fullPath = Path.Combine(webRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));

        if (!System.IO.File.Exists(fullPath))
            return NotFound();

        var ext = Path.GetExtension(fullPath).ToLowerInvariant();
        var contentType = ext switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };

        return PhysicalFile(fullPath, contentType);
    }
}
