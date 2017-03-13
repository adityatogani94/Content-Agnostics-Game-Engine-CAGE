using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System;

public static partial class VisageTrackerNative
{
	#if (UNITY_EDITOR_OSX && !UNITY_IOS) || UNITY_STANDALONE_OSX

	/** This function initialises the tracker.
 	* 
 	* Implemented in VisageTrackerUnityPlugin library.
 	*/
	[DllImport("VisageTrackerUnityPlugin")]
	public static extern void _initTracker(string config, string license);

	/** This functions binds a texture with the given native hardware texture id through OpenGL.
     * 
     * Implemented in VisageTrackerUnityPlugin library.
     */
	[DllImport("VisageTrackerUnityPlugin")]
	public static extern void _bindTexture(int texID);

	/** This functions binds a texture with the given native hardware texture id through metal.
     * 
     * Implemented in VisageTrackerUnityPlugin library.
     */
	[DllImport ("VisageTrackerUnityPlugin")]
	public static extern void _bindTextureMetal (IntPtr texID);

	/** This functions returns the current head translation, rotation and tracking status.
    * 
    * Implemented in VisageTrackerUnityPlugin library.
    */
	[DllImport("VisageTrackerUnityPlugin")]
	public static extern void _get3DData(out float tx, out float ty, out float tz, out float rx, out float ry, out float rz);

	/** This functions returns camera info.
    * 
    * Implemented in VisageTrackerUnityPlugin library.
    */
	[DllImport("VisageTrackerUnityPlugin")]
	public static extern bool _getCameraInfo(out float focus, out int width, out int height);

	/** This function starts face tracking on current frame and returns tracker status.
     * 
     * Implemented in VisageTrackerUnity library.
     */
	[DllImport ("VisageTrackerUnityPlugin")]
	public static extern int _track();

	/** This function grabs current frame.
     * 
     * Implemented in VisageTrackerUnity library.
     */
	[DllImport ("VisageTrackerUnityPlugin")]
	public static extern void _grabFrame();

	/** This function initializes new camera with the given orientation and camera information.
     * 
     * Implemented in VisageTrackerUnity library.
     */
	[DllImport ("VisageTrackerUnityPlugin")]
	public static extern void _openCamera(int orientation, int device, int width, int height, int mirrored);

	/** This function closes camera.
     * 
     * Implemented in VisageTrackerUnity library.
     */
	[DllImport ("VisageTrackerUnityPlugin")]
	public static extern void _closeCamera();

	/** This functions returns data needed to draw 3D face model.
     * 
     * Implemented in VisageTrackerUnity library.
     */
	[DllImport ("VisageTrackerUnityPlugin")]
	public static extern bool _getFaceModel (
		out int vertexNumber,
		[MarshalAs(UnmanagedType.LPArray, SizeConst = 1024 * 3)]
		float[] vertices,
		out int triangleNumber,
		[MarshalAs(UnmanagedType.LPArray, SizeConst = 2048 * 3)]
		int[] triangles,
		[MarshalAs(UnmanagedType.LPArray, SizeConst = 1024 * 2)]
		float[] texCoords
	);
#endif

}






	