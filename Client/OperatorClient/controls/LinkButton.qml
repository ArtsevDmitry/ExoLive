import QtQuick 2.3
import QtQuick.Controls 1.2
import QtQuick.Window 2.2
import QtQuick.Controls.Styles 1.2
import QtGraphicalEffects 1.0

Item{
    id:my
    property string linkText: "LinkButton"
    property alias control : text
    signal click (variant mouse);

    Text{
        id:text
        wrapMode: Text.WordWrap
        color: ma.pressed ? "#0682b5" : ma.containsMouse ? "#00b4ff" : "#07a1e1"
        text: ma.pressed ? linkText : ma.containsMouse ? "<u>" + linkText + "</u>" : linkText
        font.family: fontOSB.name
        font.pixelSize: 14

        //Loader { id: loader }
        MouseArea {
            id: ma
            anchors.fill: parent
            cursorShape: Qt.PointingHandCursor;
            acceptedButtons: Qt.LeftButton
            hoverEnabled: true
            onClicked: my.click(mouse)
        }
    }
}
