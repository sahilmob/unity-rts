using UnityEngine;

namespace RTS.Units
{
    public interface IMovable
    {
        void MoveTo(Vector3 position);
    }
}