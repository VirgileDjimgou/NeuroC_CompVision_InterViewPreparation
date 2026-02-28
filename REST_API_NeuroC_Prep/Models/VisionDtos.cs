namespace REST_API_NeuroC_Prep.Models
{
    // ===== Gemeinsame Typen =====

    /// <summary>Bounding-Box eines erkannten Objekts.</summary>
    public record BoundingBoxDto(int X, int Y, int Width, int Height);

    /// <summary>Standard-Fehler/Info-Antwort.</summary>
    public record MessageDto(string Message);

    // ===== Kamera =====

    public record CameraStatusDto(bool Running, string Status);

    // ===== Frame-Informationen =====

    public record FrameInfoDto(int Width, int Height, int Stride, int Channels, int TotalBytes);

    // ===== Farberkennung =====

    public record ColorDetectionDto(bool Detected, BoundingBoxDto? BoundingBox);

    // ===== Mehrfach-Erkennung (Gesichter, Kreise) =====

    public record DetectionItemDto(int Index, BoundingBoxDto BoundingBox);

    public record MultiDetectionDto(string Type, int Count, List<DetectionItemDto> Detections);

    // ===== Frame als Bild =====

    /// <summary>RGB-Frame als Base64-kodierter String.</summary>
    public record FrameBase64Dto(int Width, int Height, int Channels, string Base64Data);

    // ===== Kanten-Bild =====

    /// <summary>Canny-Kantenbild als Base64-kodierter Graustufenstring.</summary>
    public record EdgeDetectionDto(int Width, int Height, string Base64Data);
}