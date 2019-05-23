TEMPLATE = subdirs

SUBDIRS = \
    cortexclient\
    motion \
    eeg \
    mental-command \
    mental-command-training \
    facial-expression \
    facial-expression-training \
    marker

# build the project sequentially as listed in SUBDIRS !
CONFIG += ordered

OTHER_FILES += README.md
