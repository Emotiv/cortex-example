//
//  Config.h
//  CortexV2-Emotiv-ObjC
//
//  Created by nvtu on 2/25/20.
//  Copyright © 2020 Emotiv. All rights reserved.
//

#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

@interface Config : NSObject
+ (NSString *) getClientID;
+ (NSString *) getClientSecret;
+ (NSString *) getLicense;
+ (NSString *) getTrainingProfileName;
@end

NS_ASSUME_NONNULL_END
