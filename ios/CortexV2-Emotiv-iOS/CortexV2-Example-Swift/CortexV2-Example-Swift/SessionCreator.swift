//
//  SessionCreator.swift
//  CortexV2-Example-Swift
//
//  Created by nvtu on 2/21/20.
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
    
    func createSession(headset: Headset, activate: Bool, license: String) {
        client = CortexClient.sharedInstance
        self.headset = headset
        self.activate = activate
        self.license = license
        handleCortexEvent()
        client.getUserLogin()
    }
    
    private func handleCortexEvent() {
        client.onGetUserLoginOk = { [weak self] (emotivId) in
            guard let weakSelf = self else { return }
            if emotivId == "" {
                NSLog("First, you must login with EMOTIV App")
                return
            }
            NSLog("You are logged in with the EmotivId \(emotivId)")
            weakSelf.client.requestAccess(clientId: ClientId, clientSecret: ClientSecret)
        }
        
        client.onRequestAccessOk = { [weak self] (accessGranted, message) in
            guard let weakSelf = self else { return }
            if accessGranted {
                NSLog("This application was authorized in EMOTIV App")
                let debit = weakSelf.activate ? 1 : 0
                weakSelf.client.authorize(clientId: ClientId, clientSecret: ClientSecret, license: weakSelf.license, debit: debit)
                return
            } else {
                NSLog(message)
                Timer.scheduledTimer(withTimeInterval: 2, repeats: false) { [weak self] (timer) in
                    guard let weakSelf = self else { return }
                    weakSelf.client.requestAccess(clientId: ClientId, clientSecret: ClientSecret)
                }
            }
        }
        
        client.onAuthorizeOk = { [weak self] (token) in
            guard let weakSelf = self else { return }
            weakSelf.token = token
            NSLog("Authorize successful, token \(token)")
            // next step: open a session for the headset
            weakSelf.client.createSession(token: token, headsetId: weakSelf.headset.id, activate: weakSelf.activate)
        }
        
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
