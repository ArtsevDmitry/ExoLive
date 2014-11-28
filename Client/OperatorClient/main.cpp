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

    app.setOrganizationName("ExoLive");
    app.setOrganizationDomain("exolive.org");
    app.setApplicationName("ExoLive Client");

    QQmlApplicationEngine engine;
    engine.rootContext()->setContextProperty("ExoLive", appContext);
    engine.load(QUrl(QStringLiteral("qrc:/application.qml")));

    return app.exec();
}
