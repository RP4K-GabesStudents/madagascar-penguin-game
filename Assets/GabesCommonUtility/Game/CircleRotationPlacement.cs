using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Splines;

namespace GabesCommonUtility.Game
{
    [ExecuteAlways]
    public class CircleRotationPlacement : MonoBehaviour
    {
        public Vector3 radius = Vector3.right;
        public Vector3 individualOffset;

        [Header("Angle Settings")]
        public bool useIncrementalAngle;
        [Range(0, 360)] public float totalAngle = 360f;
        public float incrementalAngle = 15f;
        
        [Header("Layout")]
        public Vector3 rotationAxis = Vector3.up;
        public ELayoutMode layoutMode = ELayoutMode.Clockwise;
        public bool faceCenter;
        [Range(-90f, 90f)] public float tilt;

        public enum ELayoutMode { [UsedImplicitly] CounterClockwise, Clockwise, Alternate, Subtract }

        private void OnValidate() => FormatCircle();

        private void OnTransformChildrenChanged()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.delayCall -= FormatCircle;
            UnityEditor.EditorApplication.delayCall += FormatCircle;
#else
            FormatCircle();
#endif
        }

        [ContextMenu("Format Circle")]
        public void FormatCircle()
        {
            if (this == null) return;
            
            int childCount = transform.childCount;
            if (childCount == 0) return;

            float angleStep = GetAngleStep(childCount);
            var calculator = GetAngleCalculator();

            for (int i = 0; i < childCount; i++)
            {
                float angle = calculator(i, angleStep, childCount);
                Transform child = transform.GetChild(i);
                
                Quaternion rot = Quaternion.AngleAxis(angle, rotationAxis);
                Vector3 localPos = rot * radius + individualOffset * i;
                
                child.localPosition = localPos;

                Quaternion finalRot;
                if (faceCenter && localPos != Vector3.zero)
                {
                    finalRot = Quaternion.LookRotation(-localPos, rotationAxis);
                }
                else
                {
                    finalRot = rot;
                }

                child.localRotation = finalRot * Quaternion.AngleAxis(tilt, Vector3.right);
            }
        }

        // --- New Randomization Feature ---
        public void RandomizeChildren()
        {
            int childCount = transform.childCount;
            if (childCount <= 1) return;

            // Gather children into a list
            List<Transform> children = new List<Transform>();
            for (int i = 0; i < childCount; i++)
            {
                children.Add(transform.GetChild(i));
            }

            // Shuffle the list (Fisher-Yates)
            for (int i = childCount - 1; i > 0; i--)
            {
                int rnd = UnityEngine.Random.Range(0, i + 1);
                Transform temp = children[i];
                children[i] = children[rnd];
                children[rnd] = temp;
            }

            // Apply new sibling indices
            for (int i = 0; i < children.Count; i++)
            {
                children[i].SetSiblingIndex(i);
            }

            FormatCircle();
        }

        [ContextMenu("Generate New Spline Object")]
        public void GenerateSpline()
        {
            GameObject splineGo = new GameObject($"{gameObject.name}_GeneratedSpline");
            splineGo.transform.position = transform.position;
            splineGo.transform.rotation = transform.rotation;

            var container = splineGo.AddComponent<SplineContainer>();
            Spline spline = container.Spline;
            
            int childCount = transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                spline.Add(new BezierKnot(transform.GetChild(i).localPosition));
            }

            spline.Closed = !useIncrementalAngle && Mathf.Approximately(totalAngle, 360f);

#if UNITY_EDITOR
            UnityEditor.Undo.RegisterCreatedObjectUndo(splineGo, "Generate Spline Object");
            UnityEditor.Selection.activeGameObject = splineGo;
#endif
        }

        private float GetAngleStep(int count) 
            => (useIncrementalAngle ? incrementalAngle * count : totalAngle) / Mathf.Max(1, count);

        private Func<int, float, int, float> GetAngleCalculator() => layoutMode switch
        {
            ELayoutMode.Alternate => (i, step, _) => step * Mathf.Floor((i + 1) / 2f) * ((i % 2 == 0) ? 1f : -1f),
            ELayoutMode.Subtract  => (i, step, childCount) => step * -i + (step * (childCount-1) / 2.0f),
            ELayoutMode.Clockwise => (i, step, _) => step * -i,
            _                     => (i, step, _) => step * i
        };

        private void OnDrawGizmosSelected()
        {
            int childCount = transform.childCount;
            if (childCount == 0) return;

            Gizmos.color = Color.cyan;
            Gizmos.matrix = transform.localToWorldMatrix;

            float angleStep = GetAngleStep(childCount);
            var calculator = GetAngleCalculator();

            Vector3 previousPoint = Vector3.zero;

            for (int i = 0; i < childCount; i++)
            {
                float angle = calculator(i, angleStep, childCount);
                Vector3 pos = Quaternion.AngleAxis(angle, rotationAxis) * radius + (individualOffset * i);
                
                Gizmos.DrawWireSphere(pos, 0.1f);
                if (i > 0) Gizmos.DrawLine(previousPoint, pos);
                else Gizmos.DrawLine(Vector3.zero, pos); 

                previousPoint = pos;
            }
        }

        public Vector3 GetNextPosition()
        {
            int nextIndex = transform.childCount;
            int predictedTotal = nextIndex + 1;
            float step = GetAngleStep(predictedTotal);
            var calculator = GetAngleCalculator();
            float angle = calculator(nextIndex, step, predictedTotal);

            Quaternion localRot = Quaternion.AngleAxis(angle, rotationAxis);
            Vector3 localPos = (localRot * radius) + (individualOffset * nextIndex);

            return transform.TransformPoint(localPos);
        }

        public Quaternion GetNextRotation()
        {
            int nextIndex = transform.childCount;
            int predictedTotal = nextIndex + 1;
            float step = GetAngleStep(predictedTotal);
            var calculator = GetAngleCalculator();
            float angle = calculator(nextIndex, step, predictedTotal);

            Quaternion baseRot = Quaternion.AngleAxis(angle, rotationAxis);
            
            if (faceCenter)
            {
                Vector3 localPos = (baseRot * radius) + (individualOffset * nextIndex);
                if (localPos != Vector3.zero)
                    baseRot = Quaternion.LookRotation(-localPos, rotationAxis);
            }

            return transform.rotation * baseRot * Quaternion.AngleAxis(tilt, Vector3.right);
        }
    }
}