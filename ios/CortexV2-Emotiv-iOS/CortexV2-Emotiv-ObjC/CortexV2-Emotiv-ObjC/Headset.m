//
//  Headset.m
//  CortexV2-Emotiv-ObjC
//
//  Created by nvtu on 2/24/20.
//  Copyright © 2020 Emotiv. All rights reserved.
//

#import "Headset.h"

@implementation Headset

- (id) init {
    self = [super init];
    if (self) {
        headsetId = @"";
        label = @"";
        connectedBy = @"";
        status = @"";
    }
    return self;
}

- (id) initJson : (NSDictionary *) jHeadset {
    self = [super init];
    if (self) {
        headsetId = [jHeadset objectForKey:@"id"];
        label = [jHeadset objectForKey:@"label"];
        connectedBy = [jHeadset objectForKey:@"connectedBy"];
        status = [jHeadset objectForKey:@"status"];
    }
    return self;
}

-(NSString *) toString {
    return [NSString stringWithFormat:@"%@ %@ %@", headsetId, status, connectedBy];
}
@end
