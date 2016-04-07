﻿using UnityEngine;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using LeapInternal;

namespace Leap.Unity.Interaction.CApi {

  public enum ReturnStatus : uint {
    Success,
    InvalidHandle,
    InvalidArgument,
    ReferencesRemain,
    NotEnabled,
    NeverUpdated,
    UnknownError,
    BadData,

    StoppedOnNonDeterministic,
    StoppedOnUnexpectedFailure,
    StoppedOnFull,
    StoppedFileError,
    UnexpectedEOF,
    Paused
  }

  public enum ShapeType : uint {
    Sphere,
    OBB,
    Convex,
    Compound
  }

  public enum SceneInfoFlags : uint {
    None = 0x00,
    HasGravity = 0x01
  };

  public enum ShapeInfoFlags : uint {
    None = 0x00,
    HasRigidBody = 0x01,
    GravityEnabled = 0x02
  };

  public enum UpdateInfoFlags : uint {
    None = 0x00,
    ResetVelocity = 0x01, // E.g. teleported.
    ApplyAcceleration = 0x02
  };

  public enum HandResultFlags : uint {
    ManipulatorMode = 0x01, // currently required
  }

  public enum ManipulatorMode : uint {
    Physics,
    Grasp,
    NoInteraction,
  }

  public enum ShapeInstanceResultFlags : uint {
    None = 0x00,
    Velocities = 0x01,
  }

  public enum DebugFlags : uint {
    None = 0x00,
    Lines = 0x01,
    Logging = 0x02,
    Strings = 0x04
  };

  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct LEAP_COLOR {
    float r;
    float g;
    float b;
    float a;

    public LEAP_COLOR(Color color) {
      r = color.r;
      g = color.g;
      b = color.b;
      a = color.a;
    }

    public Color ToUnityColor() {
      return new Color(r, g, b, a);
    }
  }

  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct INTERACTION_SCENE {
    public IntPtr pScene;
  }

  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct INTERACTION_TRANSFORM {
    public LEAP_VECTOR position;
    public LEAP_QUATERNION rotation;
    public float wallTime;
  }

  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct INTERACTION_SHAPE_DESCRIPTION {
    public ShapeType type;
  }

  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct INTERACTION_SPHERE_DESCRIPTION {
    public INTERACTION_SHAPE_DESCRIPTION shape;
    public float radius;
  }

  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct INTERACTION_OBB_DESCRIPTION {
    public INTERACTION_SHAPE_DESCRIPTION shape;
    public LEAP_VECTOR extents;
  }

  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct INTERACTION_CONVEX_POLYHEDRON_DESCRIPTION {
    public INTERACTION_SHAPE_DESCRIPTION shape;
    public UInt32 nVerticies;
    public LEAP_VECTOR[] pVertices;
    public float radius;
  }

  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct INTERACTION_COMPOUND_DESCRIPTION {
    public INTERACTION_SHAPE_DESCRIPTION shape;
    public UInt32 nShapes;
    public IntPtr pShapes; //LEAP_IE_SHAPE_DESCRIPTION**
    public INTERACTION_TRANSFORM[] pTransforms;
  }

  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct INTERACTION_SHAPE_DESCRIPTION_HANDLE : IEquatable<INTERACTION_SHAPE_DESCRIPTION_HANDLE> {
    public UInt32 handle;

    public bool Equals(INTERACTION_SHAPE_DESCRIPTION_HANDLE other) {
      return handle == other.handle;
    }

    public override int GetHashCode() {
      return (int)handle;
    }
  }

  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct INTERACTION_SHAPE_INSTANCE_HANDLE : IEquatable<INTERACTION_SHAPE_INSTANCE_HANDLE> {
    public UInt32 handle;

    public bool Equals(INTERACTION_SHAPE_INSTANCE_HANDLE other) {
      return handle == other.handle;
    }

    public override int GetHashCode() {
      return (int)handle;
    }
  }

