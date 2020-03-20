//
//  DataStreamViewController.swift
//  CortexV2-Example-Swift
//
//  Created by Emotiv Inc on 2/21/20.
//  Copyright © 2020 Emotiv. All rights reserved.
//

import UIKit

class DataStreamViewController: UIViewController, UITableViewDelegate, UITableViewDataSource {

    @IBOutlet weak var authorizeView: UIView!
    @IBOutlet weak var dataView: UIView!
    @IBOutlet weak var trainingView: UIView!
    @IBOutlet weak var headsetTableView: UITableView!
    @IBOutlet weak var markerView: UIView!
    @IBOutlet weak var streamView: UIView!
    
    @IBOutlet weak var subscribeBtn: UIButton!
    @IBOutlet weak var unsubscribeBtn: UIButton!
    @IBOutlet weak var createSession: UIButton!
    @IBOutlet weak var getLicenseInfo: UIButton!
    @IBOutlet weak var getUserInfo: UIButton!
    @IBOutlet weak var training1: UIButton!
    @IBOutlet weak var training2: UIButton!
    @IBOutlet weak var training: UIButton!
    @IBOutlet weak var loadProfile: UIButton!
    @IBOutlet weak var subscribeTraining: UIButton!
    @IBOutlet weak var createProfile: UIButton!
    @IBOutlet weak var injectMarker: UIButton!
    @IBOutlet weak var stopRecord: UIButton!
    @IBOutlet weak var createRecord: UIButton!
    @IBOutlet weak var warning: UILabel!
    
    var client = CortexClient.sharedInstance
    var creator = SessionCreator()
    var finder = HeadsetFinder()
    var headset = Headset()
    var headsetList: [Headset] = [Headset]()
    var activateSession: Bool = false
    var license: String = ""

    var token: String = ""
    var sessionId: String = ""
    var stream: String = ""
    
    var action: String = ""
    var recordId: String = ""
    var markerId: String = ""
    
    let cellReuseIdentifier = "HeadsetTableViewCell"
    
    override func viewDidLoad() {
        super.viewDidLoad()
        start()

        let headsetTableViewCell = UINib(nibName: cellReuseIdentifier, bundle: nil)
        self.headsetTableView.register(headsetTableViewCell, forCellReuseIdentifier: cellReuseIdentifier)
        headsetTableView.delegate = self
        headsetTableView.dataSource = self
        
        handleCortexEvent()
        handleHeadsetEvent()
        handleSessiontEvent()
        handleUI()
    }
    
    @IBAction func getUserLogin(_ sender: Any) {
        client.getUserLogin()
    }
    
    @IBAction func hasAccessRight(_ sender: Any) {
        client.hasAccessRight(clientId: ClientId, clientSecret: ClientSecret)
    }
    
    @IBAction func requestAccess(_ sender: Any) {
        client.requestAccess(clientId: ClientId, clientSecret: ClientSecret)
    }
    
    @IBAction func authorize(_ sender: Any) {
        client.authorize(clientId: ClientId, clientSecret: ClientSecret, license: license, debit: activateSession ? 1 : 0)
    }
    
    @IBAction func getUserInformation(_ sender: Any) {
        client.getUserInformation(token: token)
    }
    
    @IBAction func getLicenseInfo(_ sender: Any) {
        client.getLicenseInfos(token: token)
    }
    
    @IBAction func queryHeadsets(_ sender: Any) {
        finder.findHeadsets()
    }
    
    @IBAction func createSession(_ sender: Any) {
        creator.createSession(token: token, headset: headset, activate: activateSession, license: license)
    }
    
    @IBAction func subcribe(_ sender: Any) {
        client.subscribe(token: token, sessionId: sessionId, stream: stream)
    }
    
    @IBAction func unsubscribe(_ sender: Any) {
        client.unsubscribe(token: token, sessionId: sessionId, stream: stream)
    }
    
    @IBAction func subcribeTraining(_ sender: Any) {
        client.subscribe(token: token, sessionId: sessionId, stream: "sys")
    }
    
    @IBAction func createProfile(_ sender: Any) {
        client.createProfile(token: token, profileName: TrainingProfileName)
    }
    
    @IBAction func loadProfile(_ sender: Any) {
        client.loadProfile(token: token, headsetId: headset.id, profileName: TrainingProfileName)
    }
    
    @IBAction func trainNeutral(_ sender: Any) {
        self.action = "neutral"
        client.training(token: token, sessionId: sessionId, detection: stream , action: self.action, control: "start")
        self.enableButton(enable: false)
        NSLog("waiting training result from Cortex")
    }
    
