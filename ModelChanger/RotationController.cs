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
        private GameObject _npcBip;
        private const float BodyRotation = 90f;

        public void Awake()
        {
            _t = transform;
            _npcBip = EntityBip;
        }

        private void Update()
        {
            var entityT = _npcBip.transform;
            var entityRot = entityT.localRotation;
            _t.localRotation = Quaternion.Euler(BodyRotation, entityRot.y, entityRot.z);
            _t.rotation = Quaternion.Euler(BodyRotation, entityRot.y, entityRot.z);
        }
    }
}