  // All properties require eLeapIESceneFlags to enable
  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct INTERACTION_SCENE_INFO {
    public SceneInfoFlags sceneFlags;
    public LEAP_VECTOR gravity;
  }

  // All properties require eLeapIEShapeFlags to enable
  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct INTERACTION_CREATE_SHAPE_INFO {
    public ShapeInfoFlags shapeFlags;
    public LEAP_VECTOR gravity;
  }

  // All properties require eLeapIEUpdateFlags to enable
  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct INTERACTION_UPDATE_SHAPE_INFO {
    public UpdateInfoFlags updateFlags;
    public LEAP_VECTOR linearAcceleration;
    public LEAP_VECTOR angularAcceleration;
  }

  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct INTERACTION_HAND_RESULT {
    public HandResultFlags handFlags;
    public ManipulatorMode classification;
    public INTERACTION_SHAPE_INSTANCE_HANDLE instanceHandle;
  }

  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct INTERACTION_SHAPE_INSTANCE_RESULTS {
    public INTERACTION_SHAPE_INSTANCE_HANDLE handle;
    public ShapeInstanceResultFlags resultFlags;
    public LEAP_VECTOR linearVelocity;
    public LEAP_VECTOR angularVelocity;
  }

  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct INTERACTION_DEBUG_LINE {
    public LEAP_VECTOR start;
    public LEAP_VECTOR end;
    public LEAP_COLOR color;
    public float duration;
    public int depthTest;

    public void Draw() {
      Debug.DrawLine(start.ToVector3(),
                     end.ToVector3(),
                     color.ToUnityColor(),
                     duration,
                     depthTest != 0);
    }
  }

  public class InteractionC {
    public const string DLL_NAME = "LeapInteractionEngine";

    /*** Create Scene ***/
    [DllImport(DLL_NAME, EntryPoint = "LeapIECreateScene", CallingConvention = CallingConvention.Cdecl)]
    private static extern ReturnStatus LeapIECreateScene(ref INTERACTION_SCENE scene,
                                                         ref INTERACTION_SCENE_INFO sceneInfo,
                                                             string dataPath);

    public static ReturnStatus CreateScene(ref INTERACTION_SCENE scene,
                                           ref INTERACTION_SCENE_INFO sceneInfo,
                                               string dataPath) {
      Logger.Log("Create Scene", LogLevel.Info);
      var rs = LeapIECreateScene(ref scene, ref sceneInfo, dataPath);
      Logger.HandleReturnStatus(rs);
      return rs;
    }

    /*** Destroy Scene ***/
    [DllImport(DLL_NAME, EntryPoint = "LeapIEDestroyScene")]
    private static extern ReturnStatus LeapIEDestroyScene(ref INTERACTION_SCENE scene);

    public static ReturnStatus DestroyScene(ref INTERACTION_SCENE scene) {
      Logger.Log("Destroy Scene", LogLevel.Info);
      var rs = LeapIEDestroyScene(ref scene);
      Logger.HandleReturnStatus(rs);
      return rs;
    }

    /*** Update Scene Info ***/
    [DllImport(DLL_NAME, EntryPoint = "LeapIEUpdateSceneInfo", CallingConvention = CallingConvention.Cdecl)]
    private static extern ReturnStatus LeapIEUpdateSceneInfo(ref INTERACTION_SCENE scene,
                                                             ref INTERACTION_SCENE_INFO sceneInfo);

    public static ReturnStatus UpdateSceneInfo(ref INTERACTION_SCENE scene,
                                               ref INTERACTION_SCENE_INFO sceneInfo) {
      Logger.Log("Update Scene Info", LogLevel.Info);
      var rs = UpdateSceneInfo(ref scene, ref sceneInfo);
      Logger.HandleReturnStatus(rs);
      return rs;
    }

    /*** Get Last Error ***/
    [DllImport(DLL_NAME, EntryPoint = "LeapIEGetLastError")]
    public static extern ReturnStatus GetLastError(ref INTERACTION_SCENE scene);

