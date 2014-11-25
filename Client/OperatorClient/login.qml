import QtQuick 2.0
import QtQuick.Controls 1.2
import QtQuick.Window 2.2

Window {
    visible: true
    width: 400
    height: 300
    maximumWidth: 400
    maximumHeight: 300
    minimumWidth: 400
    minimumHeight: 300
    title: qsTr("ExoLive - Login")


    Text {
        text: qsTr("Login window")
        anchors.centerIn: parent
    }
}
