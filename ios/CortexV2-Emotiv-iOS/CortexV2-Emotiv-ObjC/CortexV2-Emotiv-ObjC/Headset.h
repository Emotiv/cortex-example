//
//  Headset.h
//  CortexV2-Emotiv-ObjC
//
//  Created by nvtu on 2/24/20.
//  Copyright © 2020 Emotiv. All rights reserved.
//

#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

@interface Headset : NSObject {
@public
    NSString *headsetId;
    NSString *label;
    NSString *connectedBy;
    NSString *status;
}

- (id) init;
- (id) initJson : (NSDictionary *) jHeadset;
- (NSString *) toString;
@end

NS_ASSUME_NONNULL_END
