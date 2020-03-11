//
//  TrainingViewController.swift
//  CortexV2-Example-Swift
//
//  Created by nvtu on 2/21/20.
//  Copyright © 2020 Emotiv. All rights reserved.
//

import UIKit

class TrainingViewController: DataStreamViewController {

    var actions: [String] = []
    var actionIndex = 0
    var actionCount = 0
    var trainingFailure = 0
    var detection = ""
    
    override func viewDidLoad() {
        super.viewDidLoad()
        detection = stream
        // Do any additional setup after loading the view.
    }
    
    override func handleCortexEvent() {
        super.handleCortexEvent()
        client.onConnected = { [weak self] in
            guard let weakSelf = self else { return }
            weakSelf.client.getDetectionInfo(detection: weakSelf.detection)
        }
        
        client.onGetDetectionInfoOk = { [weak self] (actions, controls, events) in
            guard let weakSelf = self else { return }
            weakSelf.actions = actions
            NSLog("Information for \(weakSelf.detection): ")
            NSLog("Actions \(actions)")
            NSLog("Controls \(controls)")
            NSLog("Events \(events)")
            weakSelf.finder.findHeadsets()
        }
        
        client.onQueryProfileOk = { [weak self] (profiles) in
            guard let weakSelf = self else { return }
            if profiles.contains(TrainingProfileName) {
                weakSelf.client.loadProfile(token: weakSelf.token, headsetId: weakSelf.headset.id, profileName: TrainingProfileName)
            } else {
                weakSelf.client.createProfile(token: weakSelf.token, profileName: TrainingProfileName)
            }
        }
        
        client.onCreateProfileOk = { [weak self] (profileName) in
            guard let weakSelf = self else { return }
            NSLog("Training profile created \(profileName)")
            weakSelf.client.loadProfile(token: weakSelf.token, headsetId: weakSelf.headset.id, profileName: profileName)
        }
        
        client.onLoadProfileOk = { [weak self] (profileName) in
            guard let weakSelf = self else { return }
            NSLog("Training profile loaded \(profileName)")
            // we must subscribe to the "sys" stream to receive training events
            weakSelf.client.subscribe(token: weakSelf.token, sessionId: weakSelf.sessionId, stream: "sys")
        }
        
        client.onSaveProfileOk = { (profileName) in
            NSLog("Training profile saved \(profileName)")
        }
        
        client.onSubscribeOk = { [weak self] (streams) in
            guard let weakSelf = self else { return }
            NSLog("Subscription to data stream successful \(streams)")
            weakSelf.client.training(token: weakSelf.token, sessionId: weakSelf.sessionId, detection: weakSelf.detection, action: weakSelf.action(), control: "start")
        }
        
        client.onTrainingOk = { (msg) in
            // this signal is not important
            // instead we need to watch the events from the sys stream
        }
        
        client.onStreamDataReceived = { [weak self] (sessionId, stream, time, data) in
            guard let weakSelf = self else { return }
            if weakSelf.isEvent(data: data, event: "Started") {
                NSLog("")
                NSLog("Please, focus on the action \(weakSelf.action().uppercased()) for a few seconds")
            } else if weakSelf.isEvent(data: data, event: "Succeeded") {
                weakSelf.client.training(token: weakSelf.token, sessionId: weakSelf.sessionId, detection: weakSelf.detection, action: weakSelf.action(), control: "accept")
            } else if weakSelf.isEvent(data: data, event: "Failed") {
                weakSelf.retryAction();
            } else if weakSelf.isEvent(data: data, event: "Completed") {
                NSLog("Well done! You successfully trained \(weakSelf.action())")
                weakSelf.nextAction();
            }
        }
    }
    
    override func handleSessiontEvent() {
        super.handleSessiontEvent()
        creator.onSessionCreated = { [weak self] (token, sessionId) in
            guard let weakSelf = self else { return }
            weakSelf.token = token
            weakSelf.sessionId = sessionId
            // load the training profile (useful only for mental command and facial expression)
            weakSelf.client.queryProfile(token: token)
        }
    }
    
    private func action() -> String {
        return actions[actionIndex]
    }
    
    private func isEvent(data: NSArray, event: String) -> Bool {
        for val in data {
            if val is String && (val as! String).contains(event) {
                return true
            }
        }
        return false
    }
    
    private func nextAction() {
        let untrainableActions = ["blink", "winkL", "winkR", "horiEye"]
        
        actionIndex += 1;
        trainingFailure = 0;

        // some facial expression actions cannot be trained, we must skip them
        while (untrainableActions.contains(action())) {
            actionIndex += 1;
        }
        actionCount += 1;

        if (actionCount < 3 && actionIndex < actions.count) {
            // ok, let's train the next action
            client.training(token: token, sessionId: sessionId, detection: detection, action: action(), control: "start");
        }
        else {
            // that's enough training for today
            // we save the training profile before we quit
            NSLog("Saving training profile \(TrainingProfileName)")
            client.saveProfile(token: token, headsetId: headset.id, profileName: TrainingProfileName);
        }
    }
    
    private func retryAction() {
        trainingFailure += 1

        if (trainingFailure < 3) {
            NSLog("Sorry, it didn't work. Let's try again.")
            client.training(token: token, sessionId: sessionId, detection: detection, action: action(), control: "start")
        }
        else {
            NSLog("It seems you are struggling with this action. Let's try another one.")
            nextAction();
        }
    }
}
