// Der folgende ifdef-Block ist die Standardmethode zum Erstellen von Makros, die das Exportieren
// aus einer DLL vereinfachen. Alle Dateien in dieser DLL werden mit dem NEUROCCOMVISION_EXPORTS-Symbol
// (in der Befehlszeile definiert) kompiliert. Dieses Symbol darf für kein Projekt definiert werden,
// das diese DLL verwendet. Alle anderen Projekte, deren Quelldateien diese Datei beinhalten, sehen
// NEUROCCOMVISION_API-Funktionen als aus einer DLL importiert an, während diese DLL
// mit diesem Makro definierte Symbole als exportiert ansieht.

#pragma once

#ifdef NEUROC_COMVISION_EXPORTS
#define NEUROC_API __declspec(dllexport)
#else
#define NEUROC_API __declspec(dllimport)
#endif

extern "C"
{
    struct DetectionResult
    {
        int x;
        int y;
        int width;
        int height;
        bool detected;
    };

    NEUROC_API bool StartCamera();
    NEUROC_API bool GetFrame(DetectionResult* result);
    NEUROC_API void StopCamera();
}
