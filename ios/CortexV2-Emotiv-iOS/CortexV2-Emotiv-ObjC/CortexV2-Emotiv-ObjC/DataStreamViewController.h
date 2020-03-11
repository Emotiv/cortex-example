//
//  DataStreamViewController.h
//  CortexV2-Emotiv-ObjC
//
//  Created by nvtu on 2/24/20.
//  Copyright © 2020 Emotiv. All rights reserved.
//

#import <UIKit/UIKit.h>
#import "CortexClient.h"
#import "SessionCreator.h"
#import "HeadsetFinder.h"

@interface DataStreamViewController : UIViewController {
    CortexClient *client;
    SessionCreator *creator;
    HeadsetFinder *finder;

    Headset *headset;
    BOOL activateSession;
    NSString *license;
    NSString *token;
    NSString *sessionId;
    NSString *stream;
}
- (id) initActivateSession: (BOOL) activateSession license: (NSString *) license stream: (NSString *) stream;
- (void) handleCortexEvent;
- (void) handleHeadsetEvent;
- (void) handleSessionEvent;
- (void) start;

@end

