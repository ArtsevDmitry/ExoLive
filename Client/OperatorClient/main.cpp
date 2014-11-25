#include <QApplication>
#include <QQmlApplicationEngine>
#include <QIcon>
//#include <QCoreApplication>

int main(int argc, char *argv[])
{
    QApplication app(argc, argv);

    app.setOrganizationName("ExoLive");
    app.setOrganizationDomain("exolive.org");
    app.setApplicationName("ExoLive Client");

    QQmlApplicationEngine engine;
    engine.load(QUrl(QStringLiteral("qrc:/login.qml")));

    return app.exec();
}
