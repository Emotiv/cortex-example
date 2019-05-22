#ifndef CONFIG_H
#define CONFIG_H
#include <QString>
#include <QJsonObject>

/*
 * To get a client id and a client secret, you must connect to your Emotiv
 * account on emotiv.com and create a Cortex app.
 * https://www.emotiv.com/my-account/cortex-apps/
 */
static const QString ClientId = "The client id of your Cortex app goes here";
static const QString ClientSecret = "The client secret of your Cortex app goes here";

// The name of the training profile used for the facial expression and mental command
static const QString TrainingProfileName = "cortex-v2-example";

// If you use an Epoc Flex headset, then you must put your configuration here
static const QJsonObject FlexMapping = {
    { "LA", "AF3" },
    { "LB", "AF7" }
    // etc...
};

#endif // CONFIG_H
