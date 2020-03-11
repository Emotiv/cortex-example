//
//  SessionCreator.h
//  CortexV2-Emotiv-ObjC
//
//  Created by nvtu on 2/25/20.
//  Copyright © 2020 Emotiv. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "CortexClient.h"
NS_ASSUME_NONNULL_BEGIN

@interface SessionCreator : NSObject {
    CortexClient *client;
@private
    Headset *headset;
    BOOL activate;
    NSString *license;
    NSString *token;
}
@property (nonatomic, copy) void (^onSessionCreated)(NSString*, NSString*);

-(void) createSession: (Headset *) headset activate: (BOOL) activate license: (NSString *) license;
-(void) handleCortexEvent;
@end

NS_ASSUME_NONNULL_END
