//
//  DataStreamViewController.m
//  CortexV2-Emotiv-ObjC
//
//  Created by nvtu on 2/24/20.
//  Copyright © 2020 Emotiv. All rights reserved.
//

#import "DataStreamViewController.h"
#import "Config.h"

@interface DataStreamViewController ()

@end

@implementation DataStreamViewController

- (id) initActivateSession: (BOOL) activateSession license: (NSString *) license stream: (NSString *) stream {
    self = [super init];
    if (self) {
        self->activateSession = activateSession;
        self->license = license;
        self->stream = stream;
    }
    return self;
}

- (void) viewDidLoad {
    [super viewDidLoad];
    client = [CortexClient shareInstance];
    [self handleCortexEvent];
    [self handleHeadsetEvent];
    [self handleSessionEvent];
    [self start];
    // Do any additional setup after loading the view.
}

- (void) start {
    [client open];
}

- (void) handleCortexEvent {
    __unsafe_unretained typeof(self) weakSelf = self;

    client.onConnected = ^(){
        [weakSelf->finder findHeadsets];
    };
    
    client.onDisconnected = ^(){
        NSLog(@"Disconnected.");
    };
    
    client.onErrorReceived = ^(NSString *method, int code, NSString *message) {
        if ([method  isEqual: @"setupProfile"]) {
             // it's fine, we can subscribe to a data stream even without a profile
             NSLog(@"Failed to load the training profile.");
         }
    };
    
    client.onLoadProfileOk = ^(NSString *profileName) {
        NSLog(@"Training profile loaded \(profileName)");
    };
    
    client.onSubscribeOk = ^(NSArray * streams) {
        NSLog(@"Subscription successful for data streams %@", streams);
        NSLog(@"Receiving data for 30 seconds.");
        dispatch_async(dispatch_get_main_queue(), ^{
            [NSTimer scheduledTimerWithTimeInterval: 30.0 repeats: false block:^(NSTimer * timer) {
                [weakSelf->client unsubscribe: weakSelf->token sessionId: weakSelf->sessionId stream: weakSelf->stream];
            }];
        });
    };

    client.onStreamDataReceived = ^(NSString *sessionId, NSString *stream, double time, NSArray *data) {
        // a data stream can publish data with a high frequency
        // we display only a few samples per second
        NSLog(@"\(%@) \(%@)", stream, data);
    };
    
    client.onUnsubscribeOk = ^(NSArray * streams) {
        [weakSelf->client closeSession: weakSelf->token sessionId: weakSelf->sessionId];
    };
    
    client.onCloseSessionOk = ^(){
        NSLog(@"Session closed.");
        [weakSelf->client close];
    };
}

- (void) handleHeadsetEvent {
    __unsafe_unretained typeof(self) weakSelf = self;
    self->finder = [[HeadsetFinder alloc] init];

    finder.onHeadsetFound = ^(Headset *headset) {
        weakSelf->headset = headset;
        // next step: create a session for this headset
        [weakSelf->creator createSession: headset activate: weakSelf->activateSession license: weakSelf->license];
    };
}

- (void) handleSessionEvent {
    __unsafe_unretained typeof(self) weakSelf = self;
    self->creator = [[SessionCreator alloc] init];

    creator.onSessionCreated = ^(NSString *token, NSString *sessionId) {
        weakSelf->token = token;
        weakSelf->sessionId = sessionId;
        // load the training profile (useful only for mental command and facial expression)
        [weakSelf->client loadProfile: token headsetId: weakSelf->headset->headsetId profileName: [Config getTrainingProfileName]];
        // next step: subscribe to a data stream
        [weakSelf->client subscribe: token sessionId: sessionId stream: weakSelf->stream];
    };
}

@end
