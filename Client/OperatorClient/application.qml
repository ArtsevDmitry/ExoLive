//import "qrc:/js/global.js" as Global
import "qrc:/js/loginWindow.js" as LoginScript
import QtQuick 2.3
import QtQuick.Controls 1.2
import QtQuick.Window 2.2
import QtQuick.Controls.Styles 1.2
import "qrc:/Controls"

QtObject{
    id: root

    function ui(){return this;}

    property FontLoader fontOSCL: FontLoader { id: fontOSCL; source: "qrc:/fonts/OpenSans-CondLight.ttf" }
    property FontLoader fontOSR: FontLoader { id: fontOSR; source: "qrc:/fonts/OpenSans-Regular.ttf" }
    property FontLoader fontOSB: FontLoader { id: fontOSB; source: "qrc:/fonts/OpenSans-Bold.ttf" }
    property FontLoader fontOSI: FontLoader { id: fontOSI; source: "qrc:/fonts/OpenSans-Italic.ttf" }
    property FontLoader fontOSBI: FontLoader { id: fontOSBI; source: "qrc:/fonts/OpenSans-BoldItalic.ttf" }

    property Window loginWindow: Window{
        id: wndLogin
        width: 500
        height: 400
        title: qsTr("ExoLive - LogIn")
        color: "white"
        flags: Qt.Dialog

        Image {
            id: imageLogo
            transformOrigin: Item.TopLeft
            x: 30; y: 80
            width: 105; height: 100
            fillMode: Image.PreserveAspectFit
            smooth: true
            source: "qrc:/images/LogoBig.png"

        }

        Text {
            id: labelTitle
            transformOrigin: Item.TopLeft
            text: qsTr("ExoLive")
            font.family: fontOSCL.name
            font.pixelSize: 75
            color: "#2bd143"
            x: 146; y: 45
        }

        Text {
            id: labelSubTitle
            transformOrigin: Item.TopLeft
            text: qsTr("Please enter the details of your account to login in ExoLive system")
            font.family: fontOSR.name
            font.pixelSize: 14
            color: "#ababab"
            x: 150; y: 140
            width: 300
            wrapMode: Text.WordWrap
        }

        TextBox {
            id: textLogin
            placeholderText: qsTr("Domain / Login")
            x: 150; y: 195
            width: 235
            imageSource: "qrc:/images/UserServiceImage.png"
        }

        TextBox {
            id: textPassword
            placeholderText: qsTr("Password")
            echoMode: TextInput.Password
            x: 150; y: 240
            width: 235
            imageSource: "qrc:/images/PasswordServiceImage.png"
            state: "NORMAL"

            states: [
                State{
                    name: "NORMAL"
                    StateChangeScript { script: {textPassword.readOnly = false; textLogin.readOnly = false;} }
                    PropertyChanges { target: textPassword; y: 240; opacity: 1.0; }
                    PropertyChanges { target: textLogin; y: 195; opacity: 1.0; }
                },
                State{
                    name: "CLOSED"
                    StateChangeScript { script: {textPassword.readOnly = true; textLogin.readOnly = true;} }
                    PropertyChanges { target: textPassword; y: 300; opacity: 0.0; }
                    PropertyChanges { target: textLogin; y: 255; opacity: 0.0; }
                }
            ]

            transitions: [
                Transition {
                    from: "*"; to: "*"
                    ParallelAnimation {
                        SequentialAnimation {
                            NumberAnimation { target: textPassword; properties: "y,opacity"; easing.type: Easing.InOutQuad; duration: 300 }
                        }
                        SequentialAnimation {
                            PauseAnimation { duration: 150 }
                            NumberAnimation { target: textLogin; properties: "y,opacity"; easing.type: Easing.InOutQuad; duration: 300 }
                        }
                    }

                }
            ]
        }

        PrimaryButton {
            id: buttonLogin
            text: qsTr("LogIn")
            x: 150; y: 285
            onClicked: {
                if(textPassword.state==="NORMAL")
                    textPassword.state = "CLOSED"
                else if(textPassword.state==="CLOSED")
                    textPassword.state = "NORMAL"
                //textPassword.state = "CLOSED";
                //LoginScript.startAuthentication(textLogin.text, textPassword.text);
            }
        }

    }

    property ApplicationWindow mainWindow: ApplicationWindow{
        id: wndMain

        width: 640
        height: 480
        title: qsTr("Hello World")

        menuBar: MenuBar {
            Menu {
                title: qsTr("File")
                MenuItem {
                    text: qsTr("&Open")
                    onTriggered: {
                        console.log("Start Auth !");
                        LoginScript.startAuthentication("user", "123");
                    }
                }
                MenuItem {
                    text: qsTr("Exit")
                    onTriggered: Qt.quit();
                }
            }
        }

        Text {
            text: qsTr("MAIN WINDOW")
            anchors.centerIn: parent
        }
    }

    Component.onCompleted: {
        //Global.setMainWindow(mainWindow);
        ExoLive.onLoginComplete.connect(LoginScript.onLoginComplete);
        wndLogin.show();
        //My.startAuthentication("user", "123");
    }


}
