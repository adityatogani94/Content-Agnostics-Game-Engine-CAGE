using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

public static partial class VisageTrackerNative
{
#if UNITY_ANDROID
	[DllImport ("VisageVision")]
	public static extern void _loadVisageVision();
	
	[DllImport ("VisageTrackerUnityPlugin")]
	public static extern void _initTracker(string config, string license);
	

	[DllImport ("VisageTrackerUnityPlugin")]
	public static extern string _get3DData(out float tx, out float ty, out float tz, out float rx, out float ry, out float rz);
	
	[DllImport ("VisageTrackerUnityPlugin")]
	public static extern void _getCameraInfo(out float focus, out int ImageWidth, out int ImageHeight);

	/**This function grabs current frame.
	* 
	* Implemented in VisageTrackerUnity library.
	*/
	[DllImport ("VisageTrackerUnityPlugin")]
	public static extern void _grabFrame();
	
	[DllImport ("VisageTrackerUnityPlugin")]
	public static extern void _bindTexture(int texID);

	/** This functions binds a texture with the given native hardware texture id through metal (used for iOS).
     * 
     * Implemented in VisageTrackerUnityPlugin library.
     */
	[DllImport ("VisageTrackerUnityPlugin")]
	public static extern void _bindTextureMetal (IntPtr texID);
	
	[DllImport ("VisageTrackerUnityPlugin")]
	public static extern int _track();
	
	
	
	[DllImport ("VisageTrackerUnityPlugin")]
	public static extern bool _getFaceModel(
		out int vertexNumber, 
		[MarshalAs(UnmanagedType.LPArray, SizeConst = 1024 * 3)] 
		float[] vertices,
		out int triangleNumber,
		[MarshalAs(UnmanagedType.LPArray, SizeConst = 2048 * 3)]
		int[] triangles,
		[MarshalAs(UnmanagedType.LPArray, SizeConst = 1024 * 2)]
		float[] texCoords);
	
	[DllImport ("VisageTrackerUnityPlugin")]
	public static extern bool _isAutoStopped();
#endif
	
}