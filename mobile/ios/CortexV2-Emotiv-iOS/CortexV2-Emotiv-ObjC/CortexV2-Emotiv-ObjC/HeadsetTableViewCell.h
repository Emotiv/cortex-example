//
//  HeadsetTableViewCell.h
//  CortexV2-Emotiv-ObjC
//
//  Created by Viet Anh on 3/18/20.
//  Copyright © 2020 Emotiv. All rights reserved.
//

#import <UIKit/UIKit.h>

NS_ASSUME_NONNULL_BEGIN

@interface HeadsetTableViewCell : UITableViewCell
+ (NSString *) reuseIdentifier;
@property (weak, nonatomic) IBOutlet UILabel *headsetName;
@property (weak, nonatomic) IBOutlet UILabel *connectedBy;
@property (weak, nonatomic) IBOutlet UILabel *status;
@end

NS_ASSUME_NONNULL_END
