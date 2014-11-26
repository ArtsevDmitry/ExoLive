TEMPLATE = app

QT += qml quick widgets network

SOURCES += main.cpp \
    appcontext.cpp \
    userinfo.cpp

RESOURCES += qml.qrc

CONFIG += qml_debug

RC_FILE = standard.rc

# Additional import path used to resolve QML modules in Qt Creator's code model
QML_IMPORT_PATH =

# Default rules for deployment.
include(deployment.pri)

OTHER_FILES += \
    standard.rc

HEADERS += \
    appcontext.h \
    userinfo.h
