//
//  CortexClient.swift
//  CortexV2-Example-Swift
//
//  Created by Emotiv Inc on 2/21/20.
//  Copyright © 2020 Emotiv. All rights reserved.
//

import Starscream

class CortexClient {
    
    static let sharedInstance = CortexClient()
    private let socket: WebSocket
    private var nextRequestId = 0
    private var methodForRequestId: [Int: String] = [:]

    var onConnected: (() -> Void)!
    var onDisconnected: (() -> Void)!

    var onQueryHeadsetsOk: (([Headset]) -> Void)!
    var onGetUserLoginOk: ((String) -> Void)!
    var onRequestAccessOk: ((Bool, String) -> Void)!
    var onHasAccesRightOk: ((Bool, String) -> Void)!
    var onAuthorizeOk: ((String) -> Void)!
    var onCreateSessionOk: ((String) -> Void)!
    var onUpdateSessionOk: (() -> Void)!
    var onCloseSessionOk: (() -> Void)!
    var onSubscribeOk: (([String]) -> Void)!
    var onUnsubscribeOk: (([String]) -> Void)!
    var onQueryProfileOk: (([String]) -> Void)!
    var onCreateProfileOk: ((String) -> Void)!
    var onLoadProfileOk: ((String) -> Void)!
    var onSaveProfileOk: ((String) -> Void)!
    var onGetDetectionInfoOk: (([String], [String], [String]) -> Void)!
    var onTrainingOk: ((String) -> Void)!
    var onCreateRecordOk: ((String) -> Void)!
    var onStopRecordOk: ((String) -> Void)!
    var onGetRecordInfosOk: (([String : Any]) -> Void)!
    var onInjectMarkerOk: ((String) -> Void)!
    var onUpdateMarkerOk: (() -> Void)!
    var onGetUserInformationOk: (() -> Void)!
    var onGetLicenseInformationOk: (() -> Void)!
    
    // we received an error message in response to a RPC request
    var onErrorReceived: ((String, Int, String) -> Void)!

    // we received data from a data stream
    var onStreamDataReceived: ((String, String, Double, NSArray) -> Void)!
    
    private init() {
        socket = WebSocket(url: URL(string: "wss://localhost:6868/")!)
        socket.delegate = self
        nextRequestId = 1
    }
    
    func open() {
        socket.connect()
    }
    
    func close() {
        socket.disconnect()
    }
    
    func queryHeadsets(headsetId: String = "") {
        var params: [String: Any] = [:]
        if headsetId != "" {
            params["id"] = headsetId
        }
        sendRequest(method: "queryHeadsets", params: params)
    }
    
    func getUserLogin() {
        sendRequest(method:"getUserLogin")
    }
    
    func requestAccess(clientId: String, clientSecret: String) {
        var params: [String: Any] = [:]
        params["clientId"] = clientId
        params["clientSecret"] = clientSecret
        sendRequest(method: "requestAccess", params: params)
    }
    
    func hasAccessRight(clientId: String, clientSecret: String) {
        var params: [String: Any] = [:]
        params["clientId"] = clientId
        params["clientSecret"] = clientSecret
        sendRequest(method: "hasAccessRight", params: params)
    }
    
    func authorize(clientId: String, clientSecret: String, license: String, debit: Int) {
        var params: [String: Any] = [:]
        params["clientId"] = clientId
        params["clientSecret"] = clientSecret
        if license != "" {
            params["license"] = license
        }
        params["debit"] = debit
        sendRequest(method: "authorize", params: params)
    }
    
    func createSession(token: String, headsetId: String, activate: Bool) {
        var params: [String: Any] = [:]
        params["cortexToken"] = token
        params["headset"] = headsetId
        params["status"] = activate ? "active" : "open"
        sendRequest(method: "createSession", params: params)
    }
    
    func updateSession(token: String, headsetId: String, activate: Bool) {
        var params: [String: Any] = [:]
        params["cortexToken"] = token
        params["headset"] = headsetId
        params["status"] = activate ? "active" : "close"
        sendRequest(method: "createSession", params: params)
    }

    func closeSession(token: String, sessionId: String) {
        var params: [String: Any] = [:]
        params["cortexToken"] = token
        params["session"] = sessionId
        params["status"] = "close"
        sendRequest(method: "updateSession", params: params)
    }
    
    func subscribe(token: String, sessionId: String, stream: String) {
        var params: [String: Any] = [:]
        var streamArray: [String] = []
        streamArray.append(stream)
        params["cortexToken"] = token
        params["session"] = sessionId
        params["streams"] = streamArray
        sendRequest(method: "subscribe", params: params)
    }
    
