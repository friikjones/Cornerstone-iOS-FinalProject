using DungeonArchitect.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonArchitect.Builders.Snap.SideScroller
{
    public class SnapSideScrollerBuilder : SnapBuilder
    {
        protected override Matrix4x4[] FindAttachmentTransforms(ref Matrix4x4 ParentModuleTransform, ref Matrix4x4 IncomingDoorTransform, ref Matrix4x4 AttachmentDoorTransform)
        {
            var result = new List<Matrix4x4>();

            // Calculate the translation
            {
                Vector3 DesiredOffset;
                Vector3 IncomingDoorPosition = Matrix.GetTranslation(ref IncomingDoorTransform);
                IncomingDoorPosition = ParentModuleTransform.MultiplyPoint3x4(IncomingDoorPosition);
                Vector3 ClampTarget = IncomingDoorPosition;

                Vector3 LocalDoorPosition = Matrix.GetTranslation(ref AttachmentDoorTransform);
                DesiredOffset = ClampTarget - LocalDoorPosition;
                result.Add(Matrix4x4.TRS(DesiredOffset, Quaternion.identity, Vector3.one));
            }

            // Calculate the translation
            {
                /*
                var translationADT = Matrix.GetTranslation(ref AttachmentDoorTransform);
                var rotationADT = Matrix.GetRotation(ref AttachmentDoorTransform);
                var scaleADT = Matrix.GetScale(ref AttachmentDoorTransform);
                
                rotationADT = rotationADT * Quaternion.Euler(0, 180, 0);

                var flippedADT = Matrix4x4.TRS()
                */

                Vector3 DesiredOffset;
                Vector3 IncomingDoorPosition = Matrix.GetTranslation(ref IncomingDoorTransform);
                IncomingDoorPosition = ParentModuleTransform.MultiplyPoint3x4(IncomingDoorPosition);
                Vector3 ClampTarget = IncomingDoorPosition;

                Vector3 LocalDoorPosition = Matrix.GetTranslation(ref AttachmentDoorTransform);
                LocalDoorPosition.x *= -1;
                DesiredOffset = ClampTarget - LocalDoorPosition;
                result.Add(Matrix4x4.TRS(DesiredOffset, Quaternion.identity, new Vector3(-1, 1, 1)));
            }

            return result.ToArray();
        }
    }
}
