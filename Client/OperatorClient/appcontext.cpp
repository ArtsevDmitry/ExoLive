#include "appcontext.h"

AppContext::AppContext(QObject *parent) :
    QObject(parent)
{
}

void AppContext::startUserAuthentication(const QString login, const QString password){
    if(!_httpUserLogin){
        _httpUserLogin = new QNetworkAccessManager();
        connect(_httpUserLogin, SIGNAL(finished(QNetworkReply*)), this, SLOT(HttpUserLoginFinished(QNetworkReply*)));
    }

    QByteArray postData;
    postData.append("l").append("=").append(login).append("&")
            .append("p").append("=").append(password);

    QNetworkRequest request(QUrl("http://localhost:7777/operatorclient/auth"));
    request.setHeader(QNetworkRequest::ContentTypeHeader,"application/x-www-form-urlencoded");

    _httpUserLogin->post(request, postData);
}

void AppContext::HttpUserLoginFinished (QNetworkReply *reply){
    QString ret = reply->readAll();
    emit this->onLoginComplete(ret);
    qDebug("HttpUserLoginFinished");
    reply->deleteLater();
    _httpUserLogin->deleteLater();
}
