using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PolyhedraLab
{
    public class SceneObject
    {
        public Polyhedron Model { get; set; }
        public string Name { get; set; }
        public double OffsetX { get; set; }
        public double OffsetY { get; set; }
        public double OffsetZ { get; set; }

        public SceneObject(
            Polyhedron model,
            string name,
            double x = 0,
            double y = 0,
            double z = 0)
        {
            Model = model;
            Name = string.IsNullOrWhiteSpace(name) ? "Объект" : name;
            OffsetX = x;
            OffsetY = y;
            OffsetZ = z;
        }
        public override string ToString()
        {
            return $"{Name} (X={OffsetX:0.##}, Y={OffsetY:0.##}, Z={OffsetZ:0.##})";
        }
    }
}
