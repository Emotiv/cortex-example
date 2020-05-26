#ifndef PROFILEINFO_H
#define PROFILEINFO_H

#include <QObject>
#include "CortexClient.h"


/*
 * Read some advanced BCI information from a training profile.
 *
 */
class ProfileInfo : public QObject
{
    Q_OBJECT

public:
    explicit ProfileInfo(QObject *parent = nullptr);

    void print();

    void readInfo(CortexClient* client, QString token, QString profileName);

public slots:
    void onGetTrainedSignatureActionsOk(QList<TrainedAction> actions, int totalTimesTraining);
    void onMentalCommandActiveActionOk(QStringList activeActions);
    void onMentalCommandBrainMapOk(QMap<QString, QJsonArray> coordinates);
    void onMentalCommandActionSensitivityOk(QList<int> values);

signals:
    void done();

private:
    CortexClient* client = nullptr;
    QString token;
    QString profileName;

    QList<TrainedAction> actions;
    int totalTimesTraining = 0;
    QStringList activeActions;
    QMap<QString, QJsonArray> brainMapCoordinates;
    QList<int> actionSensitivities;
};

#endif // PROFILEINFO_H
