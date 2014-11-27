import QtQuick 2.3
import QtQuick.Controls 1.2
import QtQuick.Controls.Styles 1.2

QtObject{
    id: loginStyle

    property TextFieldStyle textFieldStyle: TextFieldStyle {
        textColor: "black"
        background: Rectangle {
            radius: 2
            implicitWidth: 100
            implicitHeight: 24
            border.color: "#333"
            border.width: 1
        }
    }
}
