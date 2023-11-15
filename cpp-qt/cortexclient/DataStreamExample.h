/***************
This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>
***************/
#ifndef DATASTREAMEXAMPLE_H
#define DATASTREAMEXAMPLE_H

#include <QObject>
#include "CortexClient.h"
#include "HeadsetFinder.h"
#include "SessionCreator.h"
#include "Headset.h"

/*
 * Connects to a headset and displays the data from a stream.
 *
 */
class DataStreamExample : public QObject
{
    Q_OBJECT

public:
    explicit DataStreamExample(QObject *parent = nullptr);

    // you need to activate the session if you want to get the EEG data
    void start(QString stream, bool activateSession, QString license = "");

private slots:
    void onConnected();
    void onDisconnected();
    void onErrorReceived(QString method);

    void onHeadsetFound(const Headset &headset);
    void onSessionCreated(QString token, QString sessionId);

    void onLoadProfileOk(QString profileName);
    void onSubscribeOk(QStringList streams);
    void unsubscribe();
    void onUnsubscribeOk(QStringList streams);

    void onStreamDataReceived(QString sessionId, QString stream,
                              double time, const QJsonArray &data);

    void onCloseSessionOk();

    void onAuthorizeOk(QString token);
    
    void onRefreshHeadsetList();

private:
    CortexClient client;
    HeadsetFinder finder;
    SessionCreator creator;
    Headset headset;

    bool activateSession;
    QString license;
    QString stream;
    QString token;
    QString sessionId;
    double nextDataTime;
};

#endif // DATASTREAMEXAMPLE_H
