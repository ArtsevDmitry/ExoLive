#include <QApplication>
#include <QQmlApplicationEngine>
#include <QIcon>
#include <QQmlContext>
#include <QQmlComponent>
#include <QWindow>
#include <QPointer>

#include "userinfo.h"
#include "appcontext.h"

int main(int argc, char *argv[])
{
    QApplication app(argc, argv);

    AppContext *appContext = new AppContext();

  /*  UserInfo *userInfo = new UserInfo();
    QString str = QString("Эй ты!");
    userInfo->setUserLogin(str)*/;

    app.setOrganizationName("ExoLive");
    app.setOrganizationDomain("exolive.org");
    app.setApplicationName("ExoLive Client");

//    QQmlApplicationEngine engine;
//    engine.rootContext()->setContextProperty("ctx", appContext);
//    QQmlComponent component(&engine);
//    component.loadUrl(QUrl(QStringLiteral("qrc:/main.qml")));
//    if (!component.isReady()) {
//        qWarning("%s", qPrintable(component.errorString()));
//        return -1;
//    }
//    QObject *topLevel = component.create();

    //QWindow *wnd = topLevel->findChild<QWindow*>("wndMain");

    QQmlApplicationEngine engine;
    engine.rootContext()->setContextProperty("ctx", appContext);
    engine.load(QUrl(QStringLiteral("qrc:/main.qml")));

    //QQmlContext *context = new QQmlContext(engine.rootContext());
    //QObject *root = engine.findChild<QObject*>("root");
    //engine.rootContext()->setContextProperty(QString("ctx"), appContext);

    //QMetaObject::invokeMethod(&engine, "runOnce");

    return app.exec();
}
