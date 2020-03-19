//
//  SessionCreator.swift
//  CortexV2-Example-Swift
//
//  Created by Emotiv Inc on 2/21/20.
//  Copyright © 2020 Emotiv. All rights reserved.
//

import Foundation

class SessionCreator {
    
    private var client: CortexClient!
    private var headset: Headset = Headset()
    private var activate: Bool = false
    private var license: String = ""
    private var token: String = ""

    var onSessionCreated: ((String, String) -> (Void))!
    
    init() {
        
    }
    
    func createSession(token: String, headset: Headset, activate: Bool, license: String) {
        client = CortexClient.sharedInstance
        self.token = token
        self.headset = headset
        self.activate = activate
        self.license = license
        handleCortexEvent()
    }
    
    private func handleCortexEvent() {
        
        client.createSession(token: token, headsetId: headset.id, activate: activate)

        client.onCreateSessionOk = { [weak self] (sessionId) in
            guard let weakSelf = self else { return }
            NSLog("Session created, session id \(sessionId)")
            // next step: open a session for the headset
            if weakSelf.onSessionCreated != nil {
                weakSelf.onSessionCreated(weakSelf.token, sessionId)
            }
        }
    }
}
