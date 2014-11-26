#ifndef USERINFO_H
#define USERINFO_H

#include <QObject>

class UserInfo : public QObject
{
    Q_OBJECT
    Q_PROPERTY(QString userLogin READ userLogin WRITE setUserLogin NOTIFY userLoginChanged)

public:
    explicit UserInfo(QObject *parent = 0);

    void setUserLogin(QString);
    QString userLogin() const;

signals:
    void userLoginChanged(QString);

public slots:

private:
    QString _userLogin;

};

#endif // USERINFO_H