    /*** Add Shape Description ***/
    [DllImport(DLL_NAME, EntryPoint = "LeapIEAddShapeDescription")]
    private static extern ReturnStatus LeapIEAddShapeDescription(ref INTERACTION_SCENE scene,
                                                                     IntPtr pDescription,
                                                                 out INTERACTION_SHAPE_DESCRIPTION_HANDLE handle);

    public static ReturnStatus AddShapeDescription(ref INTERACTION_SCENE scene,
                                                       IntPtr pDescription,
                                                   out INTERACTION_SHAPE_DESCRIPTION_HANDLE handle) {
      Logger.Log("Add Shape Description", LogLevel.CreateDestroy);
      var rs = LeapIEAddShapeDescription(ref scene, pDescription, out handle);
      Logger.HandleReturnStatus(rs);
      return rs;
    }

    /*** Remove Shape Description ***/
    [DllImport(DLL_NAME, EntryPoint = "LeapIERemoveShapeDescription")]
    private static extern ReturnStatus LeapIERemoveShapeDescription(ref INTERACTION_SCENE scene,
                                                                    ref INTERACTION_SHAPE_DESCRIPTION_HANDLE handle);

    public static ReturnStatus RemoveShapeDescription(ref INTERACTION_SCENE scene,
                                                      ref INTERACTION_SHAPE_DESCRIPTION_HANDLE handle) {
      Logger.Log("Remove Shape Description", LogLevel.CreateDestroy);
      var rs = LeapIERemoveShapeDescription(ref scene, ref handle);
      Logger.HandleReturnStatus(rs);
      return rs;
    }

    /*** Create Shape ***/
    [DllImport(DLL_NAME, EntryPoint = "LeapIECreateShapeInstance")]
    private static extern ReturnStatus LeapIECreateShapeInstance(ref INTERACTION_SCENE scene,
                                                                 ref INTERACTION_SHAPE_DESCRIPTION_HANDLE handle,
                                                                 ref INTERACTION_TRANSFORM transform,
                                                                 ref INTERACTION_CREATE_SHAPE_INFO shapeInfo,
                                                                 out INTERACTION_SHAPE_INSTANCE_HANDLE instance);

    public static ReturnStatus CreateShapeInstance(ref INTERACTION_SCENE scene,
                                                   ref INTERACTION_SHAPE_DESCRIPTION_HANDLE handle,
                                                   ref INTERACTION_TRANSFORM transform,
                                                   ref INTERACTION_CREATE_SHAPE_INFO shapeInfo,
                                                   out INTERACTION_SHAPE_INSTANCE_HANDLE instance) {
      Logger.Log("Create Shap eInstance", LogLevel.CreateDestroy);
      var rs = LeapIECreateShapeInstance(ref scene, ref handle, ref transform, ref shapeInfo, out instance);
      Logger.HandleReturnStatus(rs);
      return rs;
    }

    /*** Destroy Shape Instance ***/
    [DllImport(DLL_NAME, EntryPoint = "LeapIEDestroyShapeInstance")]
    private static extern ReturnStatus LeapIEDestroyShapeInstance(ref INTERACTION_SCENE scene,
                                                          ref INTERACTION_SHAPE_INSTANCE_HANDLE instance);

    public static ReturnStatus DestroyShapeInstance(ref INTERACTION_SCENE scene,
                                            ref INTERACTION_SHAPE_INSTANCE_HANDLE instance) {
      Logger.Log("Destroy Shape Instance", LogLevel.CreateDestroy);
      var rs = LeapIEDestroyShapeInstance(ref scene, ref instance);
      Logger.HandleReturnStatus(rs);
      return rs;
    }

    /*** Update Shape Instance ***/
    [DllImport(DLL_NAME, EntryPoint = "LeapIEUpdateShapeInstance")]
    private static extern ReturnStatus LeapIEUpdateShapeInstance(ref INTERACTION_SCENE scene,
                                                                 ref INTERACTION_TRANSFORM transform,
                                                                 ref INTERACTION_UPDATE_SHAPE_INFO updateInfo,
                                                                 ref INTERACTION_SHAPE_INSTANCE_HANDLE instance);