    func unsubscribe(token: String, sessionId: String, stream: String) {
        var params: [String: Any] = [:]
        var streamArray: [String] = []
        streamArray.append(stream)
        params["cortexToken"] = token
        params["session"] = sessionId
        params["streams"] = streamArray
        sendRequest(method: "unsubscribe", params: params)
    }
    
    func queryProfile(token: String)
    {
        var params: [String: Any] = [:]
        params["cortexToken"] = token
        sendRequest(method: "queryProfile", params: params)
    }
    
    func createProfile(token: String, profileName: String)
    {
        var params: [String: Any] = [:]
        params["cortexToken"] = token
        params["profile"] = profileName
        params["status"] = "create"
        sendRequest(method: "setupProfile", params: params)
    }

    func loadProfile(token: String, headsetId: String, profileName: String)
    {
        var params: [String: Any] = [:]
        params["cortexToken"] = token
        params["headset"] = headsetId
        params["profile"] = profileName
        params["status"] = "load"
        sendRequest(method: "setupProfile", params: params)
    }
    
    func saveProfile(token: String, headsetId: String, profileName: String)
    {
        var params: [String: Any] = [:]
        params["cortexToken"] = token
        params["headset"] = headsetId
        params["profile"] = profileName
        params["status"] = "save"
        sendRequest(method: "setupProfile", params: params)
    }
    
    func getDetectionInfo(detection: String) {
        var params: [String: Any] = [:]
        params["detection"] = detection
        sendRequest(method: "getDetectionInfo", params: params)
    }
    
    func training(token: String, sessionId: String, detection: String, action: String, control: String) {
        var params: [String: Any] = [:]
        params["cortexToken"] = token
        params["session"] = sessionId
        params["detection"] = detection
        params["action"] = action
        params["status"] = control
        sendRequest(method: "training", params: params)
    }
    
    func createRecord(token: String, sessionId: String, title: String)
    {
        var params: [String: Any] = [:]
        params["cortexToken"] = token
        params["session"] = sessionId
        params["title"] = title
        sendRequest(method: "createRecord", params: params)
    }
    
    func stopRecord(token: String, sessionId: String)
    {
        var params: [String: Any] = [:]
        params["cortexToken"] = token
        params["session"] = sessionId
        sendRequest(method: "stopRecord", params: params)
    }
    
    func getRecordInfos(token: String, recordId: String)
    {
        var params: [String: Any] = [:]
        params["cortexToken"] = token
        params["recordIds"] = [recordId]
        sendRequest(method: "getRecordInfos", params: params)
    }
    
    func getLicenseInfos(token: String)
    {
        var params: [String: Any] = [:]
        params["cortexToken"] = token
        sendRequest(method: "getLicenseInfo", params: params)
    }
    
    func injectMarker(token: String, sessionId: String, label: String, value: Int, time: Int64) {
        var params: [String: Any] = [:]
        params["cortexToken"] = token
        params["session"] = sessionId
        params["label"] = label
        params["value"] = value
        params["port"] = "Cortex Example"
        params["time"] = time
        sendRequest(method: "injectMarker", params: params)
    }

    func updateMarker(token: String, sessionId: String, markerId: String, time: Int64)
    {
        var params: [String: Any] = [:]
        params["cortexToken"] = token
        params["session"] = sessionId
        params["markerId"] = markerId
        params["time"] = time
        sendRequest(method: "updateMarker", params: params)
    }
    
    func getUserInformation(token: String)
    {
        var params: [String: Any] = [:]
        params["cortexToken"] = token
        sendRequest(method: "getUserInformation", params: params)
    }

    fileprivate func sendRequest(method: String, params: [String: Any] = [:]) {
        var request: [String: Any] = [:]

        // build the request
        request["jsonrpc"] = "2.0"
        request["id"] = nextRequestId
        request["method"] = method
        request["params"] = params

        do {
            let jsonData = try JSONSerialization.data(withJSONObject: request, options: .prettyPrinted)
            let decoded = String(data: jsonData, encoding: .utf8)!
            // send the message
            NSLog("send \(decoded)")
            socket.write(string: decoded)
        } catch {
            print(error.localizedDescription)
        }
        
        // remember the method used for this request
        methodForRequestId[nextRequestId] = method
        nextRequestId += 1
    }
    
