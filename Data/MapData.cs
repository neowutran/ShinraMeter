using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Data 
{
    public class MapData
    {
        public Dictionary<uint, World> Worlds;
        public Dictionary<uint, string> Names;
        public MapData(BasicTeraData basicData, string region)
        {
            Worlds = new Dictionary<uint, World>();
            Names = new Dictionary<uint, string>();

            string path = Path.Combine(basicData.ResourceDirectory, $"data/world_map/world_map-{region}.xml");
            var xdoc = XDocument.Load(path);

            foreach (var w in xdoc.Descendants().Where(x => x.Name == "World"))
            {
                var wId = uint.Parse(w.Attribute("id").Value);
                var wNameId = w.Attribute("nameId") != null ? uint.Parse(w.Attribute("nameId").Value) : 0;
                var world = new World(wId, wNameId);

                foreach (var g in w.Descendants().Where(x => x.Name == "Guard"))
                {
                    var gId = uint.Parse(g.Attribute("id").Value);
                    var gNameId = g.Attribute("nameId") != null ? uint.Parse(g.Attribute("nameId").Value) : 0;
                    var gMapId = g.Attribute("mapId") != null ? g.Attribute("mapId").Value : "";
                    var gTop = g.Attribute("top") != null ? Double.Parse(g.Attribute("top").Value, CultureInfo.InvariantCulture) : 0;
                    var gLeft = g.Attribute("left") != null ? Double.Parse(g.Attribute("left").Value, CultureInfo.InvariantCulture) : 0;
                    var gWidth = g.Attribute("width") != null ? Double.Parse(g.Attribute("width").Value, CultureInfo.InvariantCulture) : 0;
                    var gHeight = g.Attribute("height") != null ? Double.Parse(g.Attribute("height").Value, CultureInfo.InvariantCulture) : 0;

                    var guard = new Guard(gId, gNameId, gMapId, gLeft, gTop, gWidth, gHeight);

                    foreach (var s in g.Descendants().Where(x => x.Name == "Section"))
                    {
                        var sId = uint.Parse(s.Attribute("id").Value);
                        var sNameId = s.Attribute("nameId") != null ? uint.Parse(s.Attribute("nameId").Value) : 0;
                        var sTop = s.Attribute("top") != null ? Double.Parse(s.Attribute("top").Value, CultureInfo.InvariantCulture) : 0;
                        var sLeft = s.Attribute("left") != null ? Double.Parse(s.Attribute("left").Value, CultureInfo.InvariantCulture) : 0;
                        var sWidth = s.Attribute("width") != null ? Double.Parse(s.Attribute("width").Value, CultureInfo.InvariantCulture) : 0;
                        var sHeight = s.Attribute("height") != null ? Double.Parse(s.Attribute("height").Value, CultureInfo.InvariantCulture) : 0;
                        var sMapId = s.Attribute("mapId") != null ? s.Attribute("mapId").Value : "";
                        var dg = s.Attribute("type") != null && s.Attribute("type").Value == "dungeon" ? true : false;
                        var cId = s.Descendants().Any()? uint.Parse(s.Descendants().FirstOrDefault(x => x.Name == "Npc").Attribute("continentId").Value) : 0;

                        var section = new Section(sId, sNameId, sMapId, sTop, sLeft, sWidth, sHeight, dg);
                        if (guard.ContinentId == 0) guard.ContinentId = cId;
                        guard.Sections.Add(sId, section);
                    }
                    world.Guards.Add(guard.Id, guard);
                }
                Worlds.Add(world.Id, world);
            }
            LoadNames(basicData, region);
            LoadImages(basicData);
        }


        internal bool GetDungeon(Location loc)
        {
            if (loc.World == 9999) return true;
            return Worlds[loc.World].Guards[loc.Guard].Sections[loc.Section].IsDungeon;
        }


        void LoadNames(BasicTeraData basicData, string region)
        {
            var f = File.OpenText(Path.Combine(basicData.ResourceDirectory, $"data/regions/regions-{region}.tsv"));
            while (true)
            {
                var line = f.ReadLine();
                if (line == null) break;

                var s = line.Split('\t');

                var id = Convert.ToUInt32(s[0]);
                var name = s[1];

                Names.Add(id, name);
            }

        }

        void LoadImages(BasicTeraData basicData)
        {
            string path = Path.Combine(basicData.ResourceDirectory, "data/section_images.tsv");
            
            var lines = File.ReadLines(path);
            var listOfParts = lines.Select(s => s.Split('\t'));
            foreach (var parts in listOfParts)
            {
                World world = Worlds[uint.Parse(parts[0])];
                Guard guard = world.Guards[uint.Parse(parts[1])];
                Section section = guard.Sections[uint.Parse(parts[2])];

                section.ImageName = parts[3];
            }
        }

        public string GetMapId(uint w, uint g, uint s)
        {
            return Worlds[w].Guards[g].Sections[s].MapId;
        }

        public string GetGuardName(Location loc)
        {
            var guard = loc == null ? null : Worlds[loc.World]?.Guards[loc.Guard];
            if (guard == null) { return null; }

            Names.TryGetValue(guard.NameId, out var name);
            return name;
        }
        
        public string GetSectionName(Location loc)
        {
            var section = loc == null ? null : Worlds[loc.World]?.Guards[loc.Guard]?.Sections[loc.Section];
            if (section == null) { return null; }

            Names.TryGetValue(section.NameId, out var name);
            return name;
        }

        public string GetFullName(Location loc)
        {
            if (loc == null) return null;
            var guardName = GetGuardName(loc);
            var sectionName = GetSectionName(loc);
            
            if (guardName == null) return sectionName;
            
            if (sectionName.StartsWith(guardName)) sectionName = sectionName.Substring(guardName.Length);

            return sectionName == "" ? guardName : $"{guardName} - {sectionName}";
        }

        public string GetImageName(Location loc)
        {
            return loc == null ? null : Worlds[loc.World]?.Guards[loc.Guard]?.Sections[loc.Section]?.ImageName;
        }
    }


}
