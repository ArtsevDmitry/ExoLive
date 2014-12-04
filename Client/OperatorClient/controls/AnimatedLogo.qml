import QtQuick 2.3
import QtQuick.Controls 1.2
import QtQuick.Window 2.2
import QtQuick.Controls.Styles 1.2
import QtGraphicalEffects 1.0

Item{
    id: my
    property real animationSpeed: 100
    function startAnimation(){
        state = "PREANIMATION"
    }

    function stopAnimation(){
        state = "SMALL"
    }

    Rectangle{
        id: img
        state: "NotAnimated"
        width: 160
        height: 150
        Image{
            id: img1; source: "qrc:/images/LogoPart1.png"
            mipmap: true
            x:32; y:0; width:48; height:76;
        }
        Image{
            id: img2; source: "qrc:/images/LogoPart2.png"
            mipmap: true
            x:83; y:31; width:77; height:45;
        }
        Image{
            id: img3; source: "qrc:/images/LogoPart3.png"
            mipmap: true
            x:91; y:71; width:41; height:81;
        }
        Image{
            id: img4; source: "qrc:/images/LogoPart4.png"
            mipmap: true
            x:31; y:96; width:77; height:56;
        }
        Image{
            id: img5; source: "qrc:/images/LogoPart5.png"
            mipmap: true
            x:0; y:58; width:69; height:66;
        }

        SequentialAnimation {
            id: animLogo
            loops: Animation.Infinite

            ParallelAnimation {
                ParallelAnimation {
                    NumberAnimation { target: img1; property: "x"; from: 32; to: 7; duration: animationSpeed; easing.type: Easing.InOutQuad;  }
                    NumberAnimation { target: img1; property: "y"; from: 0; to: -37; duration: animationSpeed; easing.type: Easing.InOutQuad;  }
                    NumberAnimation { target: img1; property: "width"; from: 48; to: 70; duration: animationSpeed; easing.type: Easing.InOutQuad;  }
                    NumberAnimation { target: img1; property: "height"; from: 76; to: 111; duration: animationSpeed; easing.type: Easing.InOutQuad;  }
                }
                ParallelAnimation {
                    NumberAnimation { target: img4; property: "x"; from: 8; to: 31; duration: animationSpeed; easing.type: Easing.InOutQuad;  }
                    NumberAnimation { target: img4; property: "y"; from: 99; to: 96; duration: animationSpeed; easing.type: Easing.InOutQuad;  }
                    NumberAnimation { target: img4; property: "width"; from: 113; to: 77; duration: animationSpeed; easing.type: Easing.InOutQuad;  }
                    NumberAnimation { target: img4; property: "height"; from: 83; to: 56; duration: animationSpeed; easing.type: Easing.InOutQuad;  }
                }
            }

            ParallelAnimation {
                ParallelAnimation {
                    NumberAnimation { target: img2; property: "y"; from: 31; to: 10; duration: animationSpeed; easing.type: Easing.InOutQuad;  }
                    NumberAnimation { target: img2; property: "width"; from: 77; to: 112; duration: animationSpeed; easing.type: Easing.InOutQuad;  }
                    NumberAnimation { target: img2; property: "height"; from: 45; to: 64; duration: animationSpeed; easing.type: Easing.InOutQuad;  }
                }
                ParallelAnimation {
                    NumberAnimation { target: img5; property: "x"; from: -35; to: 0; duration: animationSpeed; easing.type: Easing.InOutQuad;  }
                    NumberAnimation { target: img5; property: "y"; from: 46; to: 58; duration: animationSpeed; easing.type: Easing.InOutQuad;  }
                    NumberAnimation { target: img5; property: "width"; from: 102; to: 69; duration: animationSpeed; easing.type: Easing.InOutQuad;  }
                    NumberAnimation { target: img5; property: "height"; from: 96; to: 66; duration: animationSpeed; easing.type: Easing.InOutQuad;  }
                }
            }

            ParallelAnimation {
                ParallelAnimation {
                    NumberAnimation { target: img3; property: "x"; from: 91; to: 94; duration: animationSpeed; easing.type: Easing.InOutQuad;  }
                    NumberAnimation { target: img3; property: "y"; from: 71; to: 64; duration: animationSpeed; easing.type: Easing.InOutQuad;  }
                    NumberAnimation { target: img3; property: "width"; from: 41; to: 59; duration: animationSpeed; easing.type: Easing.InOutQuad;  }
                    NumberAnimation { target: img3; property: "height"; from: 81; to: 118; duration: animationSpeed; easing.type: Easing.InOutQuad;  }
                }
                ParallelAnimation {
                    NumberAnimation { target: img1; property: "x"; from: 7; to: 32; duration: animationSpeed; easing.type: Easing.InOutQuad;  }
                    NumberAnimation { target: img1; property: "y"; from: -37; to: 0; duration: animationSpeed; easing.type: Easing.InOutQuad;  }
                    NumberAnimation { target: img1; property: "width"; from: 70; to: 48; duration: animationSpeed; easing.type: Easing.InOutQuad;  }
                    NumberAnimation { target: img1; property: "height"; from: 111; to: 76; duration: animationSpeed; easing.type: Easing.InOutQuad;  }
                }
            }

            ParallelAnimation {
                ParallelAnimation {
                    NumberAnimation { target: img2; property: "y"; from: 10; to: 31; duration: animationSpeed; easing.type: Easing.InOutQuad;  }
                    NumberAnimation { target: img2; property: "width"; from: 112; to: 77; duration: animationSpeed; easing.type: Easing.InOutQuad;  }
                    NumberAnimation { target: img2; property: "height"; from: 64; to: 45; duration: animationSpeed; easing.type: Easing.InOutQuad;  }
                }
                ParallelAnimation {
                    NumberAnimation { target: img4; property: "x"; from: 31; to: 8; duration: animationSpeed; easing.type: Easing.InOutQuad;  }
                    NumberAnimation { target: img4; property: "y"; from: 96; to: 99; duration: animationSpeed; easing.type: Easing.InOutQuad;  }
                    NumberAnimation { target: img4; property: "width"; from: 77; to: 113; duration: animationSpeed; easing.type: Easing.InOutQuad;  }
                    NumberAnimation { target: img4; property: "height"; from: 56; to: 83; duration: animationSpeed; easing.type: Easing.InOutQuad;  }
                }
            }

            ParallelAnimation {
                ParallelAnimation {
                    NumberAnimation { target: img3; property: "x"; from: 94; to: 91; duration: animationSpeed; easing.type: Easing.InOutQuad;  }
                    NumberAnimation { target: img3; property: "y"; from: 64; to: 71; duration: animationSpeed; easing.type: Easing.InOutQuad;  }
                    NumberAnimation { target: img3; property: "width"; from: 59; to: 41; duration: animationSpeed; easing.type: Easing.InOutQuad;  }
                    NumberAnimation { target: img3; property: "height"; from: 118; to: 81; duration: animationSpeed; easing.type: Easing.InOutQuad;  }
                }
                ParallelAnimation {
                    NumberAnimation { target: img5; property: "x"; from: 0; to: -35; duration: animationSpeed; easing.type: Easing.InOutQuad;  }
                    NumberAnimation { target: img5; property: "y"; from: 58; to: 46; duration: animationSpeed; easing.type: Easing.InOutQuad;  }
                    NumberAnimation { target: img5; property: "width"; from: 69; to: 102; duration: animationSpeed; easing.type: Easing.InOutQuad;  }
                    NumberAnimation { target: img5; property: "height"; from: 66; to: 96; duration: animationSpeed; easing.type: Easing.InOutQuad;  }
                }
            }

        }

        //        MouseArea {
        //            anchors.fill: parent
        //            onClicked: {
        //                if(my.state === "PREANIMATION"){
        //                    my.state = "SMALL"
        //                    console.log("state === PREANIMATION");
        //                }
        //                else{
        //                    my.state = "PREANIMATION"
        //                    console.log("state === SMALL");
        //                }
        //                //animLogo.running = true;
        //            }
        //        }

    }

    state: "SMALL"

    states: [
        State{
            name: "SMALL"
            PropertyChanges { target: img1; x:32; y:0; width:48; height:76; }
            PropertyChanges { target: img2; x:83; y:31; width:77; height:45; }
            PropertyChanges { target: img3; x:91; y:71; width:41; height:81; }
            PropertyChanges { target: img4; x:31; y:96; width:77; height:56; }
            PropertyChanges { target: img5; x:0; y:58; width:69; height:66; }
        },
        State{
            name: "PREANIMATION"
            PropertyChanges { target: img1; x:32; y:0; width:48; height:76; }
            PropertyChanges { target: img2; x:83; y:31; width:77; height:45; }
            PropertyChanges { target: img3; x:91; y:71; width:41; height:81; }
            PropertyChanges { target: img4; x:8; y:99; width:113; height:83; }
            PropertyChanges { target: img5; x:-35; y:46; width:102; height:96; }
        }
    ]

    transitions: [
        Transition {
            from: "SMALL"; to: "PREANIMATION"
            SequentialAnimation {
                ParallelAnimation {
                    NumberAnimation { targets: [img1,img2,img3,img4,img5]; properties: "x,y,width,height"; easing.type: Easing.InOutQuad; duration: 200 }
                }
                ScriptAction { script: animLogo.running = true; }
            }
        },
        Transition {
            from: "PREANIMATION"; to: "SMALL"
            SequentialAnimation {
                ScriptAction { script: animLogo.running = false; }
                ParallelAnimation {
                    NumberAnimation { targets: [img1,img2,img3,img4,img5]; properties: "x,y,width,height"; easing.type: Easing.InOutQuad; duration: 200 }
                }
            }
        }
    ]
}
