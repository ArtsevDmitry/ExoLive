import QtQuick 2.3
import QtQuick.Controls 1.2
import QtQuick.Window 2.2
import QtQuick.Controls.Styles 1.2
import QtGraphicalEffects 1.0

Item{
Rectangle{
    id: img
    x:50; y:50;
    state: "NotAnimated"
    color: "yellow"
        width: 160
        height: 150
       Image{
           id: img1; source: "http://localhost:5050/LogoPart1.png"
           x:32; y:0;
       }
       Image{
           id: img2; source: "http://localhost:5050/LogoPart2.png"
           x:83; y:31;
       }
       Image{
           id: img3; source: "http://localhost:5050/LogoPart3.png"
           x:91; y:71;
       }
       Image{
           id: img4; source: "http://localhost:5050/LogoPart4.png"
           x:31; y:96;
       }
       Image{
           id: img5; source: "http://localhost:5050/LogoPart5.png"
           x:0; y:58;
       }
       Colorize {
           anchors.fill: img1
           source: img1
           hue: 0.5
           saturation: 78.0
           lightness: -10.0
       }
states:[
 State{
  name: "NotAnimated"
  PropertyChanges { target: img1; x:32; y:0;}
  PropertyChanges { target: img2; x:83; y:31;}
 },
 State{
  name: "Animated"
  PropertyChanges { target: img1; x:22; y:-10;}
  PropertyChanges { target: img2; x:93; y:21;}
 }
]
transitions: [
 Transition {
             from: "*"; to: "*"
             SequentialAnimation{
             NumberAnimation { target: img1; properties: "x,y"; easing.type: Easing.InOutQuad; duration: 200 }
             NumberAnimation { target: img2; properties: "x,y"; easing.type: Easing.InOutQuad; duration: 200 }
             }
         }
]

    MouseArea {
        anchors.fill: parent
        onClicked: {
            if(img.state === "NotAnimated")
                img.state = "Animated";
    else
        img.state = "NotAnimated";
        }
    }

}
}
