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
#include "Training.h"
#include "Config.h"
#include <QCoreApplication>
#include <QJsonArray>
#include <QtDebug>


Training::Training(QObject *parent) : QObject(parent) {
    connect(&client, &CortexClient::connected, this, &Training::onConnected);
    connect(&client, &CortexClient::disconnected, this, &Training::onDisconnected);
    connect(&client, &CortexClient::errorReceived, this, &Training::onErrorReceived);
    connect(&client, &CortexClient::getDetectionInfoOk, this, &Training::onGetDetectionInfoOk);
    connect(&client, &CortexClient::subscribeOk, this, &Training::onSubscribeOk);
    connect(&client, &CortexClient::trainingOk, this, &Training::onTrainingOk);
    connect(&client, &CortexClient::streamDataReceived, this, &Training::onStreamDataReceived);
    connect(&client, &CortexClient::queryProfileOk, this, &Training::onQueryProfileOk);
    connect(&client, &CortexClient::createProfileOk, this, &Training::onCreateProfileOk);
    connect(&client, &CortexClient::loadProfileOk, this, &Training::onLoadProfileOk);
    connect(&client, &CortexClient::saveProfileOk, this, &Training::onSaveProfileOk);
    connect(&finder, &HeadsetFinder::headsetFound, this, &Training::onHeadsetFound);
    connect(&creator, &SessionCreator::sessionCreated, this, &Training::onSessionCreated);
    connect(&profileInfo, &ProfileInfo::done, this, &Training::onProfileInfoDone);
}

void Training::start(QString detection) {
    this->detection = detection;
    actionIndex = 0;
    actionCount = 0;
    trainingFailure = 0;
    client.open();
}

void Training::onConnected() {
    qInfo() << "Connected to Cortex.";
    client.getDetectionInfo(detection);
}

void Training::onDisconnected() {
    qInfo() << "Disconnected.";
    QCoreApplication::quit();
}

void Training::onErrorReceived() {
    QCoreApplication::quit();
}

void Training::onGetDetectionInfoOk(QStringList actions,
                                    QStringList controls,
                                    QStringList events) {
    this->actions = actions;
    qInfo() << "Information for" << detection << ":";
    qInfo() << "Actions " << actions;
    qInfo() << "Controls" << controls;
    qInfo() << "Events  " << events;
    finder.findHeadsets(&client);
}

void Training::onHeadsetFound(const Headset &headset) {
    finder.clear();
    this->headset = headset;
    creator.createSession(&client, headset, false, "");
}

void Training::onSessionCreated(QString token, QString sessionId) {
    this->token = token;
    this->sessionId = sessionId;
    creator.clear();
    // list the training profiles
    client.queryProfile(token);
}

void Training::onQueryProfileOk(QStringList profiles)
{
    if (profiles.contains(TrainingProfileName)) {
        // the profile already exists, we can load it
        client.loadProfile(token, headset.id, TrainingProfileName);
    }
    else {
        // the profile doesn't exist, we must create it first
        qInfo() << "Creating new training profile" << TrainingProfileName;
        client.createProfile(token, headset.id, TrainingProfileName);
    }
}

void Training::onCreateProfileOk(QString profileName)
{
    qInfo() << "Training profile created" << profileName;
    client.loadProfile(token, headset.id, profileName);
}

void Training::onLoadProfileOk(QString profileName)
{
    qInfo() << "Training profile loaded" << profileName;
    // we must subscribe to the "sys" stream to receive training events
    client.subscribe(token, sessionId, "sys");
}

void Training::onSaveProfileOk(QString profileName)
{
    qInfo() << "Training profile saved" << profileName;
    profileInfo.readInfo(&client, token, profileName);
}

void Training::onProfileInfoDone()
{
    profileInfo.print();
    QCoreApplication::quit();
}

void Training::onSubscribeOk(QStringList streams) {
    qInfo() << "Subscription to data stream successful" << streams;
    client.training(token, sessionId, detection, action(), "start");
}

void Training::onTrainingOk(QString msg) {
    Q_UNUSED(msg);
    // this signal is not important
    // instead we need to watch the events from the sys stream
}

void Training::onStreamDataReceived(QString sessionId, QString stream,
                                    double time, const QJsonArray &data) {
    Q_UNUSED(sessionId);
    Q_UNUSED(stream);
    Q_UNUSED(time);
    //qDebug() << " * sys data:" << data;

    if (isEvent(data, "Started")) {
        qInfo() << "";
        qInfo() << "Please, focus on the action" << action().toUpper()
                << "for a few seconds.";
    }
    else if (isEvent(data, "Succeeded")) {
        // the training of this action is a success
        // we "accept" it, and then we will receive the "Completed" event
        client.training(token, sessionId, detection, action(), "accept");
    }
    else if (isEvent(data, "Failed")) {
        retryAction();
    }
    else if (isEvent(data, "Completed")) {
        qInfo() << "Well done! You successfully trained " << action();
        nextAction();
    }
}

void Training::nextAction() {
    static const QStringList untrainableActions = {
        "blink", "winkL", "winkR", "horiEye"
    };
    actionIndex++;
    trainingFailure = 0;

    // some facial expression actions cannot be trained, we must skip them
    while (untrainableActions.contains(action())) {
        actionIndex++;
    }
    actionCount++;

    if (actionCount < 3 && actionIndex < actions.size()) {
        // ok, let's train the next action
        client.training(token, sessionId, detection, action(), "start");
    }
    else {
        // that's enough training for today
        // we save the training profile before we quit
        qInfo() << "Saving training profile" << TrainingProfileName;
        client.saveProfile(token, headset.id, TrainingProfileName);
    }
}

void Training::retryAction() {
    trainingFailure++;

    if (trainingFailure < 3) {
        qInfo() << "Sorry, it didn't work. Let's try again.";
        client.training(token, sessionId, detection, action(), "start");
    }
    else {
        qInfo() << "It seems you are struggling with this action. Let's try another one.";
        nextAction();
    }
}

bool Training::isEvent(const QJsonArray &data, QString event) {
    for (const QJsonValue &val : data) {
        QString str = val.toString();
        if (str.endsWith(event)) {
            return true;
        }
    }
    return false;
}
