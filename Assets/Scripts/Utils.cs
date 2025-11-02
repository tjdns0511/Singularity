using UnityEngine;

public static class Utils
{
    public static Vector3Int ToVector3Int(this BlockDirection direction)
    {
        switch (direction)
        {
            case BlockDirection.East:
                return Vector3Int.left; // (-1, 0, 0) -x
            case BlockDirection.West:
                return Vector3Int.right; // (1, 0, 0) +x
            case BlockDirection.South:
                return Vector3Int.back; // (0, 0, -1) -z
            case BlockDirection.North:
                return Vector3Int.forward; // (0, 0, 1) +z
            case BlockDirection.Up:
                return Vector3Int.up; // (0, 1, 0) +y
            case BlockDirection.Down:
                return Vector3Int.down; // (0, -1, 0) -y
            default:
                return Vector3Int.back; //예외 처리
        }
    }


}
