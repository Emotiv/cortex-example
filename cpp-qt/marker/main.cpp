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
#include <QCoreApplication>
#include "Marker.h"

int main(int argc, char *argv[])
{
    QCoreApplication a(argc, argv);
    Marker m;

    qInfo() << "";
    qInfo() << "#####";
    qInfo() << "Reminder: to inject markers into a session, you must get an appropriate licence from Emotiv.";
    qInfo() << "#####";
    qInfo() << "";

    // to inject markers into a session, the session must be active
    // which means that you need an appropiate license from Emotiv

    QString licenseId = ""; // you can put your license id here
    m.start(licenseId);

    return a.exec();
}
