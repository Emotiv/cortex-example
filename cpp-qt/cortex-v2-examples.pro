TEMPLATE = subdirs

SUBDIRS = \
    cortexclient\
    facialexpressions \
    motion \
    eeg \
    training \
    mentalcommand \
    marker

# build the project sequentially as listed in SUBDIRS !
CONFIG += ordered

OTHER_FILES += README.md
