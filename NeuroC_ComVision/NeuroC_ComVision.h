// Der folgende ifdef-Block ist die Standardmethode zum Erstellen von Makros, die das Exportieren
// aus einer DLL vereinfachen. Alle Dateien in dieser DLL werden mit dem NEUROCCOMVISION_EXPORTS-Symbol
// (in der Befehlszeile definiert) kompiliert. Dieses Symbol darf für kein Projekt definiert werden,
// das diese DLL verwendet. Alle anderen Projekte, deren Quelldateien diese Datei beinhalten, sehen
// NEUROCCOMVISION_API-Funktionen als aus einer DLL importiert an, während diese DLL
// mit diesem Makro definierte Symbole als exportiert ansieht.
#ifdef NEUROCCOMVISION_EXPORTS
#define NEUROCCOMVISION_API __declspec(dllexport)
#else
#define NEUROCCOMVISION_API __declspec(dllimport)
#endif

// Diese Klasse wird aus der DLL exportiert.
class NEUROCCOMVISION_API CNeuroCComVision {
public:
	CNeuroCComVision(void);
	// TODO: Methoden hier hinzufügen.
};

extern NEUROCCOMVISION_API int nNeuroCComVision;

NEUROCCOMVISION_API int fnNeuroCComVision(void);