    @IBAction func trainPush(_ sender: Any) {
        self.action = stream == "mentalCommand" ? "push" : "smile"
        client.training(token: token, sessionId: sessionId, detection: stream , action: self.action, control: "start")
        self.enableButton(enable: false)
        NSLog("waiting training result from Cortex")
    }
    
    @IBAction func createRecord(_ sender: Any) {
        client.createRecord(token: token, sessionId: sessionId, title: "Cortex Examples Swift")
    }
    
    @IBAction func injectMarker(_ sender: Any) {
        client.injectMarker(token: token, sessionId: sessionId, label: "test1", value: 41, time: Int64(Date().timeIntervalSince1970 * 1000));
    }
    
    @IBAction func stopRecord(_ sender: Any) {
        client.stopRecord(token: token, sessionId: sessionId)
    }
    
    @IBAction func training(_ sender: Any) {
        if sessionId == "" {
            NSLog("Please create session")
        } else {
            if self.stream == "session" {
                client.closeSession(token: self.token, sessionId: self.sessionId)
            } else {
                self.dataView.isHidden = true
                self.trainingView.isHidden = false
            }
        }
    }
    
    func start() {
        client.open()
    }
    
    func handleUI() {
        var nameStream = ""
        if stream == "mot" {
            nameStream = "motion"
        } else if stream == "eeg" {
            nameStream = "eeg"
        } else if stream == "pow" {
            nameStream = "Band Power"
        } else if stream == "com" {
            nameStream = "MC"
        } else if stream == "fac" {
            nameStream = "FE"
        } else if stream == "met" {
            nameStream = "PM"
        } else if stream == "facialExpression" {
            training.isHidden = false
            subscribeBtn.isHidden = true
            unsubscribeBtn.isHidden = true
            training1.setTitle("train FE Neutral", for: UIControl.State.normal)
            training2.setTitle("train FE Smile", for: UIControl.State.normal)
        } else if stream == "mentalCommand" {
            training.isHidden = false
            subscribeBtn.isHidden = true
            unsubscribeBtn.isHidden = true
            training1.setTitle("train MC Neutral", for: UIControl.State.normal)
            training2.setTitle("train MC Push", for: UIControl.State.normal)
        } else if stream == "marker" {
            streamView.isHidden = true
            markerView.isHidden = false
        }
        
        if stream != "authorize" {
            authorizeView.isHidden = true
            dataView.isHidden = false
        }

        if stream == "session" {
            training.isHidden = false
            subscribeBtn.isHidden = true
            unsubscribeBtn.isHidden = true
            self.training.setTitle("updateSession(Close)", for: UIControl.State.normal)
        }
        
        self.subscribeBtn.setTitle("subcribe " + nameStream, for: UIControl.State.normal)
        self.unsubscribeBtn.setTitle("unsubcribe " + nameStream, for: UIControl.State.normal)
        Timer.scheduledTimer(withTimeInterval: 60, repeats: false) { [weak self] (timer) in
            guard let weakSelf = self else { return }
            weakSelf.warning.isHidden = true
        }
    }
    
