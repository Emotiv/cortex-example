//
//  HeadsetTableViewCell.m
//  CortexV2-Emotiv-ObjC
//
//  Created by Viet Anh on 3/18/20.
//  Copyright © 2020 Emotiv. All rights reserved.
//

#import "HeadsetTableViewCell.h"

@implementation HeadsetTableViewCell

- (void)awakeFromNib {
    [super awakeFromNib];
    // Initialization code
}

- (void)setSelected:(BOOL)selected animated:(BOOL)animated {
    [super setSelected:selected animated:animated];

    // Configure the view for the selected state
}

+ (NSString *)reuseIdentifier {
    return @"CustomCellIdentifier";
}


@end