    public static ReturnStatus UpdateShapeInstance(ref INTERACTION_SCENE scene,
                                                   ref INTERACTION_TRANSFORM transform,
                                                   ref INTERACTION_UPDATE_SHAPE_INFO updateInfo,
                                                   ref INTERACTION_SHAPE_INSTANCE_HANDLE instance) {
      Logger.Log("Update Shape Instance", LogLevel.AllCalls);
      var rs = LeapIEUpdateShapeInstance(ref scene, ref transform, ref updateInfo, ref instance);
      Logger.HandleReturnStatus(rs);
      return rs;
    }

    /*** Update Hands ***/
    [DllImport(DLL_NAME, EntryPoint = "LeapIEUpdateHands")]
    private static extern ReturnStatus LeapIEUpdateHands(ref INTERACTION_SCENE scene,
                                                             UInt32 nHands,
                                                             IntPtr pHands);

    public static ReturnStatus UpdateHands(ref INTERACTION_SCENE scene,
                                               UInt32 nHands,
                                               IntPtr pHands) {
      Logger.Log("Update Hands", LogLevel.AllCalls);
      var rs = LeapIEUpdateHands(ref scene, nHands, pHands);
      Logger.HandleReturnStatus(rs);
      return rs;
    }

    /*** Update Controller ***/
    [DllImport(DLL_NAME, EntryPoint = "LeapIEUpdateController")]
    private static extern ReturnStatus LeapIEUpdateController(ref INTERACTION_SCENE scene,
                                                              ref INTERACTION_TRANSFORM controllerTransform);

    public static ReturnStatus UpdateController(ref INTERACTION_SCENE scene,
                                                ref INTERACTION_TRANSFORM controllerTransform) {
      Logger.Log("Update Controller", LogLevel.AllCalls);
      var rs = LeapIEUpdateController(ref scene, ref controllerTransform);
      Logger.HandleReturnStatus(rs);
      return rs;
    }

    /*** Get Classification ***/
    [DllImport(DLL_NAME, EntryPoint = "LeapIEGetHandResult")]
    private static extern ReturnStatus LeapIEGetHandResult(ref INTERACTION_SCENE scene,
                                                               UInt32 handId,
                                                           out INTERACTION_HAND_RESULT handResult);

    public static ReturnStatus GetHandResult(ref INTERACTION_SCENE scene,
                                                 UInt32 handId,
                                             out INTERACTION_HAND_RESULT handResult) {
      Logger.Log("Get Hand Result", LogLevel.AllCalls);
      var rs = LeapIEGetHandResult(ref scene, handId, out handResult);
      Logger.HandleReturnStatus(rs);
      return rs;
    }

    /*** Override Hand Result ***/
    [DllImport(DLL_NAME, EntryPoint = "LeapIEOverrideHandResult")]
    private static extern ReturnStatus LeapIEOverrideHandResult(ref INTERACTION_SCENE scene,
                                                                   UInt32 handId,
                                                                ref INTERACTION_HAND_RESULT handResult);

    public static ReturnStatus OverrideHandResult(ref INTERACTION_SCENE scene,
                                                      UInt32 handId,
                                                  ref INTERACTION_HAND_RESULT handResult) {
      Logger.Log("Override Hand Result", LogLevel.AllCalls);
      var rs = LeapIEOverrideHandResult(ref scene, handId, ref handResult);
      Logger.HandleReturnStatus(rs);
      return rs;
    }

    /*** Get Velocities ***/
    [DllImport(DLL_NAME, EntryPoint = "LeapIEGetShapeInstanceResults")]
    private static extern ReturnStatus LeapIEGetShapeInstanceResults(ref INTERACTION_SCENE scene,
                                                                     out UInt32 nResults,
                                                                     out IntPtr papResultsBuffer);

