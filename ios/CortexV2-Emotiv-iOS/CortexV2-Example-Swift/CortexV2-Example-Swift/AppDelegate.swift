//
//  AppDelegate.swift
//  CortexV2-Example-Swift
//
//  Created by nvtu on 2/21/20.
//  Copyright © 2020 Emotiv. All rights reserved.
//

import UIKit

@UIApplicationMain
class AppDelegate: UIResponder, UIApplicationDelegate {

    var window: UIWindow?
    
    func application(_ application: UIApplication, didFinishLaunchingWithOptions launchOptions: [UIApplication.LaunchOptionsKey: Any]?) -> Bool {
        // Override point for customization after application launch.
        
        if let path = Bundle.main.path(forResource: "Info", ofType: "plist") {
            let nsDictionary = NSDictionary(contentsOfFile: path)
            let stream = nsDictionary!["STREAM_NAME"] as? String ?? ""
            
            var view = DataStreamViewController()
            if stream == "facialExpression" || stream == "mentalCommand" {
                view = TrainingViewController()
            } else if stream == "marker" {
                view = MarkerViewController()
            }
            view.stream = stream
            
            if stream == "eeg" || stream == "marker" {
                NSLog("#")
                NSLog("#####")
                NSLog("Reminder: to subscribe to the EEG data stream, you must get an appropriate licence from Emotiv.")
                NSLog("#####")
                NSLog("#")
                view.stream = "eeg"
                view.license = "" // you can put your license id here
                view.activateSession = true
            }
            self.window?.rootViewController = view
        }
        return true
    }
}

