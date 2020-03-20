//
//  CortexClient.h
//  CortexV2-Emotiv-ObjC
//
//  Created by nvtu on 2/24/20.
//  Copyright © 2020 Emotiv. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <Network/Network.h>

#import "Headset.h"
NS_ASSUME_NONNULL_BEGIN

@interface CortexClient : NSObject<NSURLSessionWebSocketDelegate> {
@private
    int nextRequestId;
    NSMutableDictionary* methodForRequestId;
    NSURLSessionWebSocketTask *webSocket;
}
@property (nonatomic, copy) void (^onConnected)(void);
@property (nonatomic, copy) void (^onDisconnected)(void);
@property (nonatomic, copy) void (^onQueryHeadsetsOk)(NSArray*);
@property (nonatomic, copy) void (^onGetUserLoginOk)(NSString*);
@property (nonatomic, copy) void (^onRequestAccessOk)(BOOL, NSString*);
@property (nonatomic, copy) void (^onHasAccessRightOk)(BOOL, NSString*);
@property (nonatomic, copy) void (^onAuthorizeOk)(NSString*);
@property (nonatomic, copy) void (^onCreateSessionOk)(NSString*);
@property (nonatomic, copy) void (^onCloseSessionOk)(void);
@property (nonatomic, copy) void (^onSubscribeOk)(NSArray*);
@property (nonatomic, copy) void (^onUnsubscribeOk)(NSArray*);
@property (nonatomic, copy) void (^onQueryProfileOk)(NSArray*);
@property (nonatomic, copy) void (^onCreateProfileOk)(NSString*);
@property (nonatomic, copy) void (^onLoadProfileOk)(NSString*);
@property (nonatomic, copy) void (^onSaveProfileOk)(NSString*);
@property (nonatomic, copy) void (^onGetDetectionInfoOk)(NSArray*, NSArray*, NSArray*);
@property (nonatomic, copy) void (^onTrainingOk)(NSString*);
@property (nonatomic, copy) void (^onCreateRecordOk)(NSString*);
@property (nonatomic, copy) void (^onStopRecordOk)(NSString*);
@property (nonatomic, copy) void (^onGetRecordInfosOk)(NSDictionary*);
@property (nonatomic, copy) void (^onInjectMarkerOk)(NSString*);
@property (nonatomic, copy) void (^onUpdateMarkerOk)(void);

@property (nonatomic, copy) void (^onErrorReceived)(NSString*, int, NSString*);
@property (nonatomic, copy) void (^onStreamDataReceived)(NSString*, NSString*, double, NSArray*);

+(id) shareInstance;
-(void) initSocketState;

-(void) open;
-(void) close;
-(void) queryHeadset: (NSString *) headsetId;
-(void) getUserLogin;
-(void) requestAccess: (NSString *) clientId secret: (NSString *) clientSecret;
-(void) hasAccessRight: (NSString *) clientId secret: (NSString *) clientSecret;
-(void) authorize: (NSString *) clientId secret: (NSString *) clientSecret license: (NSString *) license debit: (int) debit;
-(void) createSession: (NSString *) token headsetId: (NSString *) headsetId activate: (BOOL) activate;
-(void) closeSession: (NSString *) token sessionId: (NSString *) sessionId;
-(void) subscribe: (NSString *) token sessionId: (NSString *) sessionId stream: (NSString *) stream;
-(void) unsubscribe: (NSString *) token sessionId: (NSString *) sessionId stream: (NSString *) stream;
-(void) queryProfile: (NSString *) token;
-(void) getUserInformation: (NSString *) token;
-(void) getLicenseInfos: (NSString *) token;
-(void) createProfile: (NSString *) token profileName: (NSString *) profileName;
-(void) loadProfile: (NSString *) token headsetId: (NSString *) headsetId profileName: (NSString *) profileName;
-(void) saveProfile: (NSString *) token headsetId: (NSString *) headsetId profileName: (NSString *) profileName;
-(void) getDetectionInfo: (NSString *) detection;
-(void) training: (NSString *) token sessionId: (NSString *) sessionId detection: (NSString *) detection action: (NSString *) action control: (NSString *) control;
-(void) createRecord: (NSString *) token sessionId: (NSString *) sessionId title: (NSString *) title;
-(void) stopRecord: (NSString *) token sessionId: (NSString *) sessionId;
-(void) getRecordInfos: (NSString *) token recordId: (NSString *) recordId;
-(void) injectMarker: (NSString *) token sessionId: (NSString *) sessionId label: (NSString *) label value: (int) value time: (int64_t) time;
-(void) updateMarker: (NSString *) token sessionId: (NSString *) sessionId markerId: (NSString *) markerId time: (int64_t) time;

-(void) sendRequest: (NSString *) method params: (NSDictionary *) params;
-(void) handleResponse: (NSString *) data;
-(void) handleResponse: (NSString *)  method result: (id) result;
-(void) emitError: (NSString *) method error: (NSDictionary *) error;
-(void) handleGetDetectionInfo: (NSDictionary *) result;
-(NSArray *) parseSubscriptionResult: (NSString *) method result: (NSDictionary *) result;

@end

NS_ASSUME_NONNULL_END
