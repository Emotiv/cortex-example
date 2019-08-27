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
#ifndef RECORDING_H
#define RECORDING_H

#include <QObject>
#include <QTimer>
#include "CortexClient.h"
#include "HeadsetFinder.h"
#include "SessionCreator.h"

/*
 * Start a session, and insert some markers in that session.
 *
 */
class Marker : public QObject
{
    Q_OBJECT

public:
    explicit Marker(QObject *parent = nullptr);
    void start(QString license = "");

private slots:
    void onConnected();
    void onDisconnected();
    void onErrorReceived();

    void onHeadsetFound(const Headset &headset);
    void onSessionCreated(QString token, QString sessionId);
    void onRecordCreated(QString recordId);

    void injectMarker1();
    void injectMarker2();
    void injectStopMarker2();
    void onInjectMarkerOK(QString markerId);
    void onUpdateMarkerOK();

    void stopRecord();
    void onRecordStopped();
    void onGetRecordInfosOk(QJsonObject record);
    void onQueryHeadsetOk(const QList<Headset> &headsets);
    void onExportRecordOk(QString recordId);

private:
    CortexClient client;
    HeadsetFinder finder;
    SessionCreator creator;

    QString license;
    QString token;
    QString headsetId;
    QString sessionId;
    QString recordId;
    QString markerId;
};

#endif // RECORDING_H
