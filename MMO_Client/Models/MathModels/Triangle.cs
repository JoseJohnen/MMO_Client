//using Newtonsoft.Json;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MMO_Client.Models.MathModels
{
    public struct Triangle
    {
        double angleA = 0;
        double angleB = 0;
        double angleC = 0;

        double sideA = 0;
        double sideB = 0;
        double sideC = 0;
        
        public Triangle()
        {

        }

        public Triangle(double sideA, double sideB, double sideC, double angleA, double angleB, double angleC)
        {
            this.sideA = sideA;
            this.sideB = sideB;
            this.sideC = sideC;
            this.angleA = angleA;
            this.angleB = angleB;
            this.angleC = angleC;
        }

        public Triangle(double angleA, double angleB, double angleC)
        {
            this.angleA = angleA;
            this.angleB = angleB;
            this.angleC = angleC;
        }

        public double AngleA
        {
            get => angleA;
            set
            {
                if (180 < (value + angleB + angleC) && value > 0)
                {
                    angleA = value;
                }
                else
                {
                    Console.WriteLine("Error Setter AngleA: Value over 180 or under 0");
                }
            }
        }

        public double AngleB
        {
            get => angleB;
            set
            {
                if (180 < (angleA + value + angleC) && value > 0)
                {
                    angleB = value;
                }
                else
                {
                    Console.WriteLine("Error Setter AngleB: Value over 180 or under 0");
                }
            }
        }

        public double AngleC
        {
            get => angleC;
            set
            {
                if (180 < (angleA + angleB + value) && value > 0)
                {
                    angleC = value;
                }
                else
                {
                    Console.WriteLine("Error Setter AngleC: Value over 180 or under 0");
                }
            }
        }

        public double SideA { get => sideA; set => sideA = value; }
        public double SideB { get => sideB; set => sideB = value; }
        public double SideC { get => sideC; set => sideC = value; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
