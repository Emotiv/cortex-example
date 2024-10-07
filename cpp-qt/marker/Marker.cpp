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
#include "Marker.h"
#include <QtDebug>
#include <QCoreApplication>
#include <QDateTime>
#include <QThread>
#include <QDir>


Marker::Marker(QObject *parent) : QObject(parent) {
    connect(&client, &CortexClient::connected, this, &Marker::onConnected);
    connect(&client, &CortexClient::disconnected, this, &Marker::onDisconnected);
    connect(&client, &CortexClient::errorReceived, this, &Marker::onErrorReceived);
    connect(&client, &CortexClient::createRecordOk, this, &Marker::onRecordCreated);
    connect(&client, &CortexClient::injectMarkerOk, this, &Marker::onInjectMarkerOK);
    connect(&client, &CortexClient::updateMarkerOk, this, &Marker::onUpdateMarkerOK);
    connect(&client, &CortexClient::stopRecordOk, this, &Marker::onRecordStopped);
    connect(&client, &CortexClient::getRecordInfosOk, this, &Marker::onGetRecordInfosOk);
    connect(&client, &CortexClient::exportRecordOk, this, &Marker::onExportRecordOk);

    connect(&finder, &HeadsetFinder::headsetFound, this, &Marker::onHeadsetFound);
    connect(&creator, &SessionCreator::sessionCreated, this, &Marker::onSessionCreated);
}

void Marker::start(QString license) {
    this->license = license;
    client.open();
}

void Marker::onConnected() {
    qInfo() << "Connected to Cortex";
    finder.findHeadsets(&client);
}

void Marker::onDisconnected() {
    qInfo() << "Disconnected";
    QCoreApplication::quit();
}

void Marker::onErrorReceived() {
    QCoreApplication::quit();
}

void Marker::onHeadsetFound(const Headset &headset) {
    this->headsetId = headset.id;
    finder.clear();
    creator.createSession(&client, headset, true, license);
}

void Marker::onSessionCreated(QString token, QString sessionId) {
    this->token = token;
    this->sessionId = sessionId;
    creator.clear();
    client.createRecord(token, sessionId, "Cortex Examples C++");
}

void Marker::onRecordCreated(QString recordId)
{
    qInfo() << "Record created, id" << recordId;
    this->recordId = recordId;

    // after a few seconds, inject some markers
    QTimer::singleShot(5*1000, this, &Marker::injectMarker1);
    QTimer::singleShot(13*1000, this, &Marker::injectMarker2);
    QTimer::singleShot(21*1000, this, &Marker::injectStopMarker2);

    // close the session after 30 seconds
    QTimer::singleShot(30*1000, this, &Marker::stopRecord);
}

void Marker::injectMarker1() {
    qInfo() << "Inject marker test1";
    client.injectMarker(token, sessionId,
                        "test1", 41,
                        QDateTime::currentMSecsSinceEpoch());
}

void Marker::injectMarker2() {
    qInfo() << "Inject marker test2";
    client.injectMarker(token, sessionId,
                        "test2", 42,
                        QDateTime::currentMSecsSinceEpoch());
}

void Marker::injectStopMarker2() {
    qInfo() << "Update marker for test2, marker id" << markerId;
    client.updateMarker(token, sessionId, markerId,
                        QDateTime::currentMSecsSinceEpoch());
}

void Marker::onInjectMarkerOK(QString markerId) {
    qInfo() << "Inject marker OK, marker id" << markerId;
    this->markerId = markerId;
}

void Marker::onUpdateMarkerOK()
{
    qInfo() << "Update marker OK";
}

void Marker::stopRecord()
{
    qInfo() << "Stopping the record";
    client.stopRecord(token, sessionId);
}

void Marker::onRecordStopped()
{
    // we can get the record now and we will display it
    client.getRecordInfos(token, recordId);
    // but to export the record we must disconnect the headset first
    client.controlDevice("disconnect", headsetId);
    // it can take a few milliseconds to disconnect the headset, so we check its status
    connect(&client, &CortexClient::queryHeadsetsOk, this, &Marker::onQueryHeadsetOk);
    client.queryHeadsets(headsetId);
}

void Marker::onGetRecordInfosOk(QJsonObject record)
{
    qDebug().noquote() << "The record:" << record;
}

void Marker::onQueryHeadsetOk(const QList<Headset> &headsets)
{
    if (headsets.isEmpty() || headsets.first().status == "discovered") {
        // the headset is disconnected, we can export the record
        QThread::sleep(1);
        QString folder = QDir::homePath();
        qInfo() << "Exporting the record to the folder" << folder;
        client.exportRecordToCSV(token, recordId, folder, {"MOTION","PM","BP"});
    }
    else {
        // try again
        client.queryHeadsets(headsetId);
    }
}

void Marker::onExportRecordOk(QString recordId)
{
    qInfo() << "The record was successfuly exported, id" << recordId;
    client.close();
}
