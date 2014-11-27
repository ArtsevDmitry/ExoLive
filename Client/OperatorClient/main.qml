import "qrc:/js/loginWindow.js" as My
import QtQuick 2.3
import QtQuick.Controls 1.2
import QtQuick.Window 2.2
import QtQuick.Controls.Styles 1.2

QtObject{
    id: root

    property FontLoader fontOSCL: FontLoader { id: fontOSCL; source: "qrc:/fonts/OpenSans-CondLight.ttf" }
    property FontLoader fontOSR: FontLoader { id: fontOSR; source: "qrc:/fonts/OpenSans-Regular.ttf" }
    property FontLoader fontOSB: FontLoader { id: fontOSB; source: "qrc:/fonts/OpenSans-Bold.ttf" }
    property FontLoader fontOSI: FontLoader { id: fontOSI; source: "qrc:/fonts/OpenSans-Italic.ttf" }
    property FontLoader fontOSBI: FontLoader { id: fontOSBI; source: "qrc:/fonts/OpenSans-BoldItalic.ttf" }

    property Component loginFieldStyle: TextFieldStyle {
        textColor: "#4b4b4b"
        placeholderTextColor: "#dadada"
        padding.left: 35
        padding.right: 8
        font.family: fontOSR.name
        font.pixelSize: 14
        background: Rectangle {
            radius: 4
            color: "white"
            implicitWidth: 100
            implicitHeight: 36
            border.width: 1
            border.color: "#cfcfcf"
            Image{
                width:22; height:22
                smooth: true
                fillMode: Image.PreserveAspectFit
                source: "qrc:/images/UserServiceImage.png"
                x: 8
                y: (parent.implicitHeight/2) - (width / 2)
            }
        }
    }
    property Component passwordFieldStyle: TextFieldStyle {
        textColor: "#4b4b4b"
        placeholderTextColor: "#dadada"
        padding.left: 35
        padding.right: 8
        font.family: fontOSR.name
        font.pixelSize: 14
        background: Rectangle {
            radius: 4
            color: "white"
            implicitWidth: 100
            implicitHeight: 36
            border.width: 1
            border.color: "#cfcfcf"
            Image{
                width:22; height:22
                smooth: true
                fillMode: Image.PreserveAspectFit
                source: "qrc:/images/PasswordServiceImage.png"
                x: 8
                y: (parent.implicitHeight/2) - (width / 2)
            }
        }
    }
    property Component submitButtonStyle: ButtonStyle {
        background: Rectangle {
                    implicitWidth: 100
                    implicitHeight: 35
                    border.width: control.activeFocus ? 1 : 0
                    border.color: "#00648d"
                    radius: 4
                    color: control.pressed ? "#077aaa" : control.hovered ? "#00b4ff" : "#07a1e1"
                }
        label: Component {
            Text {
                text: control.text
                clip: true
                wrapMode: Text.WordWrap
                verticalAlignment: Text.AlignVCenter
                horizontalAlignment: Text.AlignHCenter
                anchors.fill: parent
                font.family: fontOSB.name
                font.pixelSize: 14
                color: "white"
            }
        }
    }

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

        TextField {
            id: textLogin
            placeholderText: qsTr("Domain / Login")
            x: 150; y: 195
            width: 235
            style: loginFieldStyle
        }

        TextField {
            id: textPassword
            placeholderText: qsTr("Password")
            echoMode: TextInput.Password
            x: 150; y: 240
            width: 235
            style: passwordFieldStyle
        }

        Button {
            id: buttonLogin
            text: qsTr("LogIn")
            x: 150; y: 285
            //width: 120
            style: submitButtonStyle
        }

//        Rectangle{
//            y: buttonLogin.y + (buttonLogin.height/2)
//            width: 150
//            height: 30
//            anchors.left: buttonLogin.right
//            anchors.leftMargin: 20
//            MouseArea {
//                id: labelSettingsArea
//                hoverEnabled: true
//                anchors.fill: parent

//                onEntered: {
//                    console.log("SplitVCursor");
//                    cursorShape=Qt.SplitVCursor;
//                }
//                onExited: {
//                    console.log("ArrowCursor");
//                    cursorShape = Qt.ArrowCursor;
//                }
//            }
//            color: "green"

//            Text{
//                id: labelSettings
//                anchors.fill: parent
//                text: qsTr("SETTINGS")
//                font.family: fontOSR.name
//                font.pixelSize: 14

//                color: "#07a1e1"
//            }
//        }


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
                        My.startAuthentication("user", "123");
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
        wndLogin.show();
        //My.startAuthentication("user", "123");
    }


}
