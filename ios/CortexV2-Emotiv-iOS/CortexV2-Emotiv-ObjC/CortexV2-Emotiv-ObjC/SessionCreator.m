//
//  SessionCreator.m
//  CortexV2-Emotiv-ObjC
//
//  Created by nvtu on 2/25/20.
//  Copyright © 2020 Emotiv. All rights reserved.
//

#import "SessionCreator.h"
#import "Config.h"

@implementation SessionCreator
@synthesize onSessionCreated;

-(void) createSession: (Headset *) headset activate: (BOOL) activate license: (NSString *) license {
    client = [CortexClient shareInstance];
    self->headset = headset;
    self->activate = activate;
    self->license = license;
    [self handleCortexEvent];
    [client getUserLogin];
}

-(void) handleCortexEvent {
    __unsafe_unretained typeof(self) weakSelf = self;

    client.onGetUserLoginOk = ^(NSString * emotivId) {
        if ([emotivId isEqual: @""]) {
            NSLog(@"First, you must login with EMOTIV App");
            return;
        }
        NSLog(@"You are logged in with the EmotivId %@", emotivId);
        [weakSelf->client requestAccess:[Config getClientID] secret:[Config getClientSecret]];
    };
    
    client.onRequestAccessOk = ^(BOOL accessGranted, NSString *message) {
        if (accessGranted) {
            NSLog(@"This application was authorized in EMOTIV App");
            int debit = weakSelf->activate ? 1 : 0;
            [weakSelf->client authorize:[Config getClientID] secret:[Config getClientSecret] license:weakSelf->license debit:debit];
            return;
        } else {
            NSLog(@"%@", message);
            dispatch_async(dispatch_get_main_queue(), ^{
                [NSTimer scheduledTimerWithTimeInterval:2 repeats:false block:^(NSTimer * timer) {
                    [weakSelf->client requestAccess:[Config getClientID] secret:[Config getClientSecret]];
                }];
            });
        }
    };
    
    client.onAuthorizeOk = ^(NSString *token) {
        weakSelf->token = token;
        NSLog(@"Authorize successful, token \(token)");
        // next step: open a session for the headset
        [weakSelf->client createSession: token headsetId: weakSelf->headset->headsetId activate: weakSelf->activate];
    };
    
    client.onCreateSessionOk = ^(NSString *sessionId) {
        NSLog(@"Session created, session id \(sessionId)");
        // next step: open a session for the headset
        if (weakSelf->onSessionCreated != nil){
            weakSelf->onSessionCreated(weakSelf->token, sessionId);
        }
    };
}

@end
