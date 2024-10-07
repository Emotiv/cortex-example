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
#include "SessionCreator.h"
#include "Config.h"
#include <QCoreApplication>
#include <QTimer>
#include <QJsonObject>


SessionCreator::SessionCreator(QObject *parent) : QObject(parent) {
    client = nullptr;
    activate = false;
}

void SessionCreator::clear() {
    if (client) {
        disconnect(client, nullptr, this, nullptr);
        client = nullptr;
    }
}

void SessionCreator::createSession(CortexClient* client,
                                   Headset headset,
                                   bool activate,
                                   QString license) {
    this->client = client;
    this->headset = headset;
    this->activate = activate;
    this->license = license;

    connect(client, &CortexClient::getUserLoginOk, this, &SessionCreator::onGetUserLoginOk);
    connect(client, &CortexClient::requestAccessOk, this, &SessionCreator::onRequestAccessOk);
    connect(client, &CortexClient::authorizeOk, this, &SessionCreator::onAuthorizeOk);
    connect(client, &CortexClient::createSessionOk, this, &SessionCreator::onCreateSessionOk);

    // first step: get the current user
    client->getUserLogin();
}

void SessionCreator::onGetUserLoginOk(QString emotivId) {
    if (emotivId.isEmpty()) {
        qInfo() << "First, you must login with EMOTIV Launcher";
        QCoreApplication::quit();
        return;
    }
    qInfo() << "You are logged in with the EmotivId" << emotivId;
    client->requestAccess(ClientId, ClientSecret);
}

void SessionCreator::onRequestAccessOk(bool accessGranted, QString message) {
    if (accessGranted) {
        qInfo() << "This application was authorized in EMOTIV Launcher";
        // if we want to activate the session, then we need to debit the license
        int debit = activate ? 1 : 0;
        client->authorize(ClientId, ClientSecret, license, debit);
    }
    else {
        qInfo() << message;
        // let's try again later
        QTimer::singleShot(2*1000, [this]() {
            client->requestAccess(ClientId, ClientSecret);
        });
    }
}

void SessionCreator::onAuthorizeOk(QString token) {
    this->token = token;
    qInfo() << "Authorize successful, token" << token;
    // next step: open a session for the headset
    client->createSession(token, headset.id, activate);
}

void SessionCreator::onCreateSessionOk(QString sessionId) {
    qInfo() << "Session created, session id " << sessionId;
    emit sessionCreated(token, sessionId);
}
