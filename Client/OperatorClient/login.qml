import QtQuick 2.0
import QtQuick.Controls 1.2
import QtQuick.Window 2.2

Window{
    id: window1
    FontLoader { id: fontOSCL; source: "qrc:/fonts/OpenSans-CondLight.ttf" }
    FontLoader { id: fontOSR; source: "qrc:/fonts/OpenSans-Regular.ttf" }
    FontLoader { id: fontOSB; source: "qrc:/fonts/OpenSans-Bold.ttf" }
    FontLoader { id: fontOSI; source: "qrc:/fonts/OpenSans-Italic.ttf" }
    FontLoader { id: fontOSBI; source: "qrc:/fonts/OpenSans-BoldItalic.ttf" }
    //id: loginWindow

        width: 500
        height: 400
        title: qsTr("ExoLive - LogIn")
}
