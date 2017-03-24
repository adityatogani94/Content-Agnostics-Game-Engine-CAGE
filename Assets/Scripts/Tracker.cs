using System;
using UnityEngine;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
/** Enum used for tracking status.
 */
public enum TrackStatus
{
    Off = 0,
    Ok = 1,
    Recovering = 2,
    Init = 3
}

public enum FaceEffect
{
    Glasses = 0,
    Tiger = 1
}


/** Class that implements the behaviour of tracking application.
 * 
 * This is the core class that shows how to use visage|SDK capabilities in Unity. It connects with visage|SDK through calls to native methods that are implemented in VisageTrackerUnityPlugin.
 * It uses tracking data to transform objects that are attached to it in ControllableObjects list.
 */

/*internal static class NativeMethods
{
    [DllImport("Plugin")]
    public static extern int MessageBox(int hWnd, string msg, string title, uint flags);
}*/

public class Tracker : MonoBehaviour
{
    #region Properties

    /** Tracker config file in editor.
	 */
#if !UNITY_EDITOR
	[HideInInspector]
#endif
    public string ConfigFileEditor = "Visage Tracker/Head Tracker.cfg";

    /** Tracker config file name on standalone platforms.
	 */
#if !UNITY_STANDALONE_WIN
	[HideInInspector]
#endif
    public string ConfigFileStandalone = "Visage Tracker/Head Tracker.cfg";

    /** Tracker config file on iOS.
	 */
#if !UNITY_IPHONE
    [HideInInspector]
#endif
    public string ConfigFileIOS;

    /** Tracker config file on Android.
	 */
#if !UNITY_ANDROID
    [HideInInspector]
#endif
    public string ConfigFileAndroid;

    /** Tracker config file on Mac.
	 */
#if !UNITY_STANDALONE_OSX
    [HideInInspector]
#endif
    public string ConfigFileOSX;

#if UNITY_ANDROID
	private AndroidJavaObject androidCameraActivity;
	[HideInInspector]
	public int ImWidth;
	
	[HideInInspector]
	public int ImHeight;
#endif

#if !UNITY_STANDALONE_WIN || !UNITY_EDITOR
	[HideInInspector]
#endif
    public string licenseFileFolder = "Visage Tracker/";

#if !UNITY_STANDALONE_OSX && !UNITY_ANDROID && !UNITY_IPHONE
    [HideInInspector]
#endif
    public string licenseFileName;

    public Transform[] ControllableObjects;
    Vector3[] startingPositions;
    Vector3[] startingRotations;
    //Mesh data
    public const int MaxVertices = 1024;
    public const int MaxTriangles = 2048;
    public int VertexNumber;
    public int TriangleNumber;
    public Vector2[] TexCoords;
    public Vector3[] Vertices;
    public int[] Triangles;
    private float[] vertices = new float[MaxVertices * 3];
    private int[] triangles = new int[MaxTriangles * 3];
    private float[] texCoords = new float[MaxVertices * 2];
    private MeshFilter meshFilter;
    public TextAsset TexCoordinatesFile;
    private Vector2[] modelTexCoords;

    //
    public Material CameraViewMaterial;
    public Vector3 Translation;
    public Vector3 Rotation;
    public float Focus;
    public int ImageWidth = 360;
    public int ImageHeight = 480;
    public int TexWidth = 512;
    public int TexHeight = 512;
    private Texture2D texture = null;
    private Color32[] texturePixels;
    private GCHandle texturePixelsHandle;
    public bool isTracking = false;
    public int TrackerStatus = 0;
    public int Orientation = 0;
    public int currentOrientation = 0;
    public int device = 0;
    public int currentDevice = 1;
    GUIContent contentSwitchCam = new GUIContent();
    GUIContent contentStartTracking = new GUIContent();
    GUIContent contentStopTracking = new GUIContent();
    GUIContent contentSwitchEffect = new GUIContent();
    public Texture2D imageSwitchCam;
    public Texture2D imageStartTracking;
    public Texture2D imageStopTracking;
    public Texture2D imageSwitchEffect;
    private bool trackButton;
    private bool stopTrackButton;
    private bool switchCamButton;
    private bool effectButton;
    private FaceEffect currentEffect = FaceEffect.Glasses;

