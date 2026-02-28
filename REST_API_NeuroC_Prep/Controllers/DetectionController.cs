using Microsoft.AspNetCore.Mvc;
using REST_API_NeuroC_Prep.Models;
using REST_API_NeuroC_Prep.Services;

namespace REST_API_NeuroC_Prep.Controllers
{
    /// <summary>
    /// Erkennungsfunktionen: Farbe, Gesicht, Kanten, Kreise.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class DetectionController : ControllerBase
    {
        private readonly VisionService _vision;

        public DetectionController(VisionService vision) => _vision = vision;

        /// <summary>
        /// Farberkennung (Rot) — HSV-Filterung.
        /// Gibt die Bounding-Box des größten roten Objekts zurück.
        /// </summary>
        [HttpGet("color")]
        [ProducesResponseType(typeof(ColorDetectionDto), 200)]
        [ProducesResponseType(503)]
        public IActionResult DetectColor()
        {
            var result = _vision.DetectColor();
            return result != null
                ? Ok(result)
                : StatusCode(503, new MessageDto("Kamera nicht aktiv"));
        }

        /// <summary>
        /// Gesichtserkennung — Haar-Cascade.
        /// Gibt bis zu 32 erkannte Gesichter mit Bounding-Boxes zurück.
        /// Cascade muss vorher über POST /api/camera/cascade geladen worden sein.
        /// </summary>
        [HttpGet("faces")]
        [ProducesResponseType(typeof(MultiDetectionDto), 200)]
        [ProducesResponseType(typeof(MessageDto), 503)]
        public IActionResult DetectFaces()
        {
            var result = _vision.DetectFaces();
            return result != null
                ? Ok(result)
                : StatusCode(503, new MessageDto(
                    "Kamera nicht aktiv oder Cascade nicht geladen. " +
                    "POST /api/camera/cascade aufrufen."));
        }

        /// <summary>
        /// Kreiserkennung — Hough-Transformation.
        /// Gibt bis zu 32 erkannte Kreise als Bounding-Boxes zurück.
        /// Typischer Einsatz: Bohrlocherkennung, Dichtungsprüfung.
        /// </summary>
        [HttpGet("circles")]
        [ProducesResponseType(typeof(MultiDetectionDto), 200)]
        [ProducesResponseType(503)]
        public IActionResult DetectCircles()
        {
            var result = _vision.DetectCircles();
            return result != null
                ? Ok(result)
                : StatusCode(503, new MessageDto("Kamera nicht aktiv"));
        }

        /// <summary>
        /// Kantenerkennung — Canny-Algorithmus.
        /// Gibt das Kantenbild als Base64-kodierte Graustufendaten zurück.
        /// Typischer Einsatz: Oberflächeninspektion, Maßkontrolle.
        /// </summary>
        [HttpGet("edges")]
        [ProducesResponseType(typeof(EdgeDetectionDto), 200)]
        [ProducesResponseType(503)]
        public IActionResult DetectEdges()
        {
            var result = _vision.DetectEdges();
            return result != null
                ? Ok(result)
                : StatusCode(503, new MessageDto("Kamera nicht aktiv"));
        }
    }
}