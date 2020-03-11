//
//  AppDelegate.m
//  CortexV2-Emotiv-ObjC
//
//  Created by nvtu on 2/24/20.
//  Copyright © 2020 Emotiv. All rights reserved.
//

#import "AppDelegate.h"
#import "MarkerViewController.h"
#import "TrainingViewController.h"

@interface AppDelegate ()

@end

@implementation AppDelegate


- (BOOL)application:(UIApplication *)application didFinishLaunchingWithOptions:(NSDictionary *)launchOptions {
    // Override point for customization after application launch.
    
    NSString *path = [[NSBundle mainBundle] pathForResource:@"Info" ofType:@"plist"];
    if (path != nil) {
        NSDictionary * nsDictionary = [[NSDictionary alloc] initWithContentsOfFile:path];
        NSString *stream = nsDictionary[@"STREAM_NAME"];
        
        if (stream != nil) {
            self.window = [[UIWindow alloc] initWithFrame:[[UIScreen mainScreen] bounds]];
            BOOL activateSession = false;
            NSString *license = @"";
            if ([stream  isEqual: @"eeg"] || [stream  isEqual: @"marker"]) {
                NSLog(@"#");
                NSLog(@"#####");
                NSLog(@"Reminder: to subscribe to the EEG data stream, you must get an appropriate licence from Emotiv.");
                NSLog(@"#####");
                NSLog(@"#");
                license = @""; // you can put your license id here
                activateSession = true;
            }
            DataStreamViewController *dataVC;
            if ([stream isEqual: @"marker"]) {
                dataVC = [[MarkerViewController alloc] initActivateSession:activateSession license:license stream:@"eeg"];
            } else if ([stream isEqual: @"facialExpression"] || [stream isEqual: @"mentalCommand"]) {
                dataVC = [[TrainingViewController alloc] initActivateSession:activateSession license:license stream:stream];
            }
            else {
                dataVC = [[DataStreamViewController alloc] initActivateSession:activateSession license:license stream:stream];
            }
            [self.window setRootViewController:dataVC];
            [dataVC.view setFrame:CGRectMake(0, 0, [UIScreen mainScreen].bounds.size.width, [UIScreen mainScreen].bounds.size.height)];
            [self.window addSubview:dataVC.view];
            [self.window makeKeyAndVisible];
        }
    }

    return YES;
}


#pragma mark - UISceneSession lifecycle


- (UISceneConfiguration *)application:(UIApplication *)application configurationForConnectingSceneSession:(UISceneSession *)connectingSceneSession options:(UISceneConnectionOptions *)options {
    // Called when a new scene session is being created.
    // Use this method to select a configuration to create the new scene with.
    return [[UISceneConfiguration alloc] initWithName:@"Default Configuration" sessionRole:connectingSceneSession.role];
}


- (void)application:(UIApplication *)application didDiscardSceneSessions:(NSSet<UISceneSession *> *)sceneSessions {
    // Called when the user discards a scene session.
    // If any sessions were discarded while the application was not running, this will be called shortly after application:didFinishLaunchingWithOptions.
    // Use this method to release any resources that were specific to the discarded scenes, as they will not return.
}


@end
