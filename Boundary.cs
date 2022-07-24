using System;
using System.Collections.Generic;
using System.Text;

namespace FinalTask.Task1
{
    public class Boundary
    {

        public const string EmptySpace = " ";
        public const string NewLine = "\n";
        public const string VerticalBorder = "║";
        public const string HorizontalBorder = "═";

        public readonly string LeftAngle;
        public readonly string MiddleAngle;
        public readonly string RightAngle;

        public static readonly Boundary Top = new Boundary("╔", "╦", "╗");

        public static readonly Boundary Middle = new Boundary("╠", "╬", "╣");

        public static readonly Boundary Bottom = new Boundary("╚", "╩", "╝");

        private Boundary(string leftAngle, string middleAngle, string rightAngle)
        {
            LeftAngle = leftAngle;
            MiddleAngle = middleAngle;
            RightAngle = rightAngle;
        }
    }
}
