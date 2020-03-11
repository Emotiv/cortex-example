//
//  MarkerViewController.m
//  CortexV2-Emotiv-ObjC
//
//  Created by nvtu on 2/25/20.
//  Copyright © 2020 Emotiv. All rights reserved.
//

#import "MarkerViewController.h"

@interface MarkerViewController ()

@end

@implementation MarkerViewController

- (void)viewDidLoad {
    [super viewDidLoad];
    // Do any additional setup after loading the view.
}

- (void)handleCortexEvent {
    [super handleCortexEvent];

    __unsafe_unretained typeof(self) weakSelf = self;

    client.onCreateRecordOk = ^(NSString *recordId){
        weakSelf->recordId = recordId;
        
        // after a few seconds, inject some markers
        dispatch_async(dispatch_get_main_queue(), ^{
            [NSTimer scheduledTimerWithTimeInterval: 5.0 repeats: false block:^(NSTimer * timer) {
                [weakSelf injectMarker1];
            }];
        });
        
        dispatch_async(dispatch_get_main_queue(), ^{
            [NSTimer scheduledTimerWithTimeInterval: 13.0 repeats: false block:^(NSTimer * timer) {
                [weakSelf injectMarker2];
            }];
        });
        
        dispatch_async(dispatch_get_main_queue(), ^{
            [NSTimer scheduledTimerWithTimeInterval: 21.0 repeats: false block:^(NSTimer * timer) {
                [weakSelf injectStopMarker2];
            }];
        });
        
        // close the session after 30 seconds
        dispatch_async(dispatch_get_main_queue(), ^{
            [NSTimer scheduledTimerWithTimeInterval: 30.0 repeats: false block:^(NSTimer * timer) {
                [weakSelf stopRecord];
            }];
        });
    };
    
    client.onInjectMarkerOk = ^(NSString *markerId) {
        NSLog(@"Inject marker OK, marker id %@", (markerId));
        weakSelf->markerId = markerId;
    };
    
    client.onUpdateMarkerOk = ^() {
        NSLog(@"Update marker OK");
    };
    
    client.onStopRecordOk = ^(NSString *recordId) {
        [weakSelf->client getRecordInfos:weakSelf->token recordId:weakSelf->recordId];
    };
}

- (void)handleSessionEvent {
    __unsafe_unretained typeof(self) weakSelf = self;
    [super handleSessionEvent];
    creator.onSessionCreated = ^(NSString *token, NSString *sessionId) {
        weakSelf->token = token;
        weakSelf->sessionId = sessionId;
        [weakSelf->client createRecord: token sessionId: sessionId title: @"Cortex Examples Swift"];
    };
}

-(void) stopRecord {
    NSLog(@"Stopping the record");
    [client stopRecord: token sessionId: sessionId];
}
  
-(void) injectMarker1 {
    NSLog(@"Inject marker test1");
    [client injectMarker: token sessionId: sessionId label: @"test1" value: 41 time: [[NSDate now] timeIntervalSince1970] * 1000];
}
  
-(void) injectMarker2 {
    NSLog(@"Inject marker test2");
    [client injectMarker: token sessionId: sessionId label: @"test2" value: 41 time: [[NSDate now] timeIntervalSince1970] * 1000];
}
  
-(void) injectStopMarker2 {
    NSLog(@"Update marker for test2, marker id \(markerId)");
    [client updateMarker: token sessionId: sessionId markerId: markerId time: [[NSDate now] timeIntervalSince1970] * 1000];
}
@end
