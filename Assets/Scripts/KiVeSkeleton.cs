﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bone{
    private Transform startJoint = null;
    private Transform endJoint = null;
    private GameObject bone = null;

    public GameObject getBone(){return bone;}

    public Bone(Transform start, Transform end){
        startJoint = start;
        endJoint = end;
        bone = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        bone.name = "PlayerBone";
        bone.tag = "Player";
        bone.transform.localScale = new Vector3(0.1f, 1f, 0.1f);

        Rigidbody tempRigidBody = bone.AddComponent<Rigidbody>();
        tempRigidBody.useGravity = false;
        tempRigidBody.isKinematic = false;
        bone.GetComponent<CapsuleCollider>().isTrigger = true;
    }

    public void update(){
        bone.transform.position = (startJoint.position + endJoint.position )/2;
        bone.transform.up = startJoint.position - endJoint.position;
        bone.transform.localScale = new Vector3(bone.transform.localScale.x, 
            Vector3.Distance(startJoint.position, endJoint.position)/2,
            bone.transform.localScale.z);
        /*bone.transform.position = endJoint.position - startJoint.position;
        bone.transform.LookAt(endJoint.position);*/
    }
}

// TODO find solution : bug with miror : double all the elements
// https://answers.unity.com/questions/1534621/vr-how-to-render-gameobject-in-one-eye-only.html
// https://assetstore.unity.com/packages/vfx/shaders/fullscreen-camera-effects/vr-eye-shaders-88499?_ga=2.228974492.1618207112.1581084038-513779130.1578677382

/****************
Joints structure :
    0
    |
  2_1_5
 /|   |\
3 |   | 6
| |   | |
4 |   | 7
  8___11
  |   |
  9   12
  |   |
  10  13

Bones structure :

     *
     0
  *1_*_2*
 3 |   | 6
*  7   10 *
3  |   |  6
*  |   |  *
   *13_*  
   8   11
   *   *
   9   12  

****************/
public class KiVeSkeleton : MonoBehaviour
{
    private Transform headset = null;
    private Transform leftHand = null;
    private Transform rightHand = null;
    //private KinectVRPN kinect = null;

    const int HEAD = 0, NECK = 1, RIGHT_SHOULDER = 2, RIGHT_ELBOW = 3, 
        RIGHT_HAND=4, LEFT_SHOULDER = 5, LEFT_ELBOW = 6, LEFT_HAND = 7,
        RIGHT_HIPS = 8, RIGHT_KNEE = 9, RIGHT_FOOT = 10, LEFT_HIPS = 11,
        LEFT_KNEE = 12, LEFT_FOOT = 13;

    private List<GameObject> joints = new List<GameObject>();
    private List<Bone> bones = new List<Bone>();

    private GameObject skeletonData = null;
    private SkeletonContainer kinectSkeleton = null;

    private Calibrator calibrator = new Calibrator();

    private float kinectSkeletonHeight = 0.0f;
    private void getVRComponent(){
        // Search and hopefully find the CameraRig component of SteamVR plugin (the component should be at root of scene)
        Transform cameraRig = GameObject.Find("/[CameraRig]").transform;

        if(cameraRig == null){
            Debug.LogError("Can't find CameraRig (of the steamVR plugin)");
        } else {
            Debug.Log("CameraRig of steamVR found");
            headset = cameraRig.Find("Camera");
            leftHand = cameraRig.Find("Controller (left)");
            rightHand = cameraRig.Find("Controller (right)");
            if(leftHand == null){Debug.LogError("No left controller found");}
            if(rightHand == null){Debug.LogError("No right controller found");}
        }
    }

