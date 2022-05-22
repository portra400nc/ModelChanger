using System;
using UnhollowerRuntimeLib;
using UnityEngine;

using static ModelChanger.Main;

namespace ModelChanger
{
    public class RotationController : MonoBehaviour
    {
        public RotationController(IntPtr ptr) : base(ptr)
        {
        }

        public RotationController() : base(ClassInjector.DerivedConstructorPointer<RotationController>())
        {
            ClassInjector.DerivedConstructorBody(this);
        }
        
        private Transform _t;
        private Quaternion _entityLocalRot;
        private Quaternion _entityRot;
        private const float BodyRotation = 90f;

        public void Awake()
        {
            _t = transform;
            _entityLocalRot = EntityBip.transform.localRotation;
            _entityRot = EntityBip.transform.rotation;
        }

        private void Update()
        {
            _t.localRotation = Quaternion.Euler(BodyRotation, _entityLocalRot.y, _entityLocalRot.z);
            _t.rotation = Quaternion.Euler(BodyRotation, _entityRot.y, _entityRot.z);
        }
    }
}