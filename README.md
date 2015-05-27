# manualisator 1.0.5
Handbuch

# Neu in Version 1.0.5

- Es kann alternativ zum Seitenumbruch vor jeder "Überschrift 1" auch ein Abschnittswechsel eingefügt werden.
- Das Programm warnt vor dem überschreiben bereits bestehender Dateien

# Version 1.0.4

## Einstellungen

Ab Version 1.0.4 gibt es einen Dialog, über den die wichtigsten Programmeinstellungen festgelegt werden können.

### Generierter Dateiname (= Dateiname nicht aus Excel übernehmen)
Wenn diese Option **NICHT** aktiviert ist, wird statt dessen der Dateiname verwendet, der in der Excel-Tabelle in dem Feld "Zieldatei" steht. 

Wenn diese Option aktiviert **IST**, wird eine Vorlage für den Dateinamen benutzt, die Variablen über %NAME% einbinden kann. Die folgenden Variablen werden unterstützt:

- `%NAME%` ist der Feldinhalt
- `%DEVICE%` ist der Feldinhalt
- `%LANGUAGE%` ist der Feldinhalt
- `%TITEL1%` ist der Feldinhalt `Titel 1`
- `%TITEL2%` ist der Feldinhalt `Titel 2`
- `%TITEL3%` ist der Feldinhalt `Titel 3` 

### Zeile "Textmarken" berücksichtigen (= nicht automatisch bestimmen)
Wenn diese Option **NICHT** ausgewählt ist, werden die zu kopierenden Textmarken "automatisch" bestimmt: Wenn die Textmarke mit `E` endet, ist es eine englische, sonst eine deutsche.

Wenn diese Option aktiviert **IST**, wird werden nur die Textmarken kopiert, die im Excel-Strukturdokument explizit angegeben sind. 

### Dokumente in der Reihenfolge der Textmarken erzeugen
Wenn diese Option **NICHT** ausgewählt ist, werden die Dokumente in der Reihenfolge erzeugt, in der sie in Excel angegeben sind.

Wenn diese Option ausgewählt **IST**, werden die Dokumente in der Reihenfolge der Textmarken erzeugt, d.h. es kann sein, dass ein Dokument mehrfach berücksichtigt wird, wenn zwei Textmarken an unterschiedlicher Position darin enthalten sind. 

### Seitenumbruch vor Überschrift 1
Wenn diese Option **NICHT** ausgewählt ist, werden keine zusätzlichen Seitenumbrüche eingefügt.

Wenn diese Option aktiviert **IST**, wird vor jedem eingefügten Text mit der Formatvorlage `Überschrift 1` ein Seitenumbruch eingefügt. 

