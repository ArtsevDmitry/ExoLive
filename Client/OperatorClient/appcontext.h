#ifndef APPCONTEXT_H
#define APPCONTEXT_H

#include <QObject>
#include <QNetworkAccessManager>
#include <QUrl>
#include <QUrlQuery>
#include <QByteArray>
#include <QNetworkRequest>
#include <QNetworkReply>
#include <QQmlContext>
#include <QMetaObject>
#include <QQmlComponent>
#include <QPointer>

class AppContext : public QObject
{
    Q_OBJECT
public:
    explicit AppContext(QObject *parent = 0);

    Q_INVOKABLE void startUserAuthentication(const QString, const QString);

signals:

public slots:
    void HttpUserLoginFinished (QNetworkReply*);

private:
    QPointer<QNetworkAccessManager> _httpUserLogin = NULL;
};

#endif // APPCONTEXT_H
