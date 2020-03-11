//
//  DataStreamViewController.swift
//  CortexV2-Example-Swift
//
//  Created by nvtu on 2/21/20.
//  Copyright © 2020 Emotiv. All rights reserved.
//

import UIKit

class DataStreamViewController: UIViewController {

    var client = CortexClient.sharedInstance
    var creator = SessionCreator()
    var finder = HeadsetFinder()
    var headset = Headset()
    var activateSession: Bool = false
    var license: String = ""

    var token: String = ""
    var sessionId: String = ""
    var stream: String = ""

    override func viewDidLoad() {
        super.viewDidLoad()

        handleCortexEvent()
        handleHeadsetEvent()
        handleSessiontEvent()
        start()
        // Do any additional setup after loading the view.
    }
    
    func start() {
        client.open()
    }
    
    func handleCortexEvent() {
        client.onConnected = { [weak self] in
            guard let weakSelf = self else { return }
            weakSelf.finder.findHeadsets()
        }
        
        client.onDisconnected = { [weak self] in
            NSLog("Disconnected.")
        }
        

        client.onErrorReceived = { (method, code, message) in
            if method == "setupProfile" {
                // it's fine, we can subscribe to a data stream even without a profile
                NSLog("Failed to load the training profile.")
            }
        }
        
        client.onLoadProfileOk = { (profileName) in
            NSLog("Training profile loaded \(profileName)")
        }
        
        client.onSubscribeOk = { [weak self] (streams) in
            guard let weakSelf = self else { return }
            NSLog("Subscription successful for data streams \(streams)")
            NSLog("Receiving data for 30 seconds.")
            Timer.scheduledTimer(withTimeInterval: 30, repeats: false) { [weak weakSelf] (time) in
                guard let weakSelf2 = weakSelf else { return }
                weakSelf2.client.unsubscribe(token: weakSelf2.token, sessionId: weakSelf2.sessionId, stream: weakSelf2.stream)
            }
        }
        
        client.onStreamDataReceived = { (sessionId, stream, time, data) in
            // a data stream can publish data with a high frequency
            // we display only a few samples per second
            NSLog("\(stream) \(data)")
        }
        
        client.onUnsubscribeOk = { [weak self] (streams) in
            guard let weakSelf = self else { return }
            NSLog("Subscription cancelled for data streams \(streams)")
            weakSelf.client.closeSession(token: weakSelf.token, sessionId: weakSelf.sessionId)
        }
        
        client.onCloseSessionOk = { [weak self] in
            guard let weakSelf = self else { return }
            NSLog("Session closed.")
            weakSelf.client.close()
        }
    }
    
    private func handleHeadsetEvent() {
        finder.onHeadsetFound = { [weak self] (headset) in
            guard let weakSelf = self else { return }
            weakSelf.headset = headset
            // next step: create a session for this headset
            weakSelf.creator.createSession(headset: headset, activate: weakSelf.activateSession, license: weakSelf.license)
        }
    }
    
    func handleSessiontEvent() {
        creator.onSessionCreated = { [weak self] (token, sessionId) in
            guard let weakSelf = self else { return }
            weakSelf.token = token
            weakSelf.sessionId = sessionId
            // load the training profile (useful only for mental command and facial expression)
            weakSelf.client.loadProfile(token: token, headsetId: weakSelf.headset.id, profileName: TrainingProfileName)
            // next step: subscribe to a data stream
            weakSelf.client.subscribe(token: token, sessionId: sessionId, stream: weakSelf.stream)
        }
    }
}

