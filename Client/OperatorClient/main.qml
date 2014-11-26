import QtQuick 2.3
import QtQuick.Controls 1.2
import QtQuick.Window 2.2

QtObject{
    id: root

    property ApplicationWindow loginWindow: ApplicationWindow{
        //id: loginWindow

        width: 640
        height: 480
        title: qsTr("Hello World")

        menuBar: MenuBar {
            Menu {
                title: qsTr("File")
                MenuItem {
                    text: qsTr("&Open")
                    onTriggered: console.log("Open action triggered");
                }
                MenuItem {
                    text: qsTr("Exit")
                    onTriggered: Qt.quit();
                }
            }
        }

        Text {
            text: qsTr("Hello World")
            anchors.centerIn: parent
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
                    onTriggered: console.log("Open action triggered");
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

    function showLoginWindow(){
        loginWindow.show();
    }
    function runOnce(){
        var i = 0;
        mainWindow.show();
    }

    Component.onCompleted: {
        mainWindow.show();
        ctx.startUserAuthentication("user", "123");
    }


}
