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
#ifndef CONFIG_H
#define CONFIG_H
#include <QString>
#include <QJsonObject>

/*
 * To get a client id and a client secret, you must connect to your Emotiv
 * account on emotiv.com and create a Cortex app.
 * https://www.emotiv.com/my-account/cortex-apps/
 */

//=== Dev server
// acc test_sync2
static const QString ClientId     = "mHi4rhpLHNx7raXny1fa1v2SDx0w0wL5G47XqxHJ";
static const QString ClientSecret = "rDSLvEB3l1325iUcNCg2UUaRg0oJlpH8XhOznNeMlkzLfXLfeh6gp5bzk355wxWU9mzzo8E05LHKtlggFaVq3PnT4IflHnEKsbhORggbqhsmnfvWDg29CMCUfLNzmTey";

//=== Product server
// acc tpv
//static const QString ClientId     = "rfkIx2SaXVfMmfnrBo5tvEuv2OPnOnqiXjFYsDak";
//static const QString ClientSecret = "U1qTmkCP0AU5HOo5YNTiGVFlZtpPF9MZyWknDj1zU3SmKsXUKaMGmgB0YKKBFiKKEeZqolITvPb22uEVakv9ajWnZMSNxcNVnByumL2PkRSdouoH3hlrzrDjZObpJlJK";

// acc hoangpt89
//static const QString ClientId     = "PTftRnfSUdrO2We2lApb61IX9SRlKZumG1mdAJ7C";
//static const QString ClientSecret = "jdkUbeGUT1c5SN5bCWlhQ1RV6zvxDSjznz3HZrkGcKeFfdgEvqVpd2cmBUWSdxPh4lUAOla0D5Koz7HQAqi4aMszOcrPGvf9kOpAiqjM2hUqUQJpz58uywwY3ckamuOq";

// The name of the training profile used for the facial expression and mental command
static const QString TrainingProfileName = "cortex-v2-example";

// If you use an Epoc Flex headset, then you must put your configuration here
static const QJsonObject FlexMapping = {
    { "LA",  "AF3" },
    { "LB",  "AF7" },
    { "CMS", "TP8" },
    { "DRL", "P6" }
    // etc...
};

//static QString XtrodesMontage = "EEG2";
//static QString XtrodesMontage = "EEG12";
static QString XtrodesMontage = "EEG16";

// With new Cortex, don't need to send a mapping from apps
static const QJsonObject XtrodesMapping = {
    {"Ch-1", "EEG"},
    {"Ch-2", "EEG"},
    {"Ch-3", "EEG"},
    {"Ch-4", "EEG"},
    {"Ch-5", "EEG"},
    {"Ch-6", "EEG"},
    {"Ch-7", "EEG"},
//    {"Ch-8", "EEG"},
//    {"Ch-9", "EEG"},
//    {"Ch-10", "EEG"},
//    {"Ch-11", "EEG"},
//    {"Ch-12", "EEG"},
//    {"Ch-13", "EEG"},
//    {"Ch-14", "EEG"},
//    {"Ch-15", "EEG"},
//    {"Ch-16", "EEG"},
};

#endif // CONFIG_H