    // TODO
    private void getKinectVRPN(){
        skeletonData = GameObject.Find("/SkeletonData(Clone)");
        if(skeletonData !=null){
            Debug.Log("ON A LE SQUELETTE !");
            kinectSkeleton = skeletonData.GetComponent<SkeletonContainer>();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Rigidbody tempRigidBody;

        getVRComponent();   
        //getKinectVRPN();

        for(int i=0;i<14;++i){
            joints.Add(GameObject.CreatePrimitive(PrimitiveType.Sphere));
            joints[i].transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            tempRigidBody = joints[i].AddComponent<Rigidbody>();
            tempRigidBody.useGravity = false;
            tempRigidBody.isKinematic = false;
            string header = "Player";
            switch(i){
                case HEAD:              {joints[i].name = header + "Head";           break;}
                case NECK:              {joints[i].name = header + "Neck";           break;}
                case RIGHT_SHOULDER:    {joints[i].name = header + "Right Shoulder"; break;}
                case RIGHT_ELBOW:       {joints[i].name = header + "Right Elbow";    break;}
                case RIGHT_HAND:        {joints[i].name = header + "Right Hand";     break;}
                case LEFT_SHOULDER:     {joints[i].name = header + "Left Shoulder";  break;}
                case LEFT_ELBOW:        {joints[i].name = header + "Left Elbow";     break;}
                case LEFT_HAND:         {joints[i].name = header + "Left Hand";      break;}
                case RIGHT_HIPS:        {joints[i].name = header + "Right Hip";      break;}
                case RIGHT_KNEE:        {joints[i].name = header + "Right Knee";     break;}
                case RIGHT_FOOT:        {joints[i].name = header + "Right Foot";     break;}
                case LEFT_HIPS:         {joints[i].name = header + "Left Hip";       break;}
                case LEFT_KNEE:         {joints[i].name = header + "Left Knee";      break;}
                case LEFT_FOOT:         {joints[i].name = header + "Left Foot";      break;}
                default : break;
            }
        }
        joints[HEAD].GetComponent<MeshRenderer>().enabled = false;
        

        for(int i=0;i<14;++i){
            if(i==4){
                bones.Add(new Bone(joints[1].transform, joints[i+1].transform));
            } else if(i==7){
                bones.Add(new Bone(joints[2].transform, joints[i+1].transform));
            } else if(i==10){
                bones.Add(new Bone(joints[5].transform, joints[i+1].transform));
            } else if(i==13){
                bones.Add(new Bone(joints[8].transform, joints[11].transform));
            } else {
                bones.Add(new Bone(joints[i].transform, joints[i+1].transform));
            }
        }
        bones[0].getBone().GetComponent<MeshRenderer>().enabled = false;
    }

    public Transform getHeadset(){
        return headset;
    }

    public Transform getLeftHand(){
        return leftHand;
    }

    public Transform getRightHand(){
        return rightHand;
    }

    private void asICanSkeletonUpdate(){

            Transform cameraRig = GameObject.Find("/[CameraRig]").transform;
            Vector3 playerPos = headset.position;
            float playerHeigth = headset.localPosition.y;
            joints[LEFT_FOOT].transform.position        = playerPos + new Vector3(0.2f, -playerHeigth, 0);
            joints[RIGHT_FOOT].transform.position       = playerPos + new Vector3(-0.2f, -playerHeigth, 0);
            joints[LEFT_KNEE].transform.position        = playerPos + new Vector3(0.2f, -3*playerHeigth/4, 0);
            joints[RIGHT_KNEE].transform.position       = playerPos + new Vector3(-0.2f, -3*playerHeigth/4, 0);
            joints[LEFT_HIPS].transform.position        = playerPos + new Vector3(0.2f, -2*playerHeigth/4, 0);
            joints[RIGHT_HIPS].transform.position       = playerPos + new Vector3(-0.2f, -2*playerHeigth/4, 0);
            joints[LEFT_SHOULDER].transform.position    = playerPos + new Vector3(0.2f, -playerHeigth/4, 0);
            joints[RIGHT_SHOULDER].transform.position   = playerPos + new Vector3(-0.2f, -playerHeigth/4, 0);
            joints[NECK].transform.position             = playerPos + new Vector3(0, -playerHeigth/4, 0);
            joints[LEFT_ELBOW].transform.position       = (leftHand.position + joints[LEFT_SHOULDER].transform.position)/2;
            joints[RIGHT_ELBOW].transform.position      = (rightHand.position + joints[RIGHT_SHOULDER].transform.position)/2;

        
    }

    public void kinectSkeletonUpdate() {
        List<Kine_joint> jointsKinect = kinectSkeleton.GetJoints();
        Debug.Log(jointsKinect.Count);
        Vector3 leftCtrl    = joints[LEFT_HAND].transform.position, 
            rightCtrl       = joints[RIGHT_HAND].transform.position,
            leftHand        = jointsKinect[LEFT_HAND].position,
            rightHand       = jointsKinect[RIGHT_HAND].position,
            head            = jointsKinect[HEAD].position;
        Transform headsetT = getHeadset();
        
        if(Vector3.Distance(jointsKinect[HEAD].position, jointsKinect[LEFT_FOOT].position) > kinectSkeletonHeight )
            kinectSkeletonHeight = Vector3.Distance(jointsKinect[HEAD].position, jointsKinect[LEFT_FOOT].position);

        if(Vector3.Distance(jointsKinect[HEAD].position, jointsKinect[RIGHT_FOOT].position) > kinectSkeletonHeight )
            kinectSkeletonHeight = Vector3.Distance(jointsKinect[HEAD].position, jointsKinect[RIGHT_FOOT].position);

        calibrator.computeKive(leftCtrl,  rightCtrl,  headsetT,
          leftHand,  rightHand,  head, kinectSkeletonHeight);

        Debug.Log("Kinect skeleton height : " + kinectSkeletonHeight);
        Debug.Log("Kive Matrix : " + calibrator.kiveMat.ToString());

        for(int i=0;i<jointsKinect.Count;++i){
            if(i!=HEAD && i!=RIGHT_HAND && i!=LEFT_HAND) {
                // joints[i].transform.position = calibrator.kinect2VivePos(jointsKinect[i].position);
                joints[i].transform.position = calibrator.getViveJoint(jointsKinect[i].position);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(headset == null || leftHand == null || rightHand == null) {
            getVRComponent();
        }
        if(skeletonData == null){getKinectVRPN();}


        // Update head, left and right hand (basic known position)
        joints[HEAD].transform.position = headset.position;  
        joints[LEFT_HAND].transform.position = leftHand.position;  
        joints[RIGHT_HAND].transform.position = rightHand.position;

        if(skeletonData == null){
            asICanSkeletonUpdate();
        } else {
            kinectSkeletonUpdate();
        }

        // Bones update
        for(int i=0;i<14;++i){
            bones[i].update();
        }
        
    }
}