    fileprivate func handleResponse(method: String, result: Any) {
        if (method == "queryHeadsets") {
            var headsets: [Headset] = []
            if result is NSArray {
                for val in (result as! NSArray) {
                    if val is [String: Any] {
                        let headset = Headset(jHeadset: val as! [String: Any])
                        headsets.append(headset)
                    }
                }
            }
            if self.onQueryHeadsetsOk != nil {
                self.onQueryHeadsetsOk!(headsets)
            }
        } else if (method == "getUserLogin") {
            var emotivId = ""
            if result is NSArray {
                if (result as! NSArray).count != 0 {
                    let obj = (result as! NSArray)[0] as? [String: Any] ?? [:]
                    let currentOSUId = obj["currentOSUId"] as? String ?? ""
                    let loggedInOSUId = obj["loggedInOSUId"] as? String ?? ""
                    if currentOSUId == loggedInOSUId {
                        emotivId = obj["username"]as? String ?? ""
                    }
                }
            }
            if self.onGetUserLoginOk != nil {
                self.onGetUserLoginOk!(emotivId)
            }
        } else if method == "requestAccess" {
            let accessGranted = (result as! [String: Any])["accessGranted"] as? Bool ?? false
            let message = (result as! [String: Any])["message"] as? String ?? ""
            if self.onRequestAccessOk != nil {
                self.onRequestAccessOk!(accessGranted, message)
            }
        } else if method == "hasAccessRight" {
            let accessGranted = (result as! [String: Any])["accessGranted"] as? Bool ?? false
            let message = (result as! [String: Any])["message"] as? String ?? ""
            if self.onHasAccesRightOk != nil {
                self.onHasAccesRightOk!(accessGranted, message)
            }
        } else if method == "authorize" {
            let token = (result as! [String: Any])["cortexToken"] as? String ?? ""
            if self.onAuthorizeOk != nil {
                self.onAuthorizeOk!(token)
            }
        } else if method == "createSession" {
            let sessionId = (result as! [String: Any])["id"] as? String ?? ""
            if self.onCreateSessionOk != nil {
                self.onCreateSessionOk!(sessionId)
            }
        } else if (method == "updateSession") {
            if self.onCloseSessionOk != nil {
                self.onCloseSessionOk!()
            }
        } else if (method == "subscribe") {
            let streams = parseSubscriptionResult(method: method, result: result)
            if !streams.isEmpty {
                if self.onSubscribeOk != nil {
                    self.onSubscribeOk!(streams)
                }
            }
        }  else if (method == "unsubscribe") {
            let streams = parseSubscriptionResult(method: method, result: result)
            if !streams.isEmpty {
                if self.onUnsubscribeOk != nil {
                    self.onUnsubscribeOk!(streams)
                }
            }
        } else if (method == "queryProfile") {
            var profiles: [String] = []
            let array = result as? NSArray ?? []
            for value in array {
                profiles.append((value as! [String: Any])["name"] as? String ?? "")
            }
            if self.onQueryProfileOk != nil {
                self.onQueryProfileOk!(profiles)
            }
        } else if (method == "setupProfile") {
            let obj = result as? [String: Any] ?? [:]
            let action = obj["action"] as? String ?? ""
            let name = obj["name"] as? String ?? ""
            if (action == "create") {
                if self.onCreateProfileOk != nil {
                    self.onCreateProfileOk!(name)
                }
            } else if (action == "load") {
                if self.onLoadProfileOk != nil {
                    self.onLoadProfileOk!(name)
                }
            } else if (action == "save") {
                if self.onSaveProfileOk != nil {
                    self.onSaveProfileOk!(name)
                }
            }
        } else if (method == "getDetectionInfo") {
            handleGetDetectionInfo(result: result)
        } else if (method == "training") {
            if self.onTrainingOk != nil {
                self.onTrainingOk!(result as? String ?? "")
            }
        } else if (method == "createRecord") {
            let record = (result as! [String: Any])["record"] as? [String: Any] ?? [:]
            if self.onCreateRecordOk != nil {
                self.onCreateRecordOk!(record["uuid"] as? String ?? "")
            }
        } else if (method == "stopRecord") {
            let record = (result as! [String: Any])["record"] as? [String: Any] ?? [:]
            if self.onStopRecordOk != nil {
                self.onStopRecordOk!(record["uuid"] as? String ?? "")
            }
        } else if (method == "getRecordInfos") {
            let record = (result as! NSArray)[0] as? [String: Any] ?? [:]
            if self.onGetRecordInfosOk != nil {
                self.onGetRecordInfosOk!(record)
            }
        } else if (method == "injectMarker") {
            let marker = (result as! [String: Any])["marker"]  as? [String: Any] ?? [:]
            let markerId = marker["uuid"] as? String ?? ""
            if self.onInjectMarkerOk != nil {
                self.onInjectMarkerOk!(markerId)
            }
        } else if (method == "updateMarker") {
            if self.onUpdateMarkerOk != nil {
                self.onUpdateMarkerOk!()
            }
        } else if (method == "getUserInformation") {
            if self.onGetUserInformationOk != nil {
                self.onGetUserInformationOk!()
            }
        } else if (method == "getLicenseInfo") {
            if self.onGetLicenseInformationOk != nil {
                self.onGetLicenseInformationOk!()
            }
        } else {
            // unknown method, so we don't know how to interpret the result
            NSLog("Result from an unexpected API method: \(method) \(result)")
        }
    }
    
