using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace manualisator.Core
{
    public static class Strings
    {
        public static string Language_DE = "de";
        public static string Language_EN = "en";


        public static string StepCreatingTemplateLookup = "§Schritt {0}: Lese Namen bestehender Dokumentbausteine ein";
        public static string StepLookingUpBookmarksInDocuments = "§Schritt {0}: Suche {1} Lesezeichen in {2} Dokumenten";
        public static string OpeningExistingDatabase = "Öffne bestehende Datenbank '{0}'.";
        public static string CreatingNewDatabase = "Lege Datenbank '{0}' neu an.";

        public static string TotalTimeToCreateAllManuals = "Gesamte Laufzeit für alle {0} Handbücher: {1}.";
        public static string StepCreatingManualForDeviceInLanguage = "§Schritt {0}: Erzeuge {1} für {2} in der Sprache {3}.";
        public static string FailedAfterTime = "- Fehlgeschlagen nach {0:hh\\:mm\\:ss}.";
        public static string SucceededAfterTime = "- Erfolgreich nach {0:hh\\:mm\\:ss}.";

        public static string StepReadingExcelSheets = "§Handbuch '{0}' wird erzeugt";
        public static string StepConvertingDocToDocx = "§Schritt {0}: Umwandlung von .doc zu .docx.";
        public static string AboutToRemoveFile = "- Datei '{0}' wird gelöscht.";
        public static string ConvertedDocFilesInDirectory = "- {0} Dateien wurden innerhalb von {1} umgewandelt.";
        public static string NoFilesToConvert = "- OK: Alle Dateien sind bereits konvertiert.";
        public static string FileMustBeConverted = "- Datei '{0}' wird konvertiert...";

        public static string CreatingInstanceOfWordAutomation = "- Neue Instanz von Microsoft Word wird im Hintergrund gestartet.";
        public static string AboutToSaveAsDocx = "- Datei eingelesen, wird nun als '{0}' abgespeichert.";

        public static string ExceptionWhileConvertingDocFiles = "Fehler E01 bei der Umwandlung von '{0}': Ausnahmebedingung aufgetreten.";

        public static string NumberOfTemplatesNeeded = "- Es werden {0} von {1} = {2:0.00}% der Vorlagen benötigt (Zeit zur Ermittelung: {3})";
        public static string NoTemplatesNeeded = "- Es werden keine Vorlagen benötigt (Zeit zur Ermittelung: {0})";

        public static string DatabaseHasNoExistingTemplates = "- Anzahl Vorlagen in der Datenbank: - keine -";
        public static string NumberOfTemplatesInDatabase = "- Anzahl Vorlagen in der Datenbank: {0}.";

        public static string StepUpdatingTemplates = "§Schritt {0}: Vorlagen werden angelegt.";

        public static string CreatingNewTemplateForName = "- Neue Vorlage wird für '{0}' angelegt.";
        public static string InsertedTotalOfNewTemplates = "- Insgesamt wurden {0} Vorlagen innerhalb von {1} angelegt.";
        public static string NoTemplatesToInsert = "- OK: Alle Vorlagen sind bereits angelegt.";

        public static string StepImportingManuals = "§Schritt {0}: Struktur der Handbücher wird in die Datenbank importiert.";
        public static string NumberOfManualsInDatabase = "- Es wurden {0} Handbücher innerhalb von {1} importiert.";
        public static string NoManualsToImportInDatabase = "- OK: Es wurden keine Handbücher importiert.";

        public static string ImportingManualInLanguage = "Das Handbuch '{0}' (Sprache: {1}) wird importiert...";
        public static string ManualHasSoManyParts = "- Das Handbuch enthält {0} aktive Vorlagen";

        public static string ErrorUnableToReadFile = "Fehler E02, die Datei '{0}' kann nicht gelesen werden.";

        public static string ErrorKeyNotFoundInKnownTemplates = "Fehler E03, die Vorlage {0} ist unbekannt.";

        public static string ExceptionWhileImportingManual = "Fehler E04 beim Import von '{0}': Ausnahmebedingung aufgetreten.";

        public static string ReadingExcelFile = "- Die Excel-Datei '{0}' wird eingelesen...";

        public static string DetermineWhichTemplatesAreNeeded = "§Schritt {0}: Bestimmen, welche Vorlagen überhaupt benötigt werden.";
        public static string NumberOfManualsThatNeedUpdating = "- {0} Handbücher müssen aktualisiert werden.";

        public static string WarningLanguageDoesNotMatchExpectedLanguage = "Warnung W01 in '{0}': Die Sprache ist angegeben als '{1}', erwartet wurde '{2}'";
        public static string LanguageResetToThis = "Sprache wurde angepasst in '{0}'";
        public static string ErrorLanguageIsUnknown = "Fehler E05 in '{0}': Die Sprache '{0}' ist unbekannt.";

        public static string WarningMachineDoesNotMatchExpectedMachine = "Warnung W02 in '{0}': Das Gerät ist '{1}', erwartet wurde '{2}'.";

        public static string ErrorTemplateNameIsInvalid = "Fehler E06 in '{0}': Es gibt keine Vorlage namens '{1}'.";
        public static string DidYouMean = "- Meinten Sie vielleicht {0}?";
        public static string ErrorTemplateSpecIsInvalid = "Fehler E07 in '{0}': Ungültige Angabe '{1}' für die Vorlage [Datentyp: {2}].";

        public static string ErrorValueXYShouldBeZButIsNot = "Fehler E08 in '{0}': Wert [{1},{2}] sollte '{3}' sein, ist aber '{4}'.";
        public static string ErrorRequiredValueXYMissing = "Fehler E09 in '{0}': Wert '{4}' [{1},{2}] ist notwendig, aber derzeit nicht definiert ({3}).";

        public static string ErrorExceptionWhileCreatingDocument = "Fehler E10 bei der Erzeugung des Handbuchs {0}: Ausnahmebedingung aufgetreten.";

        public static string NumberOfManualsNotImported = "Warnung W03: {0} Handbücher konnten nicht importiert werden.";

        public static string ErrorUnableToLocateBookmark = "Fehler E11: Das Lesezeichen '{0}' konnte nicht gefunden werden";

        public static string StepParsingBookmarksInExistingDocument = "§Schritt {0}: Lesezeichen werden eingelesen.";

        public static string FoundBookmarksAfterSearching = "- {0} deutsche und {1} englische Lesezeichen gefunden nach {2}.";

        public static string StepOpenIntermediateDocument = "§Schritt {0}: Öffne Hilfsdokument.";

        public static string NeedToCreateNewInstanceOfWord = "- neue Instanz von Microsoft Word(TM) wird angelegt.";

        public static string ErrorUnableToReadLookupDocument = "Fehler E12 beim Lesen des Hilfsdokuments: Ausnahmebedingung aufgetreten";

        public static string OpenedExistingLookupDocument = "- Bestehendes Hilfsdokument eingelesen in {0}.";

        public static string StepCreateLookupDocument = "§Schritt {0}: Hilfsdokument neu anlegen.";

        public static string CreatingLookupDocument = "- {0,-50} gelesen - {1,3} / {2,3} - Zeit: {3}";

        public static string ErrorUnableToCreateLookupDocument = "Fehler E13 beim Anlegen des Hilfsdokuments: Ausnahmebedingung aufgetreten";

        public static string ErrorUnableToAddTemplateContentsToLookupDocument = "Fehler E13 beim Einfügen in das Hilfsdokument: Ausnahmebedingung aufgetreten";

        public static string ErrorTemplateKeyNotFound = "Fehler E14, das Template mit der ID {0} ('{1}') wurde nicht gefunden";

        public static string FilesHasSoManyBookmarks = "- Das Dokument '{0}' hat {1} Textmarken";

        public static string ErrorDocumentFailedToConvert = "Fehler E15 beim Bearbeiten von {0}: Ausnahmebedingung aufgetreten";
        public static string ErrorBookmarkDoesNotExistAnywhere = "Fehler E16: Das Lesezeichen '{0}' existiert in keinem der angegebenen Dokumente";
        public static string ErrorFileHasNoReferencedBookmarks = "Fehler E17: Die Datei '{0}' enthält keine benutzten Lesezeichen";


        public static string SpecialHandlingForTemplateAddDirectly = "- Das Dokument '{0}' muss direkt eingefügt werden";
    }
}
