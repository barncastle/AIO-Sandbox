using System;
using System.Text.RegularExpressions;
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

        private readonly string formattedDesc = "";

        #region Constructors

        public Location() { }

        public Location(float x, float y, float z, float o, uint map) : this(x, y, z, o, map, "") { }

        public Location(float x, float y, float z, float o, uint map, string description)
        {
            X = x;
            Y = y;
            Z = z;
            O = o;
            Map = map;
            Description = description;

            if (!string.IsNullOrEmpty(description))
                formattedDesc = Regex.Replace(description, @"[\s',]", "", RegexOptions.Compiled);
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

        public bool HasDescriptionValue(string needle, bool exact)
        {
            if (exact)
                return formattedDesc.Equals(needle, StringComparison.OrdinalIgnoreCase);
            else
                return formattedDesc.IndexOf(needle, StringComparison.OrdinalIgnoreCase) != -1;
        }


        public override string ToString() => $"X: {X}, Y: {Y}, Z: {Z}, O: {O}, Map: {Map}";

        public object Clone() => MemberwiseClone();

    }
}
