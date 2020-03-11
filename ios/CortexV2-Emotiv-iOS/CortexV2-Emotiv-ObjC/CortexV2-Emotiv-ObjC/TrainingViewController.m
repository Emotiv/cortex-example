//
//  TrainingViewController.m
//  CortexV2-Emotiv-ObjC
//
//  Created by nvtu on 2/25/20.
//  Copyright © 2020 Emotiv. All rights reserved.
//

#import "TrainingViewController.h"
#import "Config.h"

@interface TrainingViewController ()

@end

@implementation TrainingViewController

- (void) viewDidLoad {
    [super viewDidLoad];
    detection = stream;
    actions = [[NSArray alloc] init];
    actionIndex = 0;
    actionCount = 0;
    trainingFailure = 0;

}

- (void) handleCortexEvent {
    [super handleCortexEvent];
    __unsafe_unretained typeof(self) weakSelf = self;

    client.onConnected = ^() {
        [weakSelf->client getDetectionInfo: weakSelf->detection];
    };
    
    client.onGetDetectionInfoOk = ^(NSArray *actions, NSArray *controls, NSArray *events) {
        weakSelf->actions = actions;
        NSLog(@"Information for %@: ", weakSelf->detection);
        NSLog(@"Actions %@", actions);
        NSLog(@"Controls %@", controls);
        NSLog(@"Events %@", events);
        [weakSelf->finder findHeadsets];
    };
    
    client.onQueryProfileOk = ^(NSArray *profiles) {
        if ([profiles containsObject:[Config getTrainingProfileName]]) {
            [weakSelf->client loadProfile:weakSelf->token headsetId:weakSelf->headset->headsetId profileName:[Config getTrainingProfileName]];
        } else {
            [weakSelf->client createProfile: weakSelf->token profileName: [Config getTrainingProfileName]];
        }
    };
    
    client.onCreateProfileOk = ^(NSString *profileName) {
        NSLog(@"Training profile created %@", profileName);
        [weakSelf->client loadProfile: weakSelf->token headsetId: weakSelf->headset->headsetId profileName: profileName];
    };
    
    client.onLoadProfileOk = ^(NSString *profileName) {
        NSLog(@"Training profile loaded %@", profileName);
        // we must subscribe to the "sys" stream to receive training events
        [weakSelf->client subscribe: weakSelf->token sessionId: weakSelf->sessionId stream: @"sys"];
    };
    
    client.onSaveProfileOk = ^(NSString *profileName) {
        NSLog(@"Training profile saved %@", profileName);
    };
    
    client.onSubscribeOk = ^(NSArray * streams) {
        NSLog(@"Subscription to data stream successful %@", streams);
        [weakSelf->client training: weakSelf->token sessionId: weakSelf->sessionId detection: weakSelf->detection action: [weakSelf action] control: @"start"];
    };

    client.onTrainingOk = ^(NSString *msg) {
        // this signal is not important
        // instead we need to watch the events from the sys stream
    };

    client.onStreamDataReceived = ^(NSString *sessionId, NSString *stream, double time, NSArray *data) {
        if ([weakSelf isEvent: data event: @"Started"]) {
            NSLog(@"");
            NSLog(@"Please, focus on the action %@ for a few seconds", [[weakSelf action] uppercaseString]);
        } else if ([weakSelf isEvent: data event: @"Succeeded"]) {
            [weakSelf->client training: weakSelf->token sessionId: weakSelf->sessionId detection: weakSelf->detection action: [weakSelf action] control: @"accept"];
        } else if ([weakSelf isEvent: data event: @"Failed"]) {
            [weakSelf retryAction];
        } else if ([weakSelf isEvent: data event: @"Completed"]) {
            NSLog(@"Well done! You successfully trained %@", [weakSelf action]);
            [weakSelf nextAction];
        }
    };

}
              
- (void) handleSessionEvent {
    [super handleSessionEvent];
    __unsafe_unretained typeof(self) weakSelf = self;
    
    creator.onSessionCreated = ^(NSString *token, NSString *sessionId) {
        weakSelf->token = token;
        weakSelf->sessionId = sessionId;
        // load the training profile (useful only for mental command and facial expression)
        [weakSelf->client queryProfile: token];
    };
}

- (NSString *) action {
    return actions[actionIndex];
}

- (BOOL) isEvent: (NSArray *) data event: (NSString *) event  {
    for (NSString * val in data) {
        if ([val containsString:event]) {
            return true;
        }
    }
    return false;
}

-(void) nextAction {
    NSArray *untrainableActions = [NSArray arrayWithObjects:@"blink", @"winkL", @"winkR", @"horiEye", nil];
    
    actionIndex += 1;
    trainingFailure = 0;

    // some facial expression actions cannot be trained, we must skip them
    while ([untrainableActions containsObject:[self action]]) {
        actionIndex += 1;
    }
    actionCount += 1;

    if (actionCount < 3 && actionIndex < actions.count) {
        // ok, let's train the next action
        [client training: token sessionId: sessionId detection: detection action: [self action] control: @"start"];
    }
    else {
        // that's enough training for today
        // we save the training profile before we quit
        NSLog(@"Saving training profile %@", [Config getTrainingProfileName]);
        [client saveProfile: token headsetId: headset->headsetId profileName: [Config getTrainingProfileName]];
    }
}

-(void) retryAction {
    trainingFailure += 1;

    if (trainingFailure < 3) {
        NSLog(@"Sorry, it didn't work. Let's try again.");
        [client training: token sessionId: sessionId detection: detection action: [self action] control: @"start"];
    }
    else {
        NSLog(@"It seems you are struggling with this action. Let's try another one.");
        [self nextAction];
    }
}

@end
