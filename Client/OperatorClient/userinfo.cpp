#include "userinfo.h"

UserInfo::UserInfo(QObject *parent) :
    QObject(parent)
{
}

void UserInfo::setUserLogin(QString value){
    _userLogin = value;
    emit userLoginChanged(value);
}

QString UserInfo::userLogin() const{
    return _userLogin;
}
