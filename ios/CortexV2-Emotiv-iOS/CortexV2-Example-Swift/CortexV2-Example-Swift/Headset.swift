//
//  Headset.swift
//  CortexV2-Example-Swift
//
//  Created by Emotiv Inc on 2/21/20.
//  Copyright © 2020 Emotiv. All rights reserved.
//

import Foundation

class Headset {
    var id: String = ""
    var label: String = ""
    var connectedBy: String = ""
    var status: String = ""
    
    init() {
        
    }
    
    init(jHeadset: [String: Any]) {
        fromJson(jheadset: jHeadset)
    }
    
    func fromJson(jheadset: [String: Any]) {
        id = jheadset["id"] as? String ?? ""
        label = jheadset["label"] as? String ?? ""
        connectedBy = jheadset["connectedBy"] as? String ?? ""
        status = jheadset["status"] as? String ?? ""
    }
    
    func toString() -> String {
        return "\(id) \(status) \(connectedBy)"
    }
}
