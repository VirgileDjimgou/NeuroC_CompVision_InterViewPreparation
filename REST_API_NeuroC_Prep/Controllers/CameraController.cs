using Microsoft.AspNetCore.Mvc;
using REST_API_NeuroC_Prep.Models;
using REST_API_NeuroC_Prep.Services;

namespace REST_API_NeuroC_Prep.Controllers
{
    /// <summary>
    /// Kamera-Steuerung: Start, Stop, Status.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class CameraController : ControllerBase
    {
        private readonly VisionService _vision;

        public CameraController(VisionService vision) => _vision = vision;

        /// <summary>Kamera-Status abfragen.</summary>
        [HttpGet("status")]
        [ProducesResponseType(typeof(CameraStatusDto), 200)]
        public IActionResult GetStatus()
        {
            return Ok(_vision.GetStatus());
        }

        /// <summary>Kamera starten.</summary>
        [HttpPost("start")]
        [ProducesResponseType(typeof(MessageDto), 200)]
        [ProducesResponseType(typeof(MessageDto), 500)]
        public IActionResult Start()
        {
            var (success, message) = _vision.Start();
            return success
                ? Ok(new MessageDto(message))
                : StatusCode(500, new MessageDto(message));
        }

        /// <summary>Kamera stoppen.</summary>
        [HttpPost("stop")]
        [ProducesResponseType(typeof(MessageDto), 200)]
        public IActionResult Stop()
        {
            var (_, message) = _vision.Stop();
            return Ok(new MessageDto(message));
        }

        /// <summary>Haar-Cascade für Gesichtserkennung laden.</summary>
        [HttpPost("cascade")]
        [ProducesResponseType(typeof(MessageDto), 200)]
        [ProducesResponseType(typeof(MessageDto), 400)]
        public IActionResult LoadCascade([FromQuery] string? path = null)
        {
            var (success, message) = _vision.LoadCascade(path);
            return success
                ? Ok(new MessageDto(message))
                : BadRequest(new MessageDto(message));
        }
    }
}