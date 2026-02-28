using Microsoft.AspNetCore.Mvc;
using REST_API_NeuroC_Prep.Models;
using REST_API_NeuroC_Prep.Services;

namespace REST_API_NeuroC_Prep.Controllers
{
    /// <summary>
    /// Frame-Zugriff: Metadaten, RGB-Rohdaten, Bild-Download.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class FrameController : ControllerBase
    {
        private readonly VisionService _vision;

        public FrameController(VisionService vision) => _vision = vision;

        /// <summary>Frame-Metadaten (Breite, Höhe, Kanäle, Stride).</summary>
        [HttpGet("info")]
        [ProducesResponseType(typeof(FrameInfoDto), 200)]
        [ProducesResponseType(503)]
        public IActionResult GetFrameInfo()
        {
            var info = _vision.GetFrameInfo();
            return info != null
                ? Ok(info)
                : StatusCode(503, new MessageDto("Kamera nicht aktiv oder kein Frame verfügbar"));
        }

        /// <summary>Aktueller Frame als Base64-kodierte RGB-Daten.</summary>
        [HttpGet("rgb")]
        [ProducesResponseType(typeof(FrameBase64Dto), 200)]
        [ProducesResponseType(503)]
        public IActionResult GetFrameRgb()
        {
            var frame = _vision.GetFrameRgb();
            return frame != null
                ? Ok(frame)
                : StatusCode(503, new MessageDto("Kein Frame verfügbar"));
        }

        /// <summary>Aktueller Frame als BMP-Bilddatei (Download).</summary>
        [HttpGet("image")]
        [Produces("image/bmp")]
        [ProducesResponseType(typeof(FileContentResult), 200)]
        [ProducesResponseType(503)]
        public IActionResult GetFrameAsImage()
        {
            var bmpBytes = _vision.GetFrameAsPng();
            return bmpBytes != null
                ? File(bmpBytes, "image/bmp", "frame.bmp")
                : StatusCode(503, new MessageDto("Kein Frame verfügbar"));
        }
    }
}