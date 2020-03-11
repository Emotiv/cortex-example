//
//  MarkerViewController.h
//  CortexV2-Emotiv-ObjC
//
//  Created by nvtu on 2/25/20.
//  Copyright © 2020 Emotiv. All rights reserved.
//

#import <UIKit/UIKit.h>
#import "DataStreamViewController.h"

NS_ASSUME_NONNULL_BEGIN

@interface MarkerViewController : DataStreamViewController {
@private
    NSString *recordId;
    NSString *markerId;
}
-(void) stopRecord;
  
-(void) injectMarker1;
  
-(void) injectMarker2;
  
-(void) injectStopMarker2;
@end

NS_ASSUME_NONNULL_END
