//-----------------------------------------------------------------------
// <copyright file="HelloARController.cs" company="Google">
//
// Copyright 2017 Google Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
//-----------------------------------------------------------------------

namespace GoogleARCore.HelloAR
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Rendering;
    using GoogleARCore;
    using UnityEngine.UI;
    using System.Collections;
    using UnityEngine.SceneManagement;

    /// <summary>
    /// Controlls the HelloAR example.
    /// </summary>
    public class HelloARController : MonoBehaviour
    {
        /// <summary>
        /// The first-person camera being used to render the passthrough camera.
        /// </summary>
        public Camera m_firstPersonCamera;

        /// <summary>
        /// A prefab for tracking and visualizing detected planes.
        /// </summary>
        public GameObject m_trackedPlanePrefab;

        /// <summary>
        /// A model to place when a raycast from a user touch hits a plane.
        /// </summary>
        public GameObject m_andyAndroidPrefab;

        public GameObject m_armChairPrefab;

        public GameObject m_sofaPrefab;

        public GameObject m_bedPrefab;

        private GameObject andyObject;


        // Becomes true when the user double taps on a model.
        private bool model_Selected_onScene = false;

        // Specific position on the scene.
        private Anchor anchor;

        // Becomes true when a object rendered on the scene.
        private bool objectRendered = false;

        /// <summary>
        /// A gameobject parenting UI for displaying the "searching for planes" snackbar.
        /// </summary>
        public GameObject m_searchingForPlaneUI;

        private List<TrackedPlane> m_newPlanes = new List<TrackedPlane>();

        private List<TrackedPlane> m_allPlanes = new List<TrackedPlane>();


        // Check
        private bool object_Selected_From_Menu = false;

        bool rotating = false;

        Vector2 startVector = new Vector2(0,0);

        float rotGestureWidth = 0;

        float rotAngleMinimum = 0;

        private Color[] m_planeColors = new Color[] {
            new Color(1.0f, 1.0f, 1.0f),
            new Color(0.956f, 0.262f, 0.211f),
            new Color(0.913f, 0.117f, 0.388f),
            new Color(0.611f, 0.152f, 0.654f),
            new Color(0.403f, 0.227f, 0.717f),
            new Color(0.247f, 0.317f, 0.709f),
            new Color(0.129f, 0.588f, 0.952f),
            new Color(0.011f, 0.662f, 0.956f),
            new Color(0f, 0.737f, 0.831f),
            new Color(0f, 0.588f, 0.533f),
            new Color(0.298f, 0.686f, 0.313f),
            new Color(0.545f, 0.764f, 0.290f),
            new Color(0.803f, 0.862f, 0.223f),
            new Color(1.0f, 0.921f, 0.231f),
            new Color(1.0f, 0.756f, 0.027f)
        };

        /// <summary>
        /// The Unity Update() method.
        /// </summary>
        /// 
       
        public void Update ()
        {

            _QuitOnConnectionErrors();

            TrackableHitFlag raycastFilter = TrackableHitFlag.PlaneWithinBounds | TrackableHitFlag.PlaneWithinPolygon;

            // The tracking state must be FrameTrackingState.Tracking in order to access the Frame.
            if (Frame.TrackingState != FrameTrackingState.Tracking)
            {
                const int LOST_TRACKING_SLEEP_TIMEOUT = 15;
                Screen.sleepTimeout = LOST_TRACKING_SLEEP_TIMEOUT;
                return;
            }

            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            Frame.GetNewPlanes(ref m_newPlanes);

            // Iterate over planes found in this frame and instantiate corresponding GameObjects to visualize them.
            for (int i = 0; i < m_newPlanes.Count; i++)
            {
                // Instantiate a plane visualization prefab and set it to track the new plane. The transform is set to
                // the origin with an identity rotation since the mesh for our prefab is updated in Unity World
                // coordinates.
                GameObject planeObject = Instantiate(m_trackedPlanePrefab, Vector3.zero, Quaternion.identity,
                    transform);
                planeObject.GetComponent<TrackedPlaneVisualizer>().SetTrackedPlane(m_newPlanes[i]);

                // Apply a random color and grid rotation.
                planeObject.GetComponent<Renderer>().material.SetColor("_GridColor", m_planeColors[Random.Range(0,
                    m_planeColors.Length - 1)]);
                planeObject.GetComponent<Renderer>().material.SetFloat("_UvRotation", Random.Range(0.0f, 360.0f));
            }

            // Disable the snackbar UI when no planes are valid.
            bool showSearchingUI = true;
            Frame.GetAllPlanes(ref m_allPlanes);
            for (int i = 0; i < m_allPlanes.Count; i++)
            {
                if (m_allPlanes[i].IsValid)
                {
                    showSearchingUI = false;
                    break;
                }
            }
            m_searchingForPlaneUI.SetActive(showSearchingUI);



            //For moving the 3d Model on the scene. After 3D Model has been Rendered and Selected.
            if(Input.touchCount == 1 && objectRendered)
            {
                TrackableHit myHit;
                Touch mytouch = Input.GetTouch(0);

                if(mytouch.phase == TouchPhase.Stationary || mytouch.phase == TouchPhase.Moved)
                {
                    // If Touch RayCast hits the tracked plane, get the co-ordinates and change the position of the 3d Model
                    if (Session.Raycast(m_firstPersonCamera.ScreenPointToRay(mytouch.position), raycastFilter, out myHit))
                    {
                        andyObject.transform.position = Vector3.Lerp(andyObject.transform.position, myHit.Point, Time.deltaTime);

                    }
                }
              

            }


            // When there are more than 2 3d Models on the scene. This code helps to select different models.
            if(Input.touchCount == 1 && !object_Selected_From_Menu && objectRendered)
            {
                RaycastHit myHit;
                Touch myTouch = Input.GetTouch(0);
                Ray ray = m_firstPersonCamera.ScreenPointToRay(myTouch.position);
                // When RayCasting, if it hits the collider and user taps twice then it becomes the new selected object.
                if (Physics.Raycast(ray, out myHit, Mathf.Infinity) && myTouch.tapCount == 2)
                {

                    _ShowAndroidToastMessage(myHit.collider.gameObject.name);
                    andyObject = myHit.collider.gameObject;
                    model_Selected_onScene = true;
                }

                // If touch Raycast hits the collider and the user taps thrice then it destroys the gameobject.
                if(Physics.Raycast(ray,out myHit,Mathf.Infinity) && myTouch.tapCount == 3)
                {
                    Destroy(myHit.collider.gameObject);
                }

                
            }


            // Rotating the 3d Model when two fingers is pressed
            if (Input.touchCount == 2)
            {
                if (!rotating)
                {
                    startVector = Input.GetTouch(1).position - Input.GetTouch(0).position;
                    rotating = startVector.sqrMagnitude > rotGestureWidth * rotGestureWidth;

                }

                else
                {
                    // Figuring out How to Calculate the Exact angle between two fingers when user performs the twist gesture.
                    var currVector = Input.GetTouch(1).position - Input.GetTouch(0).position;
                    var angleOffset = Vector2.Angle(startVector, currVector);
                    var LR = Vector3.Cross(startVector, currVector);

                    // Rotates the current selected object.
                    andyObject.transform.Rotate(0,2.5f,0);
                }
            }

            else
            {
                rotating = false;
            }

            TrackableHit hit;
            Touch touch;
            if (Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
            {
                return;
            }
            if (Session.Raycast(m_firstPersonCamera.ScreenPointToRay(touch.position), raycastFilter, out hit) && object_Selected_From_Menu)
            {
                objectRendered = true;
                object_Selected_From_Menu = false;
                // Create an anchor to allow ARCore to track the hitpoint as understanding of the physical
                // world evolves.
                 anchor = Session.CreateAnchor(hit.Point, Quaternion.identity);

                // Intanstiate an Andy Android object as a child of the anchor; it's transform will now benefit
                // from the anchor's tracking.
                andyObject = Instantiate(m_andyAndroidPrefab, hit.Point, Quaternion.identity,
                    anchor.transform);

                // Andy should look at the camera but still be flush with the plane.
                andyObject.transform.LookAt(m_firstPersonCamera.transform);
                andyObject.transform.rotation = Quaternion.Euler(0.0f,
                andyObject.transform.rotation.eulerAngles.y, andyObject.transform.rotation.z);
               
               
                // Use a plane attachment component to maintain Andy's y-offset from the plane
                // (occurs after anchor updates).
                andyObject.GetComponent<PlaneAttachment>().Attach(hit.Plane);

            }

            anchor.transform.Rotate(Vector3.right * Time.deltaTime, Space.World);

        }

        /// <summary>
        /// Quit the application if there was a connection error for the ARCore session.
        /// </summary>
        private void _QuitOnConnectionErrors()
        {
            // Do not update if ARCore is not tracking.
            if (Session.ConnectionState == SessionConnectionState.DeviceNotSupported)
            {
                _ShowAndroidToastMessage("This device does not support ARCore.");
                Application.Quit();
            }
            else if (Session.ConnectionState == SessionConnectionState.UserRejectedNeededPermission)
            {
                _ShowAndroidToastMessage("Camera permission is needed to run this application.");
                Application.Quit();
            }
            else if (Session.ConnectionState == SessionConnectionState.ConnectToServiceFailed)
            {
                _ShowAndroidToastMessage("ARCore encountered a problem connecting.  Please start the app again.");
                Application.Quit();
            }
        }

        /// <summary>
        /// Show an Android toast message.
        /// </summary>
        /// <param name="message">Message string to show in the toast.</param>
        /// <param name="length">Toast message time length.</param>
        private static void _ShowAndroidToastMessage(string message)
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            if (unityActivity != null)
            {
                AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
                unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                {
                    AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", unityActivity,
                        message, 0);
                    toastObject.Call("show");
                }));
            }
        }

        public void armChairSelected()
        {
            object_Selected_From_Menu = true;
            m_andyAndroidPrefab = m_armChairPrefab;
            _ShowAndroidToastMessage("Arm Chair Selected !");
        }

        public void sofaSelected()
        {
            object_Selected_From_Menu = true;
            m_andyAndroidPrefab = m_sofaPrefab;
            _ShowAndroidToastMessage("Couch Selected !");
        }

        public void bedSelected()
        {
            object_Selected_From_Menu = true;
            m_andyAndroidPrefab = m_bedPrefab;
            _ShowAndroidToastMessage("Bed Selected !");
        }

    }
}