    //private bool AppStarted = false;

    private GUIStyle startTrackingStyle = null;
    private GUIStyle stopTrackingStyle = null;
    private GUIStyle switchCamButtonStyle = null;
    private GUIStyle switchCamEffectStyle = null;

    #endregion

    void Start()
    {
        // switch cam image
        contentSwitchCam.image = imageSwitchCam;
        contentStartTracking.image = imageStartTracking;
        contentStopTracking.image = imageStopTracking;
        contentSwitchEffect.image = imageSwitchEffect;

        startTrackingStyle = new GUIStyle();
        startTrackingStyle.normal.background = (Texture2D)contentStartTracking.image;
        startTrackingStyle.active.background = (Texture2D)contentStartTracking.image;
        startTrackingStyle.hover.background = (Texture2D)contentStartTracking.image;

        stopTrackingStyle = new GUIStyle();
        stopTrackingStyle.normal.background = (Texture2D)contentStopTracking.image;
        stopTrackingStyle.active.background = (Texture2D)contentStopTracking.image;
        stopTrackingStyle.hover.background = (Texture2D)contentStopTracking.image;

        switchCamButtonStyle = new GUIStyle();
        switchCamButtonStyle.normal.background = (Texture2D)contentSwitchCam.image;
        switchCamButtonStyle.active.background = (Texture2D)contentSwitchCam.image;
        switchCamButtonStyle.hover.background = (Texture2D)contentSwitchCam.image;

        switchCamEffectStyle = new GUIStyle();
        switchCamEffectStyle.normal.background = (Texture2D)contentSwitchEffect.image;
        switchCamEffectStyle.active.background = (Texture2D)contentSwitchEffect.image;
        switchCamEffectStyle.hover.background = (Texture2D)contentSwitchEffect.image;

        // get mesh filter component
        meshFilter = GetComponent<MeshFilter>();

        // create a new mesh
        meshFilter.mesh = new Mesh();

        // get model texture coordinates
        modelTexCoords = GetTextureCoordinates();

        Translation = new Vector3(0, 0, 0);
        Rotation = new Vector3(0, 0, 0);

        // choose config file
        string configFilePath = Application.streamingAssetsPath + "/" + ConfigFileStandalone;
        string licenseFilePath = Application.streamingAssetsPath + "/" + licenseFileFolder;

        switch (Application.platform)
        {

            case RuntimePlatform.IPhonePlayer:
                configFilePath = "Data/Raw/Visage Tracker/" + ConfigFileIOS;
                licenseFilePath = "Data/Raw/Visage Tracker/" + licenseFileName;
                break;
            case RuntimePlatform.Android:
                configFilePath = Application.persistentDataPath + "/" + ConfigFileAndroid;
                licenseFilePath = Application.persistentDataPath + "/" + licenseFileName;
                break;
            case RuntimePlatform.OSXPlayer:
                configFilePath = Application.dataPath + "/Resources/Data/StreamingAssets/Visage Tracker/" + ConfigFileOSX;
                licenseFilePath = Application.dataPath + "/Resources/Data/StreamingAssets/Visage Tracker/" + licenseFileName;
                break;
            case RuntimePlatform.OSXEditor:
                configFilePath = Application.dataPath + "/StreamingAssets/Visage Tracker/" + ConfigFileOSX;
                licenseFilePath = Application.dataPath + "/StreamingAssets/Visage Tracker/" + licenseFileName;
                break;
            case RuntimePlatform.WindowsEditor:
                configFilePath = Application.streamingAssetsPath + "/" + ConfigFileEditor;
                licenseFilePath = Application.streamingAssetsPath + "/" + licenseFileFolder;
                break;
        }

        // initialize tracker
        InitializeTracker(configFilePath, licenseFilePath);

        // check orientation and start cam
        Orientation = GetDeviceOrientation();

        OpenCamera(Orientation, device);

        startingPositions = new Vector3[ControllableObjects.Length];
        startingRotations = new Vector3[ControllableObjects.Length];

        for (int i = 0; i < ControllableObjects.Length; i++)
        {
            startingPositions[i] = ControllableObjects[i].transform.position;
            startingRotations[i] = ControllableObjects[i].transform.rotation.eulerAngles;
        }

        isTracking = true;

        if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.OpenGLCore)
            Debug.Log("Notice: if graphics API is set to OpenGLCore, the texture might not get properly updated.");

    }

    void Update()
    {
#if (UNITY_IPHONE || UNITY_ANDROID) && UNITY_EDITOR
		// no tracking on ios while in editor
		return;
#endif
        if (TrackerStatus != 0)
        {

            // find device orientation
            currentOrientation = GetDeviceOrientation();

            // check if orientation or camera device changed
            if (currentOrientation != Orientation || currentDevice != device)
            {
                OpenCamera(currentOrientation, currentDevice);
                Orientation = currentOrientation;
                device = currentDevice;
                texture = null;
            }

            // grab current frame and start face tracking

            VisageTrackerNative._grabFrame();

            TrackerStatus = VisageTrackerNative._track();

            // update tracker status and translation and rotation
            VisageTrackerNative._get3DData(out Translation.x, out Translation.y, out Translation.z, out Rotation.x, out Rotation.y, out Rotation.z);


            for (int i = 0; i < ControllableObjects.Length; i++)
            {
                ControllableObjects[i].transform.position = startingPositions[i] + Translation;
                ControllableObjects[i].transform.rotation = Quaternion.Euler(startingRotations[i] + Rotation);
            }

            transform.position = Translation;
            transform.rotation = Quaternion.Euler(Rotation);

            VisageTrackerNative._getCameraInfo(out Focus, out ImageWidth, out ImageHeight);

            float aspect = ImageWidth / (float)ImageHeight;

            float yRange = (ImageWidth > ImageHeight) ? 1.0f : 1.0f / aspect;

            Camera.main.fieldOfView = Mathf.Rad2Deg * 2.0f * Mathf.Atan(yRange / Focus);

            VisageTrackerNative._getFaceModel(out VertexNumber, vertices, out TriangleNumber, triangles, texCoords);

            // vertices
            if (Vertices.Length != VertexNumber)
                Vertices = new Vector3[VertexNumber];

            for (int i = 0; i < VertexNumber; i++)
            {
                Vertices[i] = new Vector3(vertices[i * 3 + 0], vertices[i * 3 + 1], vertices[i * 3 + 2]);
            }

            // triangles
            if (Triangles.Length != TriangleNumber)
                Triangles = new int[TriangleNumber * 3];

            for (int i = 0; i < TriangleNumber * 3; i++)
            {
                Triangles[i] = triangles[i];
            }

            // tex coords
            if (TexCoords.Length != VertexNumber)
                TexCoords = new Vector2[VertexNumber];

            for (int i = 0; i < VertexNumber; i++)
            {
                TexCoords[i] = new Vector2(modelTexCoords[i].x, modelTexCoords[i].y); //new Vector2 (texCoords [i * 2 + 0], texCoords [i * 2 + 1]);
            }

        }
        else
        {
            VisageTrackerNative._grabFrame();

            TrackerStatus = VisageTrackerNative._track();
        }

        RefreshImage();

        // create mesh

        meshFilter.mesh.Clear();
        if (currentEffect == FaceEffect.Tiger)
        {

            for (int i = 0; i < ControllableObjects.Length; i++)
            {
                ControllableObjects[i].transform.position -= new Vector3(0, 0, 10000);

            }

            meshFilter.mesh.vertices = Vertices;
            meshFilter.mesh.triangles = Triangles;
            meshFilter.mesh.uv = TexCoords;
            meshFilter.mesh.uv2 = TexCoords;
            ;
            meshFilter.mesh.RecalculateNormals();
            meshFilter.mesh.RecalculateBounds();
        }
    }


    /** This method initializes the tracker.
	 */
    bool InitializeTracker(string config, string license)
    {
        Debug.Log("Visage Tracker: Initializing tracker with config: '" + config + "'");

#if (UNITY_IPHONE || UNITY_ANDROID) && UNITY_EDITOR
		return false;
#endif

#if UNITY_ANDROID

		Shader shader = Shader.Find("Unlit/Texture");
		CameraViewMaterial.shader = shader;

		// initialize visage vision
		VisageTrackerNative._loadVisageVision();
		Unzip();

		AndroidJavaClass unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		this.androidCameraActivity = unity.GetStatic<AndroidJavaObject>("currentActivity");
#else
        Shader shader = Shader.Find("Custom/BGRATex");
        CameraViewMaterial.shader = shader;
#endif
        // initialize tracker
        VisageTrackerNative._initTracker(config, license);

        return true;
    }


    // returns current device orientation
    int GetDeviceOrientation()
    {
        int devOrientation;

        //Device orientation is obtained in AndroidCameraPlugin so we only need information about whether orientation is changed
#if UNITY_ANDROID
		int oldWidth = ImageWidth;
		int oldHeight = ImageHeight;
		VisageTrackerNative._getCameraInfo(out Focus, out ImageWidth, out ImageHeight);

		if ((oldWidth!=ImageWidth || oldHeight!=ImageHeight) && ImageWidth != 0 && ImageHeight !=0 && oldWidth != 0 && oldHeight !=0 )
			devOrientation = (Orientation ==1) ? 0:1;
		else
			devOrientation = Orientation;
#else

        if (Input.deviceOrientation == DeviceOrientation.Portrait)
            devOrientation = 0;
        else if (Input.deviceOrientation == DeviceOrientation.PortraitUpsideDown)
            devOrientation = 2;
        else if (Input.deviceOrientation == DeviceOrientation.LandscapeLeft)
            devOrientation = 3;
        else if (Input.deviceOrientation == DeviceOrientation.LandscapeRight)
            devOrientation = 1;
        else if (Input.deviceOrientation == DeviceOrientation.FaceUp)
            devOrientation = Orientation;
        else
            devOrientation = 0;
#endif

        return devOrientation;
    }

    void OpenCamera(int orientation, int currDevice)
    {
#if UNITY_ANDROID
		if (device == currDevice && AppStarted)
			return;
		this.androidCameraActivity.Call("closeCamera");
		ImWidth = 320;
		ImHeight = 240;

		this.androidCameraActivity.Call("GrabFromCamera", ImWidth, ImHeight, currDevice);
		AppStarted = true;
#else
        VisageTrackerNative._openCamera(orientation, currDevice);
#endif

    }

    void RefreshImage()
    {
        // create texture
        if (texture == null && isTracking)
        {
            TexWidth = Convert.ToInt32(Math.Pow(2.0, Math.Ceiling(Math.Log(ImageWidth) / Math.Log(2.0))));
            TexHeight = Convert.ToInt32(Math.Pow(2.0, Math.Ceiling(Math.Log(ImageHeight) / Math.Log(2.0))));
#if UNITY_ANDROID
			texture = new Texture2D (TexWidth, TexHeight, TextureFormat.RGB24, false);
#else
            texture = new Texture2D(TexWidth, TexHeight, TextureFormat.RGBA32, false);
#endif

            var cols = texture.GetPixels32();
            for (var i = 0; i < cols.Length; i++)
                cols[i] = UnityEngine.Color.black;

            texture.SetPixels32(cols);
            texture.Apply(false);



#if UNITY_STANDALONE_WIN
            // "pin" the pixel array in memory, so we can pass direct pointer to it's data to the plugin,
            // without costly marshaling of array of structures.
            texturePixels = texture.GetPixels32(0);
            texturePixelsHandle = GCHandle.Alloc(texturePixels, GCHandleType.Pinned);
#endif

        }

        if (texture != null && isTracking && TrackerStatus != 0)
        {

#if UNITY_STANDALONE_WIN
            CameraViewMaterial.SetTexture("_MainTex", texture);

            // send memory address of textures' pixel data to VisageTrackerUnityPlugin
            VisageTrackerNative._setFrameData(texturePixelsHandle.AddrOfPinnedObject());
            texture.SetPixels32(texturePixels, 0);
            texture.Apply();



#elif UNITY_IPHONE || UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX || UNITY_ANDROID
			CameraViewMaterial.SetTexture ("_MainTex", texture);
			

			if (SystemInfo.graphicsDeviceVersion.StartsWith ("Metal"))
				VisageTrackerNative._bindTextureMetal (texture.GetNativeTexturePtr ());
			else
				VisageTrackerNative._bindTexture ((int)texture.GetNativeTexturePtr ());
			
#endif
        }
    }

    void Unzip()
    {
        string[] pathsNeeded = {
            "candide3.fdp",
            "candide3.wfm",
            "jk_300.fdp",
            "jk_300.wfm",
            "Head Tracker.cfg",
            "visage_powered.png",
            "warning.png",
            "bdtsdata/FF/ff.dat",
            "bdtsdata/NN/fa.lbf",
            "bdtsdata/NN/fc.lbf",
            "bdtsdata/LBF/pe/landmarks.txt",
            "bdtsdata/LBF/pe/W",
            "bdtsdata/LBF/pe/lp11.bdf",
            "bdtsdata/LBF/ye/lp11.bdf",
            "bdtsdata/LBF/ye/W",
            "bdtsdata/LBF/ye/landmarks.txt"
            , "license-file-name.vlc"
        };
        string outputDir;
        string localDataFolder = "Visage Tracker";

        //StreamWriter sw;

        outputDir = Application.persistentDataPath;

        if (!Directory.Exists(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }
        foreach (string filename in pathsNeeded)
        {
            //if(!File.Exists(outputDir + "/" + filename))
            //{

            WWW unpacker = new WWW("jar:file://" + Application.dataPath + "!/assets/" + localDataFolder + "/" + filename);

            while (!unpacker.isDone) { }

            if (!string.IsNullOrEmpty(unpacker.error))
            {
                Debug.Log(unpacker.error);
                continue;
            }

            Debug.Log(filename);

            if (filename.Contains("/"))
            {
                string[] split = filename.Split('/');
                string name = "";
                string folder = "";
                string curDir = outputDir;

                for (int i = 0; i < split.Length; i++)
                {
                    if (i == split.Length - 1)
                    {
                        name = split[i];
                    }
                    else
                    {
                        folder = split[i];
                        curDir = curDir + "/" + folder;
                    }
                }
                if (!Directory.Exists(curDir))
                {
                    Directory.CreateDirectory(curDir);
                }

                File.WriteAllBytes("/" + curDir + "/" + name, unpacker.bytes);
            }
            else
            {
                File.WriteAllBytes("/" + outputDir + "/" + filename, unpacker.bytes);
            }

            Debug.Log("File written " + filename + "\n");
        }
    }


    // .obj file parsing (normals excluded)
    Vector2[] GetTextureCoordinates()
    {
        List<Vector3> v = new List<Vector3>();
        List<Vector2> vt = new List<Vector2>();
        Vector2[] texCoords = new Vector2[1024];
        int indexPoint;
        int texIndexPoint;

        string text = TexCoordinatesFile.text;
        string[] lines = text.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

        foreach (string line in lines)
        {
            string[] lineSplit = line.Split(' ');

            // ex. "v -22.571495 -26.737658 38.676575"             
            if (lineSplit[0] == "v")
            {
                v.Add(new Vector3((float)double.Parse(lineSplit[1]), float.Parse(lineSplit[2]), float.Parse(lineSplit[3])));
            }

            // ex. "vt 0.5465 0.2624"
            else if (lineSplit[0] == "vt")
            {
                vt.Add(new Vector2(float.Parse(lineSplit[1]), float.Parse(lineSplit[2])));
            }

            // ex. "f 410/410/413 399/399/413 63/60/413"
            else if (lineSplit[0] == "f")
            {
                int[] indexPoints = { 0, 0, 0 };
                int[] texIndexPoints = { 0, 0, 0 };
                for (int i = 1; i < 4; i++)
                {
                    indexPoints[i - 1] = int.Parse(lineSplit[i].Split('/')[0]);
                    texIndexPoints[i - 1] = int.Parse(lineSplit[i].Split('/')[1]);
                }

                for (int i = 0; i < 3; i++)
                {
                    indexPoint = indexPoints[i];
                    texIndexPoint = texIndexPoints[i];

                    texCoords[indexPoint - 1].x = vt[texIndexPoint - 1].x;
                    texCoords[indexPoint - 1].y = vt[texIndexPoint - 1].y;
                }

            }
        }

        return texCoords;
    }

}
