// MRTK GridObjectCollection 대체 구현체
// Meta Quest 전환 시 Microsoft.MixedReality.Toolkit.Utilities 의존성 제거용
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 오브젝트를 Plane / Cylinder / Radial / Sphere 형태로 배열하는 컴포넌트.
/// MRTK GridObjectCollection과 동일한 공개 API를 제공합니다.
/// </summary>
public enum ObjectOrientationSurfaceType
{
    Plane,
    Cylinder,
    Radial,
    Sphere
}

public enum OrientationType
{
    None,
    FaceOrigin,
    FaceOriginReversed,
    FaceFoward,
    FaceBack,
    FaceParentFoward,
    FaceParentBack,
    FaceCenterAxis,
    FaceCenterAxisReversed
}

public class GridObjectCollection : MonoBehaviour
{
    [Header("레이아웃 설정")]
    public ObjectOrientationSurfaceType SurfaceType = ObjectOrientationSurfaceType.Plane;
    public OrientationType OrientType = OrientationType.None;

    [Header("크기/범위")]
    public float Radius = 2f;
    public float RadialRange = 180f;
    public int Rows = 3;
    public float CellWidth = 0.5f;
    public float CellHeight = 0.5f;

    /// <summary>
    /// 현재 SurfaceType 설정에 따라 자식 오브젝트들의 위치/회전을 재계산합니다.
    /// </summary>
    public void UpdateCollection()
    {
        List<Transform> children = new List<Transform>();
        for (int i = 0; i < transform.childCount; i++)
            children.Add(transform.GetChild(i));

        if (children.Count == 0) return;

        int cols = Mathf.CeilToInt((float)children.Count / Mathf.Max(1, Rows));

        for (int i = 0; i < children.Count; i++)
        {
            int row = i / cols;
            int col = i % cols;

            Vector3 pos;
            Quaternion rot;
            CalculatePosRot(row, col, cols, out pos, out rot);

            children[i].localPosition = pos;
            children[i].localRotation = rot;
        }
    }

    private void CalculatePosRot(int row, int col, int totalCols, out Vector3 pos, out Quaternion rot)
    {
        switch (SurfaceType)
        {
            case ObjectOrientationSurfaceType.Plane:
                pos = new Vector3(
                    (col - (totalCols - 1) * 0.5f) * CellWidth,
                    -(row - (Rows - 1) * 0.5f) * CellHeight,
                    0f
                );
                rot = CalcOrientation(OrientType, pos, Vector3.forward);
                break;

            case ObjectOrientationSurfaceType.Cylinder:
            {
                float radRange = RadialRange * Mathf.Deg2Rad;
                float angleStep = (totalCols > 1) ? radRange / (totalCols - 1) : 0f;
                float angle = -radRange * 0.5f + col * angleStep;
                float x = Radius * Mathf.Sin(angle);
                float z = Radius * (1f - Mathf.Cos(angle));
                float y = -(row - (Rows - 1) * 0.5f) * CellHeight;
                pos = new Vector3(x, y, z);
                Vector3 inward = new Vector3(-x, 0f, -z).normalized;
                rot = CalcOrientation(OrientType, pos, inward);
                break;
            }

            case ObjectOrientationSurfaceType.Radial:
            {
                float radRange = RadialRange * Mathf.Deg2Rad;
                float angleStep = (totalCols > 1) ? radRange / (totalCols - 1) : 0f;
                float angle = -radRange * 0.5f + col * angleStep;
                float r = Radius - row * CellHeight;
                float x = r * Mathf.Sin(angle);
                float z = r * Mathf.Cos(angle);
                pos = new Vector3(x, 0f, z);
                rot = CalcOrientation(OrientType, pos, -pos.normalized);
                break;
            }

            case ObjectOrientationSurfaceType.Sphere:
            {
                float radRange = RadialRange * Mathf.Deg2Rad;
                float hStep = (totalCols > 1) ? radRange / (totalCols - 1) : 0f;
                float vStep = (Rows > 1) ? radRange / (Rows - 1) : 0f;
                float angleH = -radRange * 0.5f + col * hStep;
                float angleV = -radRange * 0.5f + row * vStep;
                float x = Radius * Mathf.Sin(angleH) * Mathf.Cos(angleV);
                float y = Radius * Mathf.Sin(angleV);
                float z = Radius * Mathf.Cos(angleH) * Mathf.Cos(angleV);
                pos = new Vector3(x, y, z);
                rot = CalcOrientation(OrientType, pos, -pos.normalized);
                break;
            }

            default:
                pos = Vector3.zero;
                rot = Quaternion.identity;
                break;
        }
    }

    private Quaternion CalcOrientation(OrientationType orientType, Vector3 position, Vector3 inward)
    {
        switch (orientType)
        {
            case OrientationType.FaceOrigin:
                return position != Vector3.zero
                    ? Quaternion.LookRotation(-position.normalized, Vector3.up)
                    : Quaternion.identity;

            case OrientationType.FaceOriginReversed:
                return position != Vector3.zero
                    ? Quaternion.LookRotation(position.normalized, Vector3.up)
                    : Quaternion.identity;

            case OrientationType.FaceParentFoward:
                return Quaternion.LookRotation(transform.forward, Vector3.up);

            case OrientationType.FaceParentBack:
                return Quaternion.LookRotation(-transform.forward, Vector3.up);

            case OrientationType.FaceCenterAxis:
                return inward != Vector3.zero
                    ? Quaternion.LookRotation(inward, Vector3.up)
                    : Quaternion.identity;

            case OrientationType.FaceCenterAxisReversed:
                return inward != Vector3.zero
                    ? Quaternion.LookRotation(-inward, Vector3.up)
                    : Quaternion.identity;

            default:
                return Quaternion.identity;
        }
    }
}
