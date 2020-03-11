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
    return @"DAN3SDXZCtLny4ypSzchktKzJKIOqPu6gKUS3syd";
}

//The client secret of your Cortex app goes here""
+ (NSString *) getClientSecret {
    return @"FKE7SHyVCpuVFRrMvPeOq0uTqMlPLJWgySgUk9uDnu1pCeQFAsOgi4dQvrmRcidnNuw8Fc53XF5PwZZFst3VVB0UGbaeaV8gnZN75U3g8VEvFzIszlN10sApWwZbvaSg";
}

+ (NSString *) getTrainingProfileName {
    return @"cortex-v2-example";
}
@end