    fileprivate func emitError(method: String, obj: [String: Any]) {
        let code = obj["code"] as? Int ?? 0
        let message = obj["message"] as? String ?? ""
        NSLog("The Cortex service returned an error:")
        NSLog("method \(method)")
        NSLog("code \(code)")
        NSLog("message \(message)")
        if self.onErrorReceived != nil {
            self.onErrorReceived!(method, code, message)
        }
    }
    
    private func handleGetDetectionInfo(result: Any) {
        let _result: [String: Any] = result as? [String : Any] ?? [:]
        let jactions = _result["actions"] as? NSArray ?? []
        let jcontrols = _result["controls"] as? NSArray ?? []
        let jevents = _result["events"] as? NSArray ?? []
        if self.onGetDetectionInfoOk != nil {
            self.onGetDetectionInfoOk!(arrayToStringList(array: jactions),
                                       arrayToStringList(array: jcontrols),
                                       arrayToStringList(array: jevents))
        }
    }

    private func parseSubscriptionResult(method: String, result: Any) -> [String]
    {
        let _result: [String: Any] = result as? [String : Any] ?? [:]
        let success = _result["success"] as? [Any] ?? []
        let failure = _result["failure"] as? [Any] ?? []
        var streams: [String] = []
        if !failure.isEmpty {
            for f in failure {
                emitError(method: method, obj: f as? [String : Any] ?? [:])
            }
            return streams
        }
        for s in success {
            let obj = s as? [String : Any] ?? [:]
            let stream = obj["streamName"] as? String ?? ""
            let columns = obj["cols"] as? [String] ?? []
            if !columns.isEmpty {
                NSLog("stream uses these columns: \(columns)")
            }
            streams.append(stream)
        }
        return streams
    }
    
    private func arrayToStringList(array: NSArray) -> [String] {
        var list: [String] = []
        for val in array {
            list.append(val as? String ?? "")
        }
        return list
    }

}

extension CortexClient: WebSocketDelegate {
    func websocketDidConnect(socket: WebSocketClient) {
        NSLog("socket connected")
        if self.onConnected != nil {
            self.onConnected!()
        }
    }
    
    func websocketDidDisconnect(socket: WebSocketClient, error: Error?) {
        NSLog("socket disconnected")
        if self.onDisconnected != nil {
            self.onDisconnected!()
        }
    }
    
    
    func websocketDidReceiveData(socket: WebSocketClient, data: Data) {
    }
    
    func websocketDidReceiveMessage(socket: WebSocketClient, text: String) {
        do {
            let jsonData = try JSONSerialization.jsonObject(with: text.data(using: .utf8)!, options: [])
            let response = jsonData as? [String: Any] ?? [:]
            let idResponse = response["id"] as? Int ?? -1
            let sid = response["sid"] as? String ?? ""
            let warning = response["warning"] as? [String: Any] ?? [:]
            if idResponse != -1 {
                NSLog("receive \(jsonData)")
                if let method = methodForRequestId[idResponse] {
                    let result = response["result"]
                    let error = response["error"] as? [String: Any] ?? [:]
                    if !error.isEmpty {
                        
                    } else {
                        handleResponse(method: method, result: result)
                    }
                    methodForRequestId.removeValue(forKey: idResponse)
                }
            } else if sid != "" {
                // this message has a sid (subscription id)
                // so this is some data from a data stream
                let time = response["time"] as? Double ?? 0
                var data: NSArray = []
                var stream: String = ""

                // find the data field inside the response
                for key in response.keys {
                    let value = response[key]
                    if key != "sid" && key != "time" && value is NSArray {
                        stream = key
                        data = value as? NSArray ?? []
                    }
                }
                if self.onStreamDataReceived != nil {
                    self.onStreamDataReceived!(sid, stream, time, data)
                }
            } else if !warning.isEmpty {
                NSLog("* warning \(text)")
            }
        } catch {
            print(error.localizedDescription)
        }
    }
}
