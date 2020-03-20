//
//  HeadsetFinder.h
//  CortexV2-Emotiv-ObjC
//
//  Created by nvtu on 2/24/20.
//  Copyright © 2020 Emotiv. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "CortexClient.h"

NS_ASSUME_NONNULL_BEGIN

@interface HeadsetFinder : NSObject {
    CortexClient *client;
    NSTimer *timer;
}
@property (nonatomic, copy) void (^onHeadsetFound)(Headset*,NSArray*);

-(void) handleCortexEvent;
-(void) findHeadsets;
@end

NS_ASSUME_NONNULL_END
