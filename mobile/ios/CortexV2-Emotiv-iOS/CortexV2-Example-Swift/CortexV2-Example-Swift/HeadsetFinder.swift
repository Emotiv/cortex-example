//
//  HeadsetFinder.swift
//  CortexV2-Example-Swift
//
//  Created by Emotiv Inc on 2/21/20.
//  Copyright © 2020 Emotiv. All rights reserved.
//

import Foundation

class HeadsetFinder {
    
    private var client = CortexClient.sharedInstance
    private var timer: Timer!
    
    var onHeadsetFound: ((Headset, [Headset]) -> Void)!
    
    init() {
        handleCortexEvent()
    }
    
    func findHeadsets() {
        if timer != nil {
            timer.invalidate()
        }
        timer = Timer.scheduledTimer(withTimeInterval: 1, repeats: true, block: { [weak self] (timer) in
            guard let weakSelf = self else { return }
            weakSelf.client.queryHeadsets()
        })
    }
    
    private func handleCortexEvent() {
        client.onQueryHeadsetsOk = { [weak self] (headsets) in
            guard let weakSelf = self else { return }
            if headsets.isEmpty {
                return
            }
            weakSelf.printHeadsets(headsets: headsets)
            let headset = headsets.first ?? Headset()
            NSLog("headset status \(headset.status)")
            if headset.status == "connected" {
                if weakSelf.onHeadsetFound != nil {
                    weakSelf.timer.invalidate()
                    weakSelf.onHeadsetFound!(headset, headsets)
                }
            }
        }
    }
    
    private func printHeadsets(headsets: [Headset])
    {
        NSLog("\(headsets.count) headset(s) found:")
        for hs in headsets {
            NSLog(hs.toString())
        }
    }
}
