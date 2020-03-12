package com.emotiv.cortexv2example.datastream;

import com.emotiv.cortexv2example.objects.AccessObject;
import com.emotiv.cortexv2example.objects.HeadsetObject;
import com.emotiv.cortexv2example.objects.SessionObject;
import com.emotiv.cortexv2example.objects.TrainingObject;
import com.emotiv.cortexv2example.utils.Constant;
import org.json.JSONArray;
import org.json.JSONObject;

public class DataStream extends BaseStream {

    public static DataStream dataStream;
    AccessObject mAccessObject;
    HeadsetObject mHeadsetObject;
    SessionObject mSessionObject;
    TrainingObject mTrainingObject;

    public DataStream() {
        mAccessObject = AccessObject.getInstance();
        mHeadsetObject = HeadsetObject.getInstance();
        mSessionObject = SessionObject.getInstance();
        mTrainingObject = TrainingObject.getInstance();
    }

    public static DataStream getInstance() {
        if (dataStream == null) {
            dataStream = new DataStream();
        }
        return dataStream;
    }

    public static String getCortexToken() {
        return getInstance().getAccessObject().getCortexAccessToken();
    }


    public AccessObject getAccessObject() {
        return mAccessObject;
    }

    public HeadsetObject getHeadsetObject() {
        return mHeadsetObject;
    }

    public TrainingObject getmTrainingObject() {
        return mTrainingObject;
    }

    // handle the response to a RPC request
    public void parseStreamData(String jsonString, int requestType) {
        try {
            JSONObject jsonObj = new JSONObject(jsonString);
            if (jsonObj.has("result")) {
                if (requestType == Constant.GET_USER_LOGGED_IN_REQUEST_ID) {
                    JSONArray userNameArray = jsonObj.getJSONArray("result");
                    if (userNameArray.length() > 0) {
                        JSONObject userObj = userNameArray.getJSONObject(0);
                        mAccessObject.setEmotivID(userObj.getString("username"));
                        mAccessObject.setLogin(true);
                    }
                }
                else if (requestType == Constant.HAS_APP_ACCESS_REQUEST_ID) {
                    JSONObject accessGrantedJson = jsonObj.getJSONObject("result");
                    mAccessObject.setAccessGranted(accessGrantedJson.getBoolean("accessGranted"));
                }
                else if (requestType == Constant.AUTHORIZE_REQUEST_ID) {
                    JSONObject authorizeJson = jsonObj.getJSONObject("result");
                    mAccessObject.setCortexAccessToken(authorizeJson.getString("cortexToken"));
                }
                else if (requestType == Constant.GET_USER_INFORMATION_REQUEST_ID) {
                    JSONObject userInfoJson = jsonObj.getJSONObject("result");
                    mAccessObject.setFirstName(userInfoJson.getString("firstName"));
                    mAccessObject.setLastName(userInfoJson.getString("lastName"));
                    mAccessObject.setEmotivID(userInfoJson.getString("username"));
                }
                else if (requestType == Constant.QUERY_HEADSET_REQUEST_ID) {
                    JSONArray headsetListJsonArray = jsonObj.getJSONArray("result");
                    mHeadsetObject.clearData();
                    if (headsetListJsonArray.length() > 0) {
                        for (int i = 0; i < headsetListJsonArray.length(); i++) {
                            String headsetName = headsetListJsonArray.getJSONObject(i).getString("id");
                            String headsetConnectedBy = headsetListJsonArray.getJSONObject(i).getString("connectedBy");
                            String headsetStatus = headsetListJsonArray.getJSONObject(i).getString("status");

                            if (headsetStatus.equals("connected")) {
                                mHeadsetObject.setHeadsetStatus("connected");
                                mHeadsetObject.setHeadsetName(headsetName);
                                mHeadsetObject.setHeadsetConnectedBy(headsetConnectedBy);
                                mHeadsetObject.setConnected(true);
                            }

                            HeadsetObject headsetObject = new HeadsetObject();
                            headsetObject.setHeadsetName(headsetName);
                            headsetObject.setHeadsetConnectedBy(headsetConnectedBy);
                            headsetObject.setHeadsetStatus(headsetStatus);

                            mHeadsetObject.getHeadsetList().add(headsetObject);
                        }
                    }
                }
                else if (requestType == Constant.CREATE_SESSION_REQUEST_ID) {
                    JSONObject sessionObj = jsonObj.getJSONObject("result");
                    if (sessionObj.has("id"))
                        mSessionObject.setCurrentActivedSession(sessionObj.getString("id"));
                    if (sessionObj.has("status"))
                        mSessionObject.setCurrentSessonStatus(sessionObj.getString("status"));
                }
                else if (requestType == Constant.UPDATE_SESSION_REQUEST_ID) {
                    mSessionObject.clearData();
                }
                else if (requestType == Constant.CREATE_PROFILE_REQUEST_ID) {

                }
                else if (requestType == Constant.LOAD_PROFILE_REQUEST_ID) {
                    JSONObject trainingProfileObj = jsonObj.getJSONObject("result");
                    if (trainingProfileObj.has("name")) {
                        mTrainingObject.setCurrentProfile(trainingProfileObj.getString("name"));
                    }
                }
                else if (requestType == Constant.TRAINING_PROFILE_MC_REQUEST_ID) {
                    JSONObject trainingMCObj = jsonObj.getJSONObject("result");
                    mTrainingObject.setTrainingAction(trainingMCObj.getString("action"));
                }
                else if (requestType == Constant.TRAINING_PROFILE_FE_REQUEST_ID) {
                    JSONObject trainingMCObj = jsonObj.getJSONObject("result");
                    mTrainingObject.setTrainingAction(trainingMCObj.getString("action"));
                }
            }

        } catch (Exception e) { e.printStackTrace(); }
    }
}