using UnityEngine;
using System.Collections;

namespace CLUTCH
{

    public enum KeyInput
    {
        GoLeft = 0,
        GoRight,
        GoUp,
        GoDown,
        Jump,
        Fire,
        Count
    }

    public class GameInput
    {

        public const string HORIZONTAL = "Horizontal";
        public const string VERTICAL = "Vertical";
        public const string JUMP = "Jump";
        public const string FIRE = "Fire";
        public const string SWITCH = "Switch";

    }


    public class Constants
    {
        public const float inputThresh = 0.25f;

        public const float cGravity = -1030.0f;
        public const float cMaxFallingSpeed = -900.0f;
        public const float cWalkSpeed = 150.0f;
        public const float cJumpSpeed = 410.0f;
        public const float cHalfSizesw = 6.0f;
        public const float cHalfSizesh = 18.0f;
        public const int cJumpFramesThreshold = 4;
        public const float cBotMaxPositionError = 1.0f;


        public const KeyCode goLeftKey = KeyCode.A;
        public const KeyCode goRightKey = KeyCode.D;
        public const KeyCode goJumpKey = KeyCode.W;
        public const KeyCode goDownKey = KeyCode.S;
    }
}
