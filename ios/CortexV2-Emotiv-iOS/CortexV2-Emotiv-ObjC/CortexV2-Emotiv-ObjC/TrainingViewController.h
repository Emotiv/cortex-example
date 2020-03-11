//
//  TrainingViewController.h
//  CortexV2-Emotiv-ObjC
//
//  Created by nvtu on 2/25/20.
//  Copyright © 2020 Emotiv. All rights reserved.
//

#import <UIKit/UIKit.h>
#import "DataStreamViewController.h"

NS_ASSUME_NONNULL_BEGIN

@interface TrainingViewController : DataStreamViewController {
    NSArray *actions;
    int actionIndex;
    int actionCount;
    int trainingFailure;
    NSString *detection;
}
@end

NS_ASSUME_NONNULL_END
