//
//  HeadsetTableViewCell.swift
//  CortexV2-Example-Swift
//
//  Created by Viet Anh on 3/16/20.
//  Copyright © 2020 Emotiv. All rights reserved.
//

import UIKit

class HeadsetTableViewCell: UITableViewCell {
    @IBOutlet weak var headsetName: UILabel!
    @IBOutlet weak var connectedBy: UILabel!
    @IBOutlet weak var status: UILabel!
    
    override func awakeFromNib() {
        super.awakeFromNib()
        // Initialization code
    }

    override func setSelected(_ selected: Bool, animated: Bool) {
        super.setSelected(selected, animated: animated)

        // Configure the view for the selected state
    }
    
}
