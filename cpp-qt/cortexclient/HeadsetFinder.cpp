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
#include "HeadsetFinder.h"

HeadsetFinder::HeadsetFinder(QObject *parent) : QObject(parent) {
    client = nullptr;
    timerId = 0;
}

void HeadsetFinder::clear() {
    if (client != nullptr) {
        disconnect(client, nullptr, this, nullptr);
        client = nullptr;
    }
    timerId = 0;
}

void HeadsetFinder::printHeadsets(const QList<Headset> &headsets)
{
    qInfo() << headsets.size() << "headset(s) found:";
    for (Headset hs : headsets) {
        qInfo() << "\t" << hs.toString();
    }
}

void HeadsetFinder::findHeadsets(CortexClient* client) {
    this->client = client;
    connect(client, &CortexClient::queryHeadsetsOk, this, &HeadsetFinder::onQueryHeadsetsOk);
    timerId = startTimer(1000);
}

void HeadsetFinder::timerEvent(QTimerEvent *event) {
    if (event->timerId() == timerId) {
        qInfo() << "Looking for headsets...";
        client->queryHeadsets();
    }
}

void HeadsetFinder::onQueryHeadsetsOk(const QList<Headset> &headsets) {
    if (headsets.isEmpty()) {
        return;
    }
    printHeadsets(headsets);

    // in a real world application, we should ask the user to select a headset from the list
    // but here, we just use the first headset
    Headset headset = headsets.first();

    if (headset.status == "discovered") {
        // we must connect this headset before we can use it
        QJsonObject flexMapping;
        if (headset.id.contains("flex", Qt::CaseInsensitive)) {
            // TODO put your Epoc Flex configuration here
            // flexMapping["LA"] = "AF3";
            // flexMapping["LB"] = "AF7";
            // etc...
        }
        client->controlDevice(headset.id, "connect", flexMapping);
    }
    else if (headset.status == "connecting") {
        qInfo() << "Waiting for headset connection" << headset.toString();
    }
    else if (headset.status == "connected") {
        killTimer(timerId);
        emit headsetFound(headset);
    }
}
