import QtQuick 2.3
import QtQuick.Controls 1.2
import QtQuick.Controls.Styles 1.2

TextField {
    id: textBox
    property string imageSource

    style: TextFieldStyle {
    textColor: "#4b4b4b"
    placeholderTextColor: "#dadada"
    padding.left: 35
    padding.right: 8
    font.family: ui().fontOSR.name
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
            source: textBox.imageSource
            x: 8
            y: (parent.implicitHeight/2) - (width / 2)
        }
    }
}
}
