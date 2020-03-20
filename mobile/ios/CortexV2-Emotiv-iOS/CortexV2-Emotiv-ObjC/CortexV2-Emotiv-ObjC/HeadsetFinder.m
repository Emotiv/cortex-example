//
//  HeadsetFinder.m
//  CortexV2-Emotiv-ObjC
//
//  Created by nvtu on 2/24/20.
//  Copyright © 2020 Emotiv. All rights reserved.
//

#import "HeadsetFinder.h"

@implementation HeadsetFinder

- (id) init {
    self = [super init];
    if (self) {
        client = [CortexClient shareInstance];
        [self handleCortexEvent];
    }
    return self;
}

-(void) handleCortexEvent {
    __unsafe_unretained typeof(self) weakSelf = self;
    client.onQueryHeadsetsOk = ^(NSArray *headsets) {
        if (headsets.count == 0) {
            return;
        }
        [weakSelf printHeadset:headsets];
        Headset *headset = headsets.firstObject;
        NSLog(@"headset status %@", headset->status);
        if ([headset->status isEqual: @"connected"]) {
            if (weakSelf.onHeadsetFound != nil) {
                [weakSelf->timer invalidate];
                weakSelf.onHeadsetFound(headset, headsets);
            }
        }
    };
}

-(void) findHeadsets {
    if (timer != nil) {
        [timer invalidate];
    }
    dispatch_async(dispatch_get_main_queue(), ^{
        self->timer = [NSTimer scheduledTimerWithTimeInterval:1 repeats:true block:^(NSTimer * timer) {
            [self->client queryHeadset:@""];
        }];
    });
}

-(void) printHeadset : (NSArray *) headsets {
    NSLog(@"%lu headset(s) found:", (unsigned long)headsets.count);
    for(Headset *hs in headsets) {
        NSLog(@"%@", [hs toString]);
    }
}
@end
