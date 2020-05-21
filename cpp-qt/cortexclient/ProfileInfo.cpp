#include "ProfileInfo.h"

ProfileInfo::ProfileInfo(QObject *parent) : QObject(parent)
{

}

void ProfileInfo::print()
{
    qInfo() << "Mental command actions:";
    for (int i = 0; i< actions.size(); ++i) {
        TrainedAction action = actions[i];
        bool isActive = (action.name == "neutral" || activeActions.contains(action.name));
        QString coordinates;
        QString sensitivity;
        if (i == 0) {
            // neutral
            coordinates = "[0,0]";
            sensitivity = "NA";
        }
        else {
            QJsonArray coord = brainMapCoordinates.value(action.name);
            coordinates = QString("[%1:%2]").arg(coord[0].toDouble()).arg(coord[1].toDouble());
            sensitivity = QString::number(actionSensitivities[i - 1]);
        }
        qInfo() << "\t" << action.name
                << "Trained" << action.times << "times,"
                << (isActive ? "active," : "inactive,")
                << "coordinates" << coordinates
                << "sensitivity" << sensitivity;
    }
    qInfo() << "Total training times for mental command:" << totalTimesTraining;
}

void ProfileInfo::readInfo(CortexClient *client, QString token, QString profileName)
{
    this->client = client;
    this->token = token;
    this->profileName = profileName;

    connect(client, &CortexClient::getTrainedSignatureActionsOk, this, &ProfileInfo::onGetTrainedSignatureActionsOk);
    connect(client, &CortexClient::mentalCommandActiveActionOk, this, &ProfileInfo::onMentalCommandActiveActionOk);
    connect(client, &CortexClient::mentalCommandBrainMapOk, this, &ProfileInfo::onMentalCommandBrainMapOk);
    connect(client, &CortexClient::mentalCommandActionSensitivityOk, this, &ProfileInfo::onMentalCommandActionSensitivityOk);

    client->getTrainedSignatureActions(token, "mentalCommand", profileName);
}

void ProfileInfo::onGetTrainedSignatureActionsOk(QList<TrainedAction> actions, int totalTimesTraining)
{
    this->actions = actions;
    this->totalTimesTraining = totalTimesTraining;
    client->mentalCommandActiveAction(token, profileName);
}

void ProfileInfo::onMentalCommandActiveActionOk(QStringList activeActions)
{
    this->activeActions = activeActions;
    client->mentalCommandBrainMap(token, profileName);
}

void ProfileInfo::onMentalCommandBrainMapOk(QMap<QString, QJsonArray> coordinates)
{
    this->brainMapCoordinates = coordinates;
    client->mentalCommandActionSensitivity(token, profileName);
}

void ProfileInfo::onMentalCommandActionSensitivityOk(QList<int> values)
{
    this->actionSensitivities = values;
    emit done();
}
