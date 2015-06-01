!include "MUI2.nsh"
SetCompressor /SOLID lzma

Name "MANUALISATOR"

OutFile "MANUALISATOR-SETUP-1.0.5.EXE"

InstallDir "$PROGRAMFILES\MANUALISATOR"
InstallDirRegKey HKLM "SOFTWARE\p-nand-q.com\manualisator" "Install_Dir"

  RequestExecutionLevel admin
  
  !insertmacro MUI_PAGE_WELCOME
  !insertmacro MUI_PAGE_DIRECTORY
  !insertmacro MUI_PAGE_INSTFILES
  
   
    !define MUI_FINISHPAGE_NOAUTOCLOSE
    !define MUI_FINISHPAGE_RUN    
    !define MUI_FINISHPAGE_RUN_TEXT "MANUALISATOR ausführen"
    !define MUI_FINISHPAGE_RUN_FUNCTION "LaunchLink"
    !define MUI_FINISHPAGE_SHOWREADME_NOTCHECKED
    !define MUI_FINISHPAGE_SHOWREADME "$INSTDIR\readme.html"

  !insertmacro MUI_PAGE_FINISH

  !insertmacro MUI_LANGUAGE "German"
!include LogicLib.nsh


Function .onInit
UserInfo::GetAccountType
pop $0
${If} $0 != "admin" ;Require admin rights on NT4+
    MessageBox mb_iconstop "Administrator rights required!"
    SetErrorLevel 740 ;ERROR_ELEVATION_REQUIRED
    Quit
${EndIf}
FunctionEnd

Section "MANUALISATOR (required)"

    SectionIn RO

    ; Set output path to the installation directory.
    SetOutPath "$INSTDIR"

    ; Put file there
    File /R "..\MANUALISATOR\bin\Release\*"
    File /R "..\readme.html"

    ; Write the installation path into the registry
    WriteRegStr HKLM "Software\p-nand-q.com\MANUALISATOR" "Install_Dir" "$INSTDIR"
    CreateShortCut "$DESKTOP\MANUALISATOR.lnk" "$INSTDIR\MANUALISATOR.exe" "" "$INSTDIR\MANUALISATOR.exe" 0    

    CreateDirectory "$SMPROGRAMS\MANUALISATOR"
    CreateShortCut "$SMPROGRAMS\MANUALISATOR\MANUALISATOR.lnk" "$INSTDIR\MANUALISATOR.exe" "" "$INSTDIR\MANUALISATOR.exe" 0    
    CreateShortCut "$SMPROGRAMS\MANUALISATOR\Release Notes.lnk" "$INSTDIR\readme.html" "" "$INSTDIR\readme.html" 0    

SectionEnd

Function LaunchLink
  ExecShell "" "$INSTDIR\MANUALISATOR.exe"
FunctionEnd

