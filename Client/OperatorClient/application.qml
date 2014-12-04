//import "qrc:/js/global.js" as Global
import "qrc:/js/loginWindow.js" as LoginScript
import QtQuick 2.3
import QtQuick.Controls 1.2
import QtQuick.Window 2.2
import QtQuick.Controls.Styles 1.2
import "qrc:/Controls"
import "qrc:/Windows"

QtObject{
    id: root

    function ui(){return this;}

    property FontLoader fontOSCL: FontLoader { id: fontOSCL; source: "qrc:/fonts/OpenSans-CondLight.ttf" }
    property FontLoader fontOSR: FontLoader { id: fontOSR; source: "qrc:/fonts/OpenSans-Regular.ttf" }
    property FontLoader fontOSB: FontLoader { id: fontOSB; source: "qrc:/fonts/OpenSans-Bold.ttf" }
    property FontLoader fontOSI: FontLoader { id: fontOSI; source: "qrc:/fonts/OpenSans-Italic.ttf" }
    property FontLoader fontOSBI: FontLoader { id: fontOSBI; source: "qrc:/fonts/OpenSans-BoldItalic.ttf" }

    property Window loginWindow: LoginWindow{}
    property ApplicationWindow mainWindow: MainWindow{}

    Component.onCompleted: {
        //Global.setMainWindow(mainWindow);
        ExoLive.onLoginComplete.connect(LoginScript.onLoginComplete);
        loginWindow.show();
        //My.startAuthentication("user", "123");
    }


}