    func handleCortexEvent() {
        client.onConnected = { [weak self] in
        guard let weakSelf = self else { return }
            NSLog("Connected.")
            if weakSelf.stream != "authorize" {
                weakSelf.client.authorize(clientId: ClientId, clientSecret: ClientSecret, license: weakSelf.license, debit: weakSelf.activateSession ? 1 : 0)
            }
        }
        
        client.onDisconnected = {
            NSLog("Disconnected.")
        }
        
        client.onErrorReceived = { (method, code, message) in
            
        }
        
        client.onGetUserLoginOk = { (emotivId) in
           if emotivId == "" {
               NSLog("First, you must login with EMOTIV App")
               return
           }
           NSLog("You are logged in with the EmotivId \(emotivId)")
        }

        client.onUnsubscribeOk = { [weak self] (streams) in
            guard let weakSelf = self else { return }
            NSLog("Subscription cancelled for data streams \(streams)")
            weakSelf.client.closeSession(token: weakSelf.token, sessionId: weakSelf.sessionId)
        }
        
        client.onCloseSessionOk = {
            NSLog("Session closed.")
        }
        
        client.onGetLicenseInformationOk = {
            NSLog("Get License Information successful")
        }
        
        client.onGetUserInformationOk = {
            NSLog("Get User Information successful")
        }
        
        client.onAuthorizeOk = { [weak self] (token) in
            guard let weakSelf = self else { return }
            weakSelf.token = token
            NSLog("Authorize successful, token \(token)")
            if weakSelf.stream != "authorize" {
                weakSelf.finder.findHeadsets()
            }
        }
        
        client.onSubscribeOk = { [weak self] (streams) in
            guard let weakSelf = self else { return }
            NSLog("Subscription to data stream successful \(streams)")
            weakSelf.training1.isEnabled = true
            weakSelf.training2.isEnabled = true
        }
        
        client.onSaveProfileOk = { (profileName) in
            NSLog("Training profile saved \(profileName)")
        }
        
        client.onLoadProfileOk = { [weak self] (profileName) in
            guard let weakSelf = self else { return }
            NSLog("Training profile loaded \(profileName)")
            weakSelf.training1.isEnabled = true
            weakSelf.training2.isEnabled = true
        }
        
        client.onCreateProfileOk = { (profileName) in
            NSLog("Training profile created \(profileName)")
        }
        
        
        client.onStreamDataReceived = { [weak self] (sessionId, stream, time, data) in
            guard let weakSelf = self else { return }
            if weakSelf.stream == "facialExpression" || weakSelf.stream == "mentalCommand" {
                if weakSelf.isEvent(data: data, event: "Started") {
                    NSLog("")
                    NSLog("Please, focus on the action for a few seconds")
                } else if weakSelf.isEvent(data: data, event: "Succeeded") {
                    weakSelf.client.training(token: weakSelf.token, sessionId: weakSelf.sessionId, detection: weakSelf.stream, action: weakSelf.action, control: "accept")
                } else if weakSelf.isEvent(data: data, event: "Failed") {
                    weakSelf.enableButton(enable: true)
                } else if weakSelf.isEvent(data: data, event: "Completed") {
                    NSLog("Well done! You successfully trained")
                    weakSelf.enableButton(enable: true)
                }
            }
            NSLog("\(stream) \(data)")
        }
        
        client.onTrainingOk = { (msg) in
            // this signal is not important
            // instead we need to watch the events from the sys stream
        }
        
        client.onCreateRecordOk = { [weak self] (recordId) in
            guard let weakSelf = self else { return }
            weakSelf.recordId = recordId
            weakSelf.stopRecord.isEnabled = true
            weakSelf.injectMarker.isEnabled = true
        }
        
        client.onInjectMarkerOk = { [weak self] (markerId) in
            guard let weakSelf = self else { return }
            NSLog("Inject marker OK, marker id \(markerId)")
            weakSelf.markerId = markerId
        }
        
        client.onStopRecordOk = { [weak self] (recordId) in
            guard let weakSelf = self else { return }
            weakSelf.stopRecord.isEnabled = false
            weakSelf.injectMarker.isEnabled = false
            weakSelf.client.getRecordInfos(token: weakSelf.token, recordId: recordId)
        }
        
        client.onCloseSessionOk = {
            NSLog("Close session successful")
        }
    }
    
    private func handleHeadsetEvent() {
        finder.onHeadsetFound = { [weak self] (headset, headsets) in
            guard let weakSelf = self else { return }
            weakSelf.headset = headset
            weakSelf.headsetList = headsets
            weakSelf.headsetTableView.reloadData()
        }
    }
    
    func handleSessiontEvent() {
        creator.onSessionCreated = { [weak self] (token, sessionId) in
            guard let weakSelf = self else { return }
            weakSelf.token = token
            weakSelf.sessionId = sessionId
        }
    }
    
    private func enableButton(enable: Bool) {
        self.training1.isEnabled = enable
        self.training2.isEnabled = enable
        self.createProfile.isEnabled = enable
        self.loadProfile.isEnabled = enable
        self.subscribeTraining.isEnabled = enable
    }
    
    private func isEvent(data: NSArray, event: String) -> Bool {
        for val in data {
            if val is String && (val as! String).contains(event) {
                return true
            }
        }
        return false
    }
    
    // number of rows in table view
    func tableView(_ tableView: UITableView, numberOfRowsInSection section: Int) -> Int {
        return headsetList.count
    }

    // create a cell for each table view row
    func tableView(_ tableView: UITableView, cellForRowAt indexPath: IndexPath) -> UITableViewCell {
        let hscell = tableView.dequeueReusableCell(withIdentifier: cellReuseIdentifier) as? HeadsetTableViewCell
        hscell?.status.text = headsetList[indexPath.row].status
        hscell?.headsetName.text = headsetList[indexPath.row].id
        hscell?.connectedBy.text = headsetList[indexPath.row].connectedBy
        return hscell ?? UITableViewCell()
    }
    
    func tableView(_ tableView: UITableView, viewForHeaderInSection section: Int) -> UIView? {
        let headerCell = tableView.dequeueReusableCell(withIdentifier: cellReuseIdentifier) as? HeadsetTableViewCell
        return headerCell ?? UITableViewCell()
    }
    
    func tableView(_ tableView: UITableView, heightForHeaderInSection section: Int) -> CGFloat {
        return 50
    }
    
    func tableView(_ tableView: UITableView, heightForRowAt indexPath: IndexPath) -> CGFloat {
        return 50
    }
}

