import QtQuick 2.3
import QtQuick.Controls 1.2
import QtQuick.Controls.Styles 1.2

Button {
    style: ButtonStyle {
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
}
