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
#include "CortexClient.h"

#include <QUrl>
#include <QJsonObject>
#include <QJsonDocument>
#include <QJsonParseError>
#include <QJsonArray>
#include <QtDebug>
#include <QCoreApplication>


// utility function
QStringList arrayToStringList(const QJsonArray &array) {
    QStringList list;
    for (const QJsonValue &val : array) {
        list.append(val.toString());
    }
    return list;
}

CortexClient::CortexClient(QObject *parent) : QObject(parent) {
    nextRequestId = 1;

    // forward the connected/disconnected signals
    connect(&socket, &QWebSocket::connected, this, &CortexClient::connected);
    connect(&socket, &QWebSocket::disconnected, this, &CortexClient::disconnected);

    // handle errors
    connect(&socket, static_cast<void(QWebSocket::*)(QAbstractSocket::SocketError)>(&QWebSocket::error),
            this, &CortexClient::onError);
    connect(&socket, &QWebSocket::sslErrors, this, &CortexClient::onSslErrors);

    // handle incomming text messages
    connect(&socket, &QWebSocket::textMessageReceived, this, &CortexClient::onMessageReceived);
}

void CortexClient::onError(QAbstractSocket::SocketError error) {
    qCritical() << "Socket error:" << error;
}

void CortexClient::onSslErrors(const QList<QSslError> &errors) {
    for (const QSslError &error : errors) {
        qCritical() << "SSL error:" << error.errorString();
    }
}

void CortexClient::open() {
    socket.open(QUrl("wss://localhost:6868"));
}

void CortexClient::close() {
    socket.close();
    nextRequestId = 1;
    methodForRequestId.clear();
}

void CortexClient::queryHeadsets(QString headsetId) {
    QJsonObject params;
    if (! headsetId.isEmpty()) {
        params["id"] = headsetId;
    }
    sendRequest("queryHeadsets", params);
}

void CortexClient::getUserLogin() {
    sendRequest("getUserLogin");
}

void CortexClient::requestAccess(QString clientId, QString clientSecret)
{
    QJsonObject params;
    params["clientId"] = clientId;
    params["clientSecret"] = clientSecret;
    sendRequest("requestAccess", params);
}

void CortexClient::authorize(QString clientId, QString clientSecret, QString license, int debit) {
    QJsonObject params;
    params["clientId"] = clientId;
    params["clientSecret"] = clientSecret;
    if (! license.isEmpty()) {
        params["license"] = license;
    }
    params["debit"] = debit;
    sendRequest("authorize", params);
}

void CortexClient::controlDevice(QString command, QString headsetId, QJsonObject flexMapping)
{
    QJsonObject params;
    params["headset"] = headsetId;
    params["command"] = command;
    if (! flexMapping.isEmpty()) {
        params["mappings"] = flexMapping;
    }
    sendRequest("controlDevice", params);
}

void CortexClient::createSession(QString token, QString headsetId, bool activate) {
    QJsonObject params;
    params["cortexToken"] = token;
    params["headset"] = headsetId;
    params["status"] = activate ? "active" : "open";
    sendRequest("createSession", params);
}

void CortexClient::closeSession(QString token, QString sessionId) {
    QJsonObject params;
    params["cortexToken"] = token;
    params["session"] = sessionId;
    params["status"] = "close";
    sendRequest("updateSession", params);
}

void CortexClient::subscribe(QString token, QString sessionId, QString stream) {
    QJsonObject params;
    QJsonArray streamArray;
    streamArray.append(stream);
    params["cortexToken"] = token;
    params["session"] = sessionId;
    params["streams"] = streamArray;
    sendRequest("subscribe", params);
}

void CortexClient::unsubscribe(QString token, QString sessionId, QString stream) {
    QJsonObject params;
    QJsonArray streamArray;
    streamArray.append(stream);
    params["cortexToken"] = token;
    params["session"] = sessionId;
    params["streams"] = streamArray;
    sendRequest("unsubscribe", params);
}

void CortexClient::queryProfile(QString token)
{
    QJsonObject params;
    params["cortexToken"] = token;
    sendRequest("queryProfile", params);
}

void CortexClient::createProfile(QString token, QString headsetId, QString profileName)
{
    QJsonObject params;
    params["cortexToken"] = token;
    params["profile"] = profileName;
    params["headset"] = headsetId;
    params["status"] = "create";
    sendRequest("setupProfile", params);
}

