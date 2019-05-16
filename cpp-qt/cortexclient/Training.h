#ifndef TRAINING_H
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
#define TRAINING_H

#include <QObject>
#include <QList>
#include "CortexClient.h"
#include "HeadsetFinder.h"
#include "SessionCreator.h"

/*
 * Training for the mental command or the facial expressions.
 *
 */
class Training : public QObject
{
    Q_OBJECT

public:
    explicit Training(QObject *parent = nullptr);

    // detection must be "mentalCommand" or "facialExpression"
    void start(QString detection);

private slots:
    void onConnected();
    void onDisconnected();
    void onErrorReceived();

    void onGetDetectionInfoOk(QStringList actions,
                              QStringList controls,
                              QStringList events);

    void onHeadsetFound(const Headset &headset);
    void onSessionCreated(QString token, QString sessionId);
    void onSubscribeOk(QStringList streams);
    void onTrainingOk(QString msg);
    void onStreamDataReceived(QString sessionId, QString stream,
                              double time, const QJsonArray &data);

private:
    QString action() {
        return actions.at(actionIndex);
    }
    void nextAction();
    void retryAction();
    bool isEvent(const QJsonArray &data, QString event);

private:
    CortexClient client;
    HeadsetFinder finder;
    SessionCreator creator;

    QString detection;
    QStringList actions;

    QString token;
    QString sessionId;
    int actionIndex;
    int actionCount;
    int trainingFailure;
};

#endif // TRAINING_H
