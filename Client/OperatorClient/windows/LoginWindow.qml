import QtQuick 2.3
import QtQuick.Controls 1.2
import QtQuick.Window 2.2
import QtQuick.Controls.Styles 1.2
import "qrc:/Controls"
import "qrc:/js/loginWindow.js" as LoginScript

Window{
    id: wnd
    width: 500
    height: 400
    minimumHeight: height
    minimumWidth: width
    maximumHeight: height
    maximumWidth: width
    title: qsTr("ExoLive - LogIn")
    color: "white"
    flags: Qt.Dialog

    Rectangle{
        x:0;y:0; width:10;height:10;color:"yellow"
        MouseArea{
            anchors.fill: parent
            onClicked: animationObject.state="NORMAL";
        }
    }

    AnimatedLogo{
        id: imageLogo
        x: 30; y: 80
        scale: 0.65
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
    }

    PrimaryButton {
        id: buttonLogin
        text: qsTr("LogIn")
        x: 150; y: 285
        onClicked: {
            if(animationObject.state==="NORMAL")
                animationObject.state = "CLOSED"
            else if(animationObject.state==="CLOSED")
                animationObject.state = "NORMAL"
            //textPassword.state = "CLOSED";
            //LoginScript.startAuthentication(textLogin.text, textPassword.text);
        }
    }

    LinkButton{
        id: buttonSettings
        linkText: qsTr("Settings")
        x: 270; y: 290
        control.font.pixelSize: 18
        onClick: ui().mainWindow.show()
    }

    Text {
        id: labelPleaseWait
        transformOrigin: Item.TopLeft
        text: qsTr("Logging in (please wait)")
        font.family: fontOSR.name
        font.pixelSize: 14
        color: "#ababab"
        x: 175; y: 280
        width: 300
        wrapMode: Text.WordWrap
    }

    Item{
        id: animationObject

        state: "NORMAL"

        states: [
            State{
                name: "NORMAL"
                StateChangeScript { script: {
                        textPassword.readOnly = false;
                        textLogin.readOnly = false;
                        buttonLogin.visible = true;
                        buttonSettings.visible = true;
                        labelPleaseWait.visible = false;
                    }
                }
                PropertyChanges { target: textPassword; y: 240; opacity: 1.0; }
                PropertyChanges { target: textLogin; y: 195; opacity: 1.0; }
                PropertyChanges { target: labelTitle; x: 146; y: 45; font.pixelSize: 75; }
                PropertyChanges { target: labelSubTitle; y: 140; opacity: 1.0; }
                PropertyChanges { target: buttonLogin; y: 285; opacity: 1.0; }
                PropertyChanges { target: buttonSettings; y: 290; opacity: 1.0; }
                PropertyChanges { target: imageLogo; x: 30; y: 80; scale: 0.65; }
                PropertyChanges { target: labelPleaseWait; x: 175; y: 200; opacity: 0.0; }
            },
            State{
                name: "CLOSED"
                StateChangeScript { script: {
                        textPassword.readOnly = true;
                        textLogin.readOnly = true;
                        buttonLogin.visible = false;
                        buttonSettings.visible = false;
                        labelPleaseWait.visible = true;
                    }
                }
                PropertyChanges { target: textPassword; y: 300; opacity: 0.0; }
                PropertyChanges { target: textLogin; y: 255; opacity: 0.0; }
                PropertyChanges { target: labelTitle; x: 196; y: 300; font.pixelSize: 46; }
                PropertyChanges { target: labelSubTitle; y: 200; opacity: 0.0; }
                PropertyChanges { target: buttonLogin; y: 345; opacity: 0.0; }
                PropertyChanges { target: buttonSettings; y: 350; opacity: 0.0; }
                PropertyChanges { target: imageLogo; x: (wnd.width/2)-(160/2); y: 90; scale: 1.0; }
                PropertyChanges { target: labelPleaseWait; x: 175; y: 280; opacity: 1.0; }
            }
        ]

        transitions: [
            Transition {
                from: "NORMAL"; to: "CLOSED"
                ParallelAnimation {
                    SequentialAnimation {
                        NumberAnimation { target: buttonSettings; properties: "y,opacity"; easing.type: Easing.InOutQuad; duration: 200 }
                    }
                    SequentialAnimation {
                        PauseAnimation { duration: 50 }
                        NumberAnimation { target: buttonLogin; properties: "y,opacity"; easing.type: Easing.InOutQuad; duration: 200 }
                    }
                    SequentialAnimation {
                        PauseAnimation { duration: 100 }
                        NumberAnimation { target: textPassword; properties: "y,opacity"; easing.type: Easing.InOutQuad; duration: 300 }
                    }
                    SequentialAnimation {
                        PauseAnimation { duration: 200 }
                        NumberAnimation { target: textLogin; properties: "y,opacity"; easing.type: Easing.InOutQuad; duration: 300 }
                    }
                    SequentialAnimation {
                        PauseAnimation { duration: 250 }
                        NumberAnimation { target: labelSubTitle; properties: "y,opacity"; easing.type: Easing.InOutQuad; duration: 300 }
                    }
                    SequentialAnimation {
                        PauseAnimation { duration: 280 }
                        NumberAnimation { target: labelTitle; properties: "x,y,font.pixelSize"; easing.type: Easing.InOutQuad; duration: 300 }
                    }
                    SequentialAnimation {
                        PauseAnimation { duration: 300 }
                        NumberAnimation { target: labelPleaseWait; properties: "x,y,opacity"; easing.type: Easing.InOutQuad; duration: 300 }
                    }
                    SequentialAnimation {
                        PauseAnimation { duration: 260 }
                        NumberAnimation { target: imageLogo; properties: "x,y,scale"; easing.type: Easing.InOutQuad; duration: 300 }
                        ScriptAction { script: imageLogo.startAnimation(); }
                    }
                }

            },
            Transition {
                from: "CLOSED"; to: "NORMAL"
                ParallelAnimation {
                    SequentialAnimation {
                        NumberAnimation { target: imageLogo; properties: "x,y,scale"; easing.type: Easing.InOutQuad; duration: 300 }
                        ScriptAction { script: imageLogo.stopAnimation(); }
                    }
                    SequentialAnimation {
                        PauseAnimation { duration: 50 }
                        NumberAnimation { target: labelTitle; properties: "x,y,font.pixelSize"; easing.type: Easing.InOutQuad; duration: 300 }
                    }
                    SequentialAnimation {
                        PauseAnimation { duration: 100 }
                        NumberAnimation { target: labelSubTitle; properties: "y,opacity"; easing.type: Easing.InOutQuad; duration: 300 }
                    }
                    SequentialAnimation {
                        PauseAnimation { duration: 200 }
                        NumberAnimation { target: textLogin; properties: "y,opacity"; easing.type: Easing.InOutQuad; duration: 300 }
                    }
                    SequentialAnimation {
                        PauseAnimation { duration: 250 }
                        NumberAnimation { target: textPassword; properties: "y,opacity"; easing.type: Easing.InOutQuad; duration: 300 }
                    }
                    SequentialAnimation {
                        PauseAnimation { duration: 300 }
                        NumberAnimation { target: labelPleaseWait; properties: "x,y,opacity"; easing.type: Easing.InOutQuad; duration: 300 }
                    }
                    SequentialAnimation {
                        PauseAnimation { duration: 280 }
                        NumberAnimation { target: buttonLogin; properties: "y,opacity"; easing.type: Easing.InOutQuad; duration: 200 }
                    }
                    SequentialAnimation {
                        PauseAnimation { duration: 260 }
                        NumberAnimation { target: buttonSettings; properties: "y,opacity"; easing.type: Easing.InOutQuad; duration: 200 }
                    }
                }

            }

        ]
    }

}
