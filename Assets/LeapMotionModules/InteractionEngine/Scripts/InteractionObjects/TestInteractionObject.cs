﻿using UnityEngine;
using LeapInternal;
using InteractionEngine.CApi;
using System;

namespace InteractionEngine {

  public class TestInteractionObject : InteractionObject {
    private SphereCollider _sphereCollider;
    private Renderer _renderer;

    public override LEAP_IE_TRANSFORM IeTransform {
      get {
        LEAP_IE_TRANSFORM ieTransform = new LEAP_IE_TRANSFORM();
        ieTransform.position = new LEAP_VECTOR(transform.position);
        ieTransform.rotation = new LEAP_QUATERNION(transform.rotation);
        return ieTransform;
      }
      set {
        transform.position = value.position.ToUnityVector();
        transform.rotation = value.rotation.ToUnityRotation();
      }
    }

    public override void SetClassification(eLeapIEClassification classification) {
      base.SetClassification(classification);
      switch (classification) {
        case eLeapIEClassification.eLeapIEClassification_Physics:
          _renderer.material.color = Color.white;
          break;
        case eLeapIEClassification.eLeapIEClassification_Grasp:
          _renderer.material.color = Color.green;
          break;
      }
    }

    void OnEnable() {
      _sphereCollider = GetComponent<SphereCollider>();
      _renderer = GetComponent<Renderer>();

      if (!HasRegisteredShapeDescription) {
        LEAP_IE_SPHERE_DESCRIPTION sphereDesc = new LEAP_IE_SPHERE_DESCRIPTION();
        sphereDesc.shape.type = eLeapIEShapeType.eLeapIEShape_Sphere;
        sphereDesc.radius = _sphereCollider.radius;
        RegisterShapeDescription(sphereDesc);
      }

      EnableInteraction();
    }

    void OnDisable() {
      DisableInteraction();
    }
  }
}