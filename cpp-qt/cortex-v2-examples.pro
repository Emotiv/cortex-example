TEMPLATE = subdirs

SUBDIRS = \
    cortexclient\
    motion \
    eeg \
    band-power \
    mental-command \
    mental-command-training \
    facial-expression \
    facial-expression-training \
    performance-metric \
    marker

# build the project sequentially as listed in SUBDIRS !
CONFIG += ordered

OTHER_FILES += README.md
