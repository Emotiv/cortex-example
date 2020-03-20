//
//  Config.m
//  CortexV2-Emotiv-ObjC
//
//  Created by nvtu on 2/25/20.
//  Copyright © 2020 Emotiv. All rights reserved.
//

#import "Config.h"

@implementation Config

//"The client id of your Cortex app goes here"
+ (NSString *) getClientID {
    return @"";
}

//The client secret of your Cortex app goes here""
+ (NSString *) getClientSecret {
    return @"";
}

+ (NSString *) getTrainingProfileName {
    return @"cortex-v2-example";
}
@end