    public static ReturnStatus GetVelocities(ref INTERACTION_SCENE scene, 
                                                 List<INTERACTION_SHAPE_INSTANCE_RESULTS> results) {
      Logger.Log("Get Velocities", LogLevel.AllCalls);

      UInt32 nResults;
      IntPtr papResultsBuffer;
      var rs = LeapIEGetShapeInstanceResults(ref scene, 
                                             out nResults, 
                                             out papResultsBuffer);

      results.Clear();
      if (rs == ReturnStatus.Success) {
        for (int i = 0; i < nResults; i++) {
          IntPtr resultPtr = StructMarshal<IntPtr>.ArrayElementToStruct(papResultsBuffer, i);
          var result = StructMarshal<INTERACTION_SHAPE_INSTANCE_RESULTS>.PtrToStruct(resultPtr);
          results.Add(result);
        }
      }

      Logger.HandleReturnStatus(rs);
      return rs;
    }

    /*** Enable Debug Visualization ***/
    [DllImport(DLL_NAME, EntryPoint = "LeapIEEnableDebugFlags")]
    private static extern ReturnStatus LeapIEEnableDebugFlags(ref INTERACTION_SCENE scene,
                                                                  UInt32 flags);

    public static ReturnStatus EnableDebugFlags(ref INTERACTION_SCENE scene,
                                                    UInt32 flags) {
      Logger.Log("Enable Debug Flags", LogLevel.Info);
      var rs = LeapIEEnableDebugFlags(ref scene, flags);
      Logger.HandleReturnStatus(rs);
      return rs;
    }

    /*** Get Debug Lines ***/
    [DllImport(DLL_NAME, EntryPoint = "LeapIEGetDebugLines")]
    private static extern ReturnStatus LeapIEGetDebugLines(ref INTERACTION_SCENE scene,
                                                           out UInt32 nLines,
                                                           out IntPtr ppLineBuffer);

    public static ReturnStatus GetDebugLines(ref INTERACTION_SCENE scene,
                                             out UInt32 nLines,
                                             out IntPtr ppLineBuffer) {
      Logger.Log("Get Debug Lines", LogLevel.AllCalls);
      var rs = LeapIEGetDebugLines(ref scene, out nLines, out ppLineBuffer);
      Logger.HandleReturnStatus(rs);
      return rs;
    }

    public static void DrawDebugLines(ref INTERACTION_SCENE scene) {
      UInt32 lines;
      IntPtr arrayPtr;
      GetDebugLines(ref scene, out lines, out arrayPtr);

      for (int i = 0; i < lines; i++) {
        INTERACTION_DEBUG_LINE line = StructMarshal<INTERACTION_DEBUG_LINE>.ArrayElementToStruct(arrayPtr, i);
        line.Draw();
      }
    }

    /*** Get Debug Strings ***/
    [DllImport(DLL_NAME, EntryPoint = "LeapIEGetDebugStrings")]
    private static extern ReturnStatus LeapIEGetDebugStrings(ref INTERACTION_SCENE scene,
                                                           out UInt32 nStrings,
                                                           out IntPtr pppStrings);

    public static ReturnStatus GetDebugStrings(ref INTERACTION_SCENE scene,
                                                   List<string> strings) {
      Logger.Log("Get Debug Strings", LogLevel.AllCalls);

      UInt32 nStrings;
      IntPtr pppStrings;
      var rs = LeapIEGetDebugStrings(ref scene, out nStrings, out pppStrings);

      strings.Clear();
      if (rs == ReturnStatus.Success) {
        for (int i = 0; i < nStrings; i++) {
          IntPtr charPtr = StructMarshal<IntPtr>.ArrayElementToStruct(pppStrings, i);
          string str = Marshal.PtrToStringAnsi(charPtr);
          strings.Add(str);
        }
      }

      Logger.HandleReturnStatus(rs);
      return rs;
    }
  }
}