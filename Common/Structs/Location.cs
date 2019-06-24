using System;
using Common.Constants;
using Common.Extensions;
using Common.Interfaces;

namespace Common.Structs
{
    public class Location : ICloneable
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float O { get; set; }
        public uint Map { get; set; }
        public string Description { get; set; }
        public Expansions Expansion { get; set; }

        private readonly string formattedDesc = "";

        #region Constructors

        public Location() { }

        public Location(float x, float y, float z, float o, uint map, string description = "", Expansions expansion = Expansions.PreRelease)
        {
            X = x;
            Y = y;
            Z = z;
            O = o;
            Map = map;
            Description = description;
            Expansion = expansion;

            formattedDesc = description.Sanitize();
        }

        #endregion

        #region Update

        public void Update(float x, float y, float z) => Update(x, y, z, O);

        public void Update(float x, float y, float z, float o)
        {
            X = x;
            Y = y;
            Z = z;
            O = o;
        }

        public void Update(IPacketReader packet, bool orientation = false)
        {
            X = packet.ReadFloat();
            Y = packet.ReadFloat();
            Z = packet.ReadFloat();
            if (orientation)
                O = packet.ReadFloat();
        }

        #endregion

        #region Helpers

        internal bool IsMatch(string needle, Expansions expansion, bool exact = false)
        {
            if (Expansion > expansion)
                return false;

            if (exact)
                return formattedDesc.Equals(needle, StringComparison.OrdinalIgnoreCase);
            else
                return formattedDesc.IndexOf(needle, StringComparison.OrdinalIgnoreCase) != -1;
        }


        public override string ToString() => $"X: {X}, Y: {Y}, Z: {Z}, O: {O}, Map: {Map}";

        public object Clone() => MemberwiseClone();

        #endregion

    }
}