void CortexClient::loadProfile(QString token, QString headsetId, QString profileName)
{
    QJsonObject params;
    params["cortexToken"] = token;
    params["headset"] = headsetId;
    params["profile"] = profileName;
    params["status"] = "load";
    sendRequest("setupProfile", params);
}

void CortexClient::saveProfile(QString token, QString headsetId, QString profileName)
{
    QJsonObject params;
    params["cortexToken"] = token;
    params["headset"] = headsetId;
    params["profile"] = profileName;
    params["status"] = "save";
    sendRequest("setupProfile", params);
}

void CortexClient::getDetectionInfo(QString detection) {
    QJsonObject params;
    params["detection"] = detection;
    sendRequest("getDetectionInfo", params);
}

void CortexClient::training(QString token, QString sessionId, QString detection,
                            QString action, QString control) {
    QJsonObject params;
    params["cortexToken"] = token;
    params["session"] = sessionId;
    params["detection"] = detection;
    params["action"] = action;
    params["status"] = control;
    sendRequest("training", params);
}

void CortexClient::createRecord(QString token, QString sessionId, QString title)
{
    QJsonObject params;
    params["cortexToken"] = token;
    params["session"] = sessionId;
    params["title"] = title;
    sendRequest("createRecord", params);
}

void CortexClient::stopRecord(QString token, QString sessionId)
{
    QJsonObject params;
    params["cortexToken"] = token;
    params["session"] = sessionId;
    sendRequest("stopRecord", params);
}

void CortexClient::getRecordInfos(QString token, QString recordId)
{
    QJsonObject params;
    params["cortexToken"] = token;
    params["recordIds"] = QJsonArray{recordId};
    sendRequest("getRecordInfos", params);
}

void CortexClient::exportRecordToCSV(QString token, QString recordId,
                                     QString folder, QStringList streams)
{
    QJsonObject params;
    params["cortexToken"] = token;
    params["recordIds"] = QJsonArray{recordId};
    params["folder"] = folder;
    params["streamTypes"] = QJsonArray::fromStringList(streams);
    params["format"] = "CSV";
    params["version"] = "V2";
    sendRequest("exportRecord", params);
}

void CortexClient::injectMarker(QString token, QString sessionId,
                                QString label, int value, qint64 time) {
    QJsonObject params;
    params["cortexToken"] = token;
    params["session"] = sessionId;
    params["label"] = label;
    params["value"] = value;
    params["port"] = "Cortex Example";
    params["time"] = time;
    sendRequest("injectMarker", params);
}

void CortexClient::updateMarker(QString token, QString sessionId, QString markerId, qint64 time)
{
    QJsonObject params;
    params["cortexToken"] = token;
    params["session"] = sessionId;
    params["markerId"] = markerId;
    params["time"] = time;
    sendRequest("updateMarker", params);
}

void CortexClient::getTrainedSignatureActions(QString token, QString detection, QString profileName)
{
    QJsonObject params;
    params["cortexToken"] = token;
    params["detection"] = detection;
    params["profile"] = profileName;
    sendRequest("getTrainedSignatureActions", params);
}

void CortexClient::mentalCommandActiveAction(QString token, QString profileName)
{
    QJsonObject params;
    params["cortexToken"] = token;
    params["status"] = "get";
    params["profile"] = profileName;
    sendRequest("mentalCommandActiveAction", params);
}

void CortexClient::mentalCommandBrainMap(QString token, QString profileName)
{
    QJsonObject params;
    params["cortexToken"] = token;
    params["profile"] = profileName;
    sendRequest("mentalCommandBrainMap", params);
}

void CortexClient::mentalCommandTrainingThreshold(QString token, QString profileName)
{
    QJsonObject params;
    params["cortexToken"] = token;
    params["profile"] = profileName;
    sendRequest("mentalCommandTrainingThreshold", params);
}

void CortexClient::mentalCommandActionSensitivity(QString token, QString profileName)
{
    QJsonObject params;
    params["cortexToken"] = token;
    params["status"] = "get";
    params["profile"] = profileName;
    sendRequest("mentalCommandActionSensitivity", params);
}

void CortexClient::sendRequest(QString method, QJsonObject params) {
    QJsonObject request;

    // build the request
    request["jsonrpc"] = "2.0";
    request["id"] = nextRequestId;
    request["method"] = method;
    request["params"] = params;

    // send the json message
    QString message = QJsonDocument(request).toJson(QJsonDocument::Compact);
    qDebug().noquote() << " * send    " << message;
    socket.sendTextMessage(message);

    // remember the method used for this request
    methodForRequestId.insert(nextRequestId, method);
    nextRequestId++;
}

