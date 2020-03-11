//
//  MarkerViewController.swift
//  CortexV2-Example-Swift
//
//  Created by nvtu on 2/21/20.
//  Copyright © 2020 Emotiv. All rights reserved.
//

import UIKit

class MarkerViewController: DataStreamViewController {

    var recordId = ""
    var markerId = ""
    
    override func viewDidLoad() {
        super.viewDidLoad()
        // Do any additional setup after loading the view.
    }
    
    override func handleCortexEvent() {
        super.handleCortexEvent()
        client.onCreateRecordOk = { [weak self] (recordId) in
            guard let weakSelf = self else { return }
            weakSelf.recordId = recordId
            
            // after a few seconds, inject some markers
            Timer.scheduledTimer(withTimeInterval: 5, repeats: false) { [weak self] (timer) in
                guard let weakSelf = self else { return }
                weakSelf.injectMarker1()
            }
            
            Timer.scheduledTimer(withTimeInterval: 13, repeats: false) { [weak self] (timer) in
                guard let weakSelf = self else { return }
                weakSelf.injectMarker2()
            }
            
            Timer.scheduledTimer(withTimeInterval: 21, repeats: false) { [weak self] (timer) in
                guard let weakSelf = self else { return }
                weakSelf.injectStopMarker2()
            }
            
            // close the session after 30 seconds
            Timer.scheduledTimer(withTimeInterval: 30, repeats: false) { [weak self] (timer) in
                guard let weakSelf = self else { return }
                weakSelf.stopRecord()
            }
        }
        
        client.onInjectMarkerOk = { [weak self] (markerId) in
            guard let weakSelf = self else { return }
            NSLog("Inject marker OK, marker id \(markerId)")
            weakSelf.markerId = markerId
        }
        
        client.onUpdateMarkerOk = { () in
            NSLog("Update marker OK")
        }
        
        client.onStopRecordOk = { [weak self] (recordId) in
            guard let weakSelf = self else { return }
            weakSelf.client.getRecordInfos(token: weakSelf.token, recordId: recordId)
        }
    }
    
    override func handleSessiontEvent() {
        super.handleSessiontEvent()
        creator.onSessionCreated = { [weak self] (token, sessionId) in
            guard let weakSelf = self else { return }
            weakSelf.token = token
            weakSelf.sessionId = sessionId
            weakSelf.client.createRecord(token: token, sessionId: sessionId, title: "Cortex Examples Swift")
        }
    }
    
    private func stopRecord() {
        NSLog("Stopping the record")
        client.stopRecord(token: token, sessionId: sessionId)
    }
    
    private func injectMarker1() {
        NSLog("Inject marker test1")
        client.injectMarker(token: token, sessionId: sessionId, label: "test1", value: 41, time: Int64(Date().timeIntervalSince1970 * 1000));
    }
    
    private func injectMarker2() {
        NSLog("Inject marker test2")
        client.injectMarker(token: token, sessionId: sessionId, label: "test2", value: 41, time: Int64(Date().timeIntervalSince1970 * 1000));
    }
    
    private func injectStopMarker2() {
        NSLog("Update marker for test2, marker id \(markerId)")
        client.updateMarker(token: token, sessionId: sessionId, markerId: markerId, time: Int64(Date().timeIntervalSince1970 * 1000));
    }
}