void CortexClient::onMessageReceived(QString message) {
    // parse the json message
    QJsonParseError err;
    QJsonDocument doc = QJsonDocument::fromJson(message.toUtf8(), &err);
    if (err.error != QJsonParseError::NoError) {
        qCritical() << "error, failed to parse the json message: " << message;
        return;
    }

    QJsonObject response = doc.object();
    int id = response.value("id").toInt(-1);
    QString sid = response.value("sid").toString();
    QJsonObject warning = response.value("warning").toObject();

    if (id != -1) {
        qDebug().noquote() << " * received" << message;

        // this is a RPC response, we get the method from the id
        // we must know the method in order to understand the result
        QString method = methodForRequestId.value(id);
        QJsonValue result = response.value("result");
        QJsonValue error = response.value("error");

        methodForRequestId.remove(id);

        if (error.isObject()) {
            emitError(method, error.toObject());
        } else {
            handleResponse(method, result);
        }
    }
    else if (! sid.isEmpty()) {
        // this message has a sid (subscription id)
        // so this is some data from a data stream
        double time = response.value("time").toDouble();
        QJsonArray data;
        QString stream;
        //qDebug().noquote() << " * STEAM" << message;

        // find the data field inside the response
        for (auto it = response.begin(); it != response.end(); ++it) {
            QString key = it.key();
            QJsonValue value = it.value();

            if (key != "sid" && key != "time" && value.isArray()) {
                stream = key;
                data = value.toArray();
            }
        }
        emit streamDataReceived(sid, stream, time, data);
    }
    else if (! warning.isEmpty()) {
        qInfo().noquote() << " * warning " << message;
        int code = warning["code"].toInt();
        if (code == WARNING_CODE_SCAN_FINISHED)
        {
            emit sigRefreshHeadsetListFinished();
        }
    }
}

void CortexClient::handleResponse(QString method, const QJsonValue &result) {
    if (method == "queryHeadsets") {
        QList<Headset> headsets;
        for (const QJsonValue val : result.toArray()) {
            QJsonObject jheadset = val.toObject();
            Headset hs(jheadset);
            headsets.append(hs);
        }
        emit queryHeadsetsOk(headsets);
    }
    else if (method == "getUserLogin") {
        QString emotivId;
        QJsonArray users = result.toArray();
        if (! users.isEmpty()) {
            QJsonObject obj = users[0].toObject();
            QString currentOSUId = obj["currentOSUId"].toString();
            QString loggedInOSUId = obj["loggedInOSUId"].toString();
            if (currentOSUId == loggedInOSUId) {
                emotivId = obj["username"].toString();
            }
        }
        emit getUserLoginOk(emotivId);
    }
    else if (method == "requestAccess") {
        bool accessGranted = result["accessGranted"].toBool();
        QString message = result["message"].toString();
        emit requestAccessOk(accessGranted, message);
    }
    else if (method == "authorize") {
        QString token = result.toObject().value("cortexToken").toString();
        emit authorizeOk(token);
    }
    else if (method == "controlDevice") {
        emit controlDeviceOk();
    }
    else if (method == "createSession") {
        QString sessionId = result.toObject().value("id").toString();
        emit createSessionOk(sessionId);
    }
    else if (method == "updateSession") {
        QString sessionId = result.toObject().value("id").toString();
        emit closeSessionOk();
    }
    else if (method == "subscribe") {
        QStringList streams = parseSubscriptionResult(method, result.toObject());
        if (! streams.isEmpty()) {
            emit subscribeOk(streams);
        }
    }
    else if (method == "unsubscribe") {
        QStringList streams = parseSubscriptionResult(method, result.toObject());
        if (! streams.isEmpty()) {
            emit unsubscribeOk(streams);
        }
    }
    else if (method == "queryProfile") {
        QStringList profiles;
        QJsonArray array = result.toArray();
        for (auto value : array) {
            profiles.append(value.toObject().value("name").toString());
        }
        emit queryProfileOk(profiles);
    }
    else if (method == "setupProfile") {
        QJsonObject obj = result.toObject();
        QString action = obj["action"].toString();
        QString name = obj["name"].toString();
        if (action == "create") {
            emit createProfileOk(name);
        }
        else if (action == "load") {
            emit loadProfileOk(name);
        }
        else if (action == "save") {
            emit saveProfileOk(name);
        }
    }
    else if (method == "getDetectionInfo") {
        handleGetDetectionInfo(result);
    }
    else if (method == "training") {
        emit trainingOk(result.toString());
    }
    else if (method == "createRecord") {
        QJsonObject record = result.toObject().value("record").toObject();
        emit createRecordOk(record["uuid"].toString());
    }
    else if (method == "stopRecord") {
        QJsonObject record = result.toObject().value("record").toObject();
        emit stopRecordOk(record["uuid"].toString());
    }
    else if (method == "getRecordInfos") {
        QJsonObject record = result.toArray().at(0).toObject();
        emit getRecordInfosOk(record);
    }
    else if (method == "exportRecord") {
        QJsonArray failure = result.toObject().value("failure").toArray();
        QJsonArray success = result.toObject().value("success").toArray();
        if (! failure.isEmpty()) {
            emitError(method, failure[0].toObject());
        }
        else {
            emit exportRecordOk(success[0].toObject().value("recordId").toString());
        }
    }
    else if (method == "injectMarker") {
        QJsonObject marker = result.toObject().value("marker").toObject();
        QString markerId = marker["uuid"].toString();
        emit injectMarkerOk(markerId);
    }
    else if (method == "updateMarker") {
        emit updateMarkerOk();
    }
    else if (method == "getTrainedSignatureActions") {
        QList<TrainedAction> actions;
        int totalTimesTraining = result.toObject().value("totalTimesTraining").toInt();
        QJsonArray trainedActions = result.toObject().value("trainedActions").toArray();
        for (QJsonValue value : trainedActions) {
            QJsonObject obj = value.toObject();
            TrainedAction action;
            action.name = obj.value("action").toString();
            action.times = obj.value("times").toInt();
            actions.append(action);
        }
        emit getTrainedSignatureActionsOk(actions, totalTimesTraining);
    }
    else if (method == "mentalCommandActiveAction") {
        QStringList actions;
        for (QJsonValue value : result.toArray()) {
            actions.append(value.toString());
        }
        emit mentalCommandActiveActionOk(actions);
    }
    else if (method == "mentalCommandBrainMap") {
        QMap<QString, QJsonArray> coord;
        for (QJsonValue value : result.toArray()) {
            QJsonObject obj = value.toObject();
            QString action = obj.value("action").toString();
            QJsonArray coordinates = obj.value("coordinates").toArray();
            coord.insert(action, coordinates);
        }
        emit mentalCommandBrainMapOk(coord);
    }
    else if (method == "mentalCommandTrainingThreshold") {
        double currentThreshold = result.toObject().value("currentThreshold").toDouble();
        double lastTrainingScore = result.toObject().value("lastTrainingScore").toDouble();
        emit mentalCommandTrainingThresholdOk(currentThreshold, lastTrainingScore);
    }
    else if (method == "mentalCommandActionSensitivity") {
        QList<int> values;
        for (QJsonValue value : result.toArray()) {
            values.append(value.toInt());
        }
        emit mentalCommandActionSensitivityOk(values);
    }
    else {
        // unknown method, so we don't know how to interpret the result
        qCritical() << "Result from an unexpected API method:" << method << result;
    }
}

void CortexClient::handleGetDetectionInfo(const QJsonValue &result) {
    QJsonArray jactions = result.toObject().value("actions").toArray();
    QJsonArray jcontrols = result.toObject().value("controls").toArray();
    QJsonArray jevents = result.toObject().value("events").toArray();
    emit getDetectionInfoOk(arrayToStringList(jactions),
                            arrayToStringList(jcontrols),
                            arrayToStringList(jevents));
}

QStringList CortexClient::parseSubscriptionResult(QString method, const QJsonObject &result)
{
    QJsonArray success = result["success"].toArray();
    QJsonArray failure = result["failure"].toArray();
    QStringList streams;
    if (! failure.isEmpty()) {
        for (QJsonValue f : failure) {
            emitError(method, f.toObject());
        }
        return streams;
    }
    for (QJsonValue s : success) {
        QJsonObject obj = s.toObject();
        QString stream = obj["streamName"].toString();
        QJsonArray columns = obj["cols"].toArray();
        if (! columns.isEmpty()) {
            qInfo() << "stream" << stream << "uses these columns:" << columns;
        }
        streams.append(stream);
    }
    return streams;
}

void CortexClient::emitError(QString method, const QJsonObject &obj) {
    int code = obj.value("code").toInt();
    QString message = obj.value("message").toString();
    qCritical() << "The Cortex service returned an error:";
    qCritical() << "\tmethod " << method;
    qCritical() << "\tcode   " << code;
    qCritical() << "\tmessage" << message;
    emit errorReceived(method, code, message);
}